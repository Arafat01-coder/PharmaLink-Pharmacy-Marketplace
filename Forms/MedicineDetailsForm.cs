using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 23. Everything about one medicine: manufacturer, strength,
    /// expiry date, selling pharmacy, whether a prescription is needed, and the
    /// full list of visible reviews.
    ///
    /// When an offer is running today the original price is struck through and
    /// the discounted price is shown beside it. That number comes from the same
    /// query the cart and the invoice use, so the three can never disagree.
    ///
    /// Add to cart follows Medicine.CanBuy (listed, pharmacy Approved, not
    /// expired). The screen can be opened for a medicine that is no longer
    /// buyable - from an old offer list, for example - so it says why instead
    /// of letting the cart refuse it later.
    /// </summary>
    public partial class MedicineDetailsForm : Form
    {
        private readonly MedicineService _medicines = new MedicineService();
        private readonly ReviewService _reviews = new ReviewService();
        private readonly CartService _cart = new CartService();

        private readonly int _medicineId;
        private Medicine _medicine;

        public MedicineDetailsForm(int medicineId)
        {
            InitializeComponent();
            _medicineId = medicineId;
        }

        private void MedicineDetailsForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            try
            {
                LoadMedicine();
                if (_medicine == null) return;   // LoadMedicine has already closed the form
                LoadReviews();
            }
            catch (Exception ex)
            {
                MessageBox.Show("This medicine could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Medicine Details");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblMedicineName, lblGenericName);
            lblMedicineName.Font = UiTheme.FontTitleLarge;

            foreach (GroupBox group in new[] { grpFacts, grpPrice })
            {
                group.Font = UiTheme.FontHeading;
                group.ForeColor = UiTheme.Primary;
                group.BackColor = UiTheme.CardBack;
            }

            foreach (Label caption in new[] { lblManufacturerCaption, lblStrengthCaption, lblCategoryCaption,
                                              lblExpiryCaption, lblPharmacyCaption, lblStockCaption })
            {
                caption.Font = UiTheme.FontCaption;
                caption.ForeColor = UiTheme.TextMuted;
            }

            foreach (Label value in new[] { lblManufacturer, lblStrength, lblCategory,
                                            lblExpiry, lblPharmacy, lblStock })
            {
                value.Font = UiTheme.FontValue;
                value.ForeColor = UiTheme.TextDark;
            }

            lblDescription.Font = UiTheme.FontBody;
            lblDescription.ForeColor = UiTheme.TextMuted;
            lblRxBadge.Font = UiTheme.FontBadge;

            lblReviewsTitle.Font = UiTheme.FontHeading;
            lblReviewsTitle.ForeColor = UiTheme.TextDark;
            lblAverageRating.Font = UiTheme.FontSubheading;
            lblAverageRating.ForeColor = UiTheme.TextDark;
            lblReviewNote.Font = UiTheme.FontSmall;
            lblReviewNote.ForeColor = UiTheme.TextMuted;
            lblAddMessage.Font = UiTheme.FontSmall;

            // The price fonts never change, so they are set once here rather
            // than every time ShowPrice runs.
            lblOriginalPrice.Font = UiTheme.FontStrikePrice;
            lblOriginalPrice.ForeColor = UiTheme.TextMuted;
            lblFinalPrice.Font = UiTheme.FontPriceHero;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StylePrimary(btnAddToCart);
            btnAddToCart.Font = UiTheme.FontButtonStrong;
            UiTheme.StyleGrid(dgvReviews);
            UiTheme.EnableEmptyMessage(dgvReviews, "Nobody has reviewed this medicine yet.");
            dgvReviews.DataBindingComplete += dgvReviews_DataBindingComplete;
        }

        private void LoadMedicine()
        {
            _medicine = _medicines.GetDetails(_medicineId);

            if (_medicine == null)
            {
                MessageBox.Show("That medicine is no longer available.", "PharmaLink",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                return;
            }

            lblMedicineName.Text = _medicine.MedicineName + "  " + _medicine.Strength;
            lblGenericName.Text = "Generic name: " + _medicine.GenericName;

            lblManufacturer.Text = _medicine.Manufacturer;
            lblStrength.Text = string.IsNullOrWhiteSpace(_medicine.Strength) ? "-" : _medicine.Strength;
            lblCategory.Text = _medicine.CategoryName;
            lblExpiry.Text = _medicine.ExpiryDate.ToString("dd MMM yyyy");
            lblPharmacy.Text = _medicine.PharmacyName + "   (" + _medicine.Area + ")";

            lblDescription.Text = string.IsNullOrWhiteSpace(_medicine.Description)
                ? "" : _medicine.Description;

            if (_medicine.RequiresRx)
            {
                lblRxBadge.Text = "  Rx  -  prescription only. You will be asked for a photo of your " +
                                  "doctor's prescription at checkout.";
                lblRxBadge.ForeColor = UiTheme.Warning;
            }
            else
            {
                lblRxBadge.Text = "  Over the counter  -  no prescription needed.";
                lblRxBadge.ForeColor = UiTheme.Success;
            }

            ShowPrice();
            ShowBuyState();
        }

        /// <summary>
        /// The stock line and the Add button, from the current _medicine. Called
        /// on load and again after an add, when the medicine has been re-read.
        /// </summary>
        private void ShowBuyState()
        {
            lblStock.Text = _medicine.Stock > 0
                ? _medicine.Stock + " unit(s) on the shelf"
                : "Out of stock";
            lblStock.ForeColor = _medicine.Stock > 0 ? UiTheme.Success : UiTheme.Danger;

            string reason = WhyNotBuyable();
            bool canAdd = reason == null && _medicine.Stock > 0;

            btnAddToCart.Enabled = canAdd;
            txtQuantity.Enabled = canAdd;
            btnAddToCart.Text = reason != null ? "Not available" : _medicine.Stock > 0 ? "Add to cart" : "Out of stock";

            if (reason != null) ShowAddMessage(reason, UiTheme.Danger);
        }

        /// <summary>A plain reason when CanBuy is false, or null when the medicine can be bought.</summary>
        private string WhyNotBuyable()
        {
            if (_medicine.CanBuy) return null;
            if (!_medicine.IsActive) return "This medicine is no longer sold by this pharmacy.";
            if (_medicine.ExpiryDate.Date <= DateTime.Today) return "This medicine has passed its expiry date and cannot be sold.";
            return "This pharmacy is not taking orders at the moment.";
        }

        private void ShowPrice()
        {
            if (_medicine.DiscountPercent > 0m)
            {
                // Struck through original beside the discounted price.
                lblOriginalPrice.Text = UiTheme.Money(_medicine.UnitPrice);

                lblFinalPrice.Text = UiTheme.Money(_medicine.PriceAfterDiscount);
                lblFinalPrice.ForeColor = UiTheme.Success;

                decimal saving = _medicine.UnitPrice - _medicine.PriceAfterDiscount;
                lblDiscountBadge.Text = _medicine.DiscountPercent.ToString("0.##") + "% off today  -  " +
                                        "you save " + UiTheme.Money(saving) + " per unit.";
                lblDiscountBadge.ForeColor = UiTheme.Success;
                lblDiscountBadge.Font = UiTheme.FontBadge;
            }
            else
            {
                lblOriginalPrice.Text = "";
                lblFinalPrice.Text = UiTheme.Money(_medicine.UnitPrice);
                lblFinalPrice.ForeColor = UiTheme.TextDark;

                lblDiscountBadge.Text = "No offer is running on this medicine today.";
                lblDiscountBadge.ForeColor = UiTheme.TextMuted;
                lblDiscountBadge.Font = UiTheme.FontSmall;
            }
        }

        private void LoadReviews()
        {
            DataTable table = _reviews.GetForMedicine(_medicineId);
            dgvReviews.DataSource = table;

            if (dgvReviews.Columns.Count > 0)
            {
                dgvReviews.Columns["ReviewId"].Visible = false;
                dgvReviews.Columns["ReviewerName"].HeaderText = "Reviewer";
                dgvReviews.Columns["ReviewerName"].FillWeight = 55;
                dgvReviews.Columns["Rating"].HeaderText = "Stars";
                dgvReviews.Columns["Rating"].FillWeight = 25;
                dgvReviews.Columns["Comment"].HeaderText = "Comment";
                dgvReviews.Columns["Comment"].FillWeight = 180;
                dgvReviews.Columns["ReviewDate"].HeaderText = "Written on";
                dgvReviews.Columns["ReviewDate"].FillWeight = 50;
            }

            decimal average = _reviews.GetAverageForMedicine(_medicineId);

            lblAverageRating.Text = table.Rows.Count == 0
                ? "No reviews yet"
                : average.ToString("N2") + " / 5   from " + table.Rows.Count + " review(s)";

            lblReviewsTitle.Text = table.Rows.Count == 0
                ? "What other patients said  -  nobody has reviewed this yet"
                : "What other patients said";
        }

        /// <summary>Low ratings red, high ratings green; set once per binding, not on every paint.</summary>
        private void dgvReviews_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvReviews.Columns.Count == 0) return;

            foreach (DataGridViewRow row in dgvReviews.Rows)
            {
                object rating = row.Cells["Rating"].Value;
                if (rating == null || rating == DBNull.Value) continue;

                int stars = Convert.ToInt32(rating);
                if (stars <= 2) row.DefaultCellStyle.BackColor = UiTheme.LowStockBack;
                else if (stars >= 4) row.DefaultCellStyle.BackColor = UiTheme.DeliveredBack;
                else row.DefaultCellStyle.BackColor = Color.Empty;
            }
        }

        // ---------------------------------------------------------------------

        private void ShowAddMessage(string text, Color colour)
        {
            lblAddMessage.Text = text;
            lblAddMessage.ForeColor = colour;
            lblAddMessage.Visible = true;
        }

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            if (_medicine == null) return;

            int quantity;
            if (!Validator.IsPositiveInt(txtQuantity.Text, out quantity))
            {
                UiTheme.ShowError(lblAddMessage, txtQuantity, "Enter a whole quantity of one or more.");
                return;
            }

            string message;
            bool added;
            try
            {
                added = _cart.AddOrIncrease(UserSession.UserId, _medicineId, quantity, out message);
            }
            catch (Exception ex)
            {
                ShowAddMessage(DbHelper.Describe(ex), UiTheme.Danger);
                return;
            }

            if (!added)
            {
                ShowAddMessage(message, UiTheme.Danger);
                return;
            }

            UiTheme.ClearError(lblAddMessage, txtQuantity);

            // Re-read the medicine so the stock line reflects everyone's
            // purchases, not the figure from when the screen opened.
            try
            {
                Medicine fresh = _medicines.GetDetails(_medicineId);
                if (fresh != null)
                {
                    _medicine = fresh;
                    ShowBuyState();
                }
            }
            catch
            {
                // The add itself succeeded; a stale stock line is not worth an error.
            }

            ShowAddMessage(quantity + " added to your cart.", UiTheme.Success);
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
