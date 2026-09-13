using System.Data;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 29. Every offer that is actually running today.
    ///
    /// The query returns only offers where today falls between StartDate and
    /// EndDate, on a medicine a customer can buy right now, at the same best
    /// discount the cart applies. Expired offers are excluded by the database
    /// rather than by this form, so a stale discount can never be displayed by
    /// mistake.
    ///
    /// Problems are reported in the status line under the grid instead of a
    /// message box, so a wrong quantity does not interrupt browsing.
    /// </summary>
    public partial class CustomerOffersForm : Form
    {
        private readonly OfferService _offers = new OfferService();
        private readonly CategoryService _categories = new CategoryService();
        private readonly PharmacyService _pharmacies = new PharmacyService();
        private readonly CartService _cart = new CartService();

        private bool _loading = true;

        /// <summary>True while the grid is being rebound, so SelectionChanged is ignored.</summary>
        private bool _binding;

        public CustomerOffersForm()
        {
            InitializeComponent();
        }

        private void CustomerOffersForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            UiTheme.MakeResizable(this, Size);

            try
            {
                cmbCategory.Items.Add("All categories");
                foreach (Category category in _categories.GetActiveList())
                    cmbCategory.Items.Add(category);   // shows the name; the object carries the id
                cmbCategory.SelectedIndex = 0;

                cmbArea.Items.Add("All areas");
                foreach (string area in _pharmacies.GetAreas(true)) cmbArea.Items.Add(area);
                cmbArea.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError("The filters could not be loaded. " + DbHelper.Describe(ex));
            }

            lblToday.Text = "Showing offers valid on " + DateTime.Today.ToString("dd MMM yyyy");

            _loading = false;
            LoadGrid();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Offers");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            lblToday.Font = UiTheme.FontSmall;
            lblToday.ForeColor = UiTheme.TextMuted;
            lblNote.Font = UiTheme.FontSmall;
            lblNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSecondary(btnRefresh);
            UiTheme.StylePrimary(btnAddToCart);
            UiTheme.StyleAccent(btnViewDetails);
            UiTheme.StyleGrid(dgvOffers);
            UiTheme.EnableEmptyMessage(dgvOffers, "No offers are running today for these filters.");
        }

        private int SelectedCategoryId()
        {
            return cmbCategory.SelectedItem is Category category ? category.CategoryId : 0;
        }

        private void ShowError(string text)
        {
            lblStatus.ForeColor = UiTheme.Danger;
            lblStatus.Text = text;
        }

        private void ShowInfo(string text, bool success)
        {
            lblStatus.ForeColor = success ? UiTheme.Success : UiTheme.TextMuted;
            lblStatus.Text = text;
        }

        // ---------------------------------------------------------------------

        /// <summary>Rebinds the grid on the same medicine, then shows <paramref name="successMessage"/> if given.</summary>
        private void LoadGrid(string successMessage = null)
        {
            if (_loading) return;

            try
            {
                int keepMedicineId = SelectedMedicineId();
                string area = cmbArea.SelectedIndex <= 0 ? "" : cmbArea.SelectedItem.ToString();
                DataTable table = _offers.GetActiveOffers(SelectedCategoryId(), area);

                _binding = true;
                try
                {
                    dgvOffers.DataSource = table;
                    LabelColumns();
                    SelectMedicine(keepMedicineId);

                    // Hiding MedicineId in LabelColumns can clear the current cell;
                    // restore it so Add to cart works without clicking a row first.
                    UiTheme.EnsureCurrentCell(dgvOffers, "MedicineName");
                }
                finally
                {
                    _binding = false;
                }

                if (successMessage != null)
                    ShowInfo(successMessage, true);
                else
                    ShowInfo(table.Rows.Count == 0
                        ? "No offer is running today for that combination of filters."
                        : table.Rows.Count + " offer(s) running today. The discounted price stays with the item " +
                          "all the way to your invoice.", false);

                UpdateButtons();
            }
            catch (Exception ex)
            {
                ShowError("The offers could not be loaded. " + DbHelper.Describe(ex));
            }
        }

        private void LabelColumns()
        {
            Label("OfferId", "ID", 26, 40);
            Label("OfferTitle", "Offer", 130, 150);
            Label("MedicineName", "Medicine", 90, 100);
            Label("Strength", "Strength", 42, 60);
            Label("CategoryName", "Category", 70, 80);
            Label("PharmacyName", "Sold by", 90, 110);
            Label("Area", "Area", 42, 60);
            Label("OriginalPrice", "Was (Tk)", 44, 60);
            Label("DiscountPercent", "Off %", 32, 45);
            Label("DiscountedPrice", "You pay (Tk)", 50, 70);
            Label("YouSave", "Save (Tk)", 44, 60);
            Label("EndDate", "Valid until", 60, 85);
            Label("Stock", "Stock", 34, 48);

            if (dgvOffers.Columns.Contains("MedicineId")) dgvOffers.Columns["MedicineId"].Visible = false;
            if (dgvOffers.Columns.Contains("DiscountPercent"))
                dgvOffers.Columns["DiscountPercent"].DefaultCellStyle.Format = "0.##";
            if (dgvOffers.Columns.Contains("EndDate"))
                dgvOffers.Columns["EndDate"].DefaultCellStyle.Format = "dd MMM yyyy";
        }

        /// <summary>Header text and width for a column, if the query returned it.</summary>
        private void Label(string column, string header, float fillWeight, int minimumWidth)
        {
            if (!dgvOffers.Columns.Contains(column)) return;
            dgvOffers.Columns[column].HeaderText = header;
            UiTheme.SizeColumn(dgvOffers, column, fillWeight, minimumWidth);
        }

        private void SelectMedicine(int medicineId)
        {
            if (medicineId <= 0) return;
            foreach (DataGridViewRow row in dgvOffers.Rows)
            {
                if (Convert.ToInt32(row.Cells["MedicineId"].Value) == medicineId)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.Visible)
                        {
                            dgvOffers.CurrentCell = cell;
                            return;
                        }
                    }
                }
            }
        }

        private void dgvOffers_SelectionChanged(object sender, EventArgs e)
        {
            if (_binding) return;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool hasRow = SelectedMedicineId() > 0;
            btnAddToCart.Enabled = hasRow;
            btnViewDetails.Enabled = hasRow;
        }

        private int SelectedMedicineId()
        {
            if (dgvOffers.CurrentRow == null || !dgvOffers.Columns.Contains("MedicineId")) return 0;
            object value = dgvOffers.CurrentRow.Cells["MedicineId"].Value;
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        // ---------------------------------------------------------------------

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            int medicineId = SelectedMedicineId();
            if (medicineId == 0) return;

            int quantity;
            if (!Validator.IsPositiveInt(txtQuantity.Text, out quantity))
            {
                ShowError("Enter a whole quantity of one or more.");
                txtQuantity.Focus();
                txtQuantity.SelectAll();
                return;
            }

            string name = Convert.ToString(dgvOffers.CurrentRow.Cells["MedicineName"].Value);
            string save = dgvOffers.Columns.Contains("YouSave")
                ? Convert.ToString(dgvOffers.CurrentRow.Cells["YouSave"].Value) : "";

            string message;
            bool added;
            try
            {
                added = _cart.AddOrIncrease(UserSession.UserId, medicineId, quantity, out message);
            }
            catch (Exception ex)
            {
                ShowError("That could not be added to your cart. " + DbHelper.Describe(ex));
                return;
            }

            if (!added)
            {
                ShowError(message);
                return;
            }

            txtQuantity.Text = "1";
            LoadGrid(quantity + " x " + name + " added to your cart at the offer price." +
                     (save.Length > 0 ? " You are saving Tk " + save + " per unit." : ""));
        }

        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            int medicineId = SelectedMedicineId();
            if (medicineId == 0) return;

            using (MedicineDetailsForm details = new MedicineDetailsForm(medicineId))
            {
                details.ShowDialog(this);
            }
            LoadGrid();
        }

        private void dgvOffers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnViewDetails_Click(sender, EventArgs.Empty);
        }

        private void Filter_Changed(object sender, EventArgs e) => LoadGrid();
        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
