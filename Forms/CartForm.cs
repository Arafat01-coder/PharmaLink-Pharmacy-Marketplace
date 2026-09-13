using System.Configuration;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 24. The customer's basket.
    ///
    /// Every price shown here is calculated by the query with today's offer
    /// already applied, so the cart and the checkout can never disagree.
    /// A basket that spans two pharmacies becomes two orders at checkout, each
    /// with its own delivery charge and its own invoice, and the summary panel
    /// says so before the customer commits.
    ///
    /// The summary figures (items, discount, delivery, payable) are all added up
    /// from ONE query - CartService.GetPharmacyGroups - so they can never
    /// contradict each other.
    /// </summary>
    public partial class CartForm : Form
    {
        private readonly CartService _cart = new CartService();

        /// <summary>True while the grid is being rebound, so SelectionChanged is ignored.</summary>
        private bool _binding;

        /// <summary>The line whose quantity is in the box; the box is only refilled when this changes.</summary>
        private int _quantityShownFor;

        public CartForm()
        {
            InitializeComponent();
        }

        private void CartForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            UiTheme.MakeResizable(this, Size);
            LoadCart();
        }

        private static decimal DeliveryCharge
        {
            get
            {
                string configured = ConfigurationManager.AppSettings["DeliveryCharge"];
                decimal value;
                return decimal.TryParse(configured, out value) ? value : 60m;
            }
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "My Cart");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            grpSummary.Font = UiTheme.FontHeading;
            grpSummary.ForeColor = UiTheme.Primary;
            grpSummary.BackColor = UiTheme.CardBack;

            foreach (Label caption in new[] { lblItemsCaption, lblDiscountCaption, lblDeliveryCaption })
            {
                caption.Font = UiTheme.FontBody;
                caption.ForeColor = UiTheme.TextMuted;
            }

            foreach (Label value in new[] { lblItemsValue, lblDiscountValue, lblDeliveryValue })
            {
                value.Font = UiTheme.FontBody;
                value.ForeColor = UiTheme.TextDark;
            }

            lblPayableCaption.Font = UiTheme.FontHeading;
            lblPayableCaption.ForeColor = UiTheme.TextDark;
            lblPayableValue.Font = UiTheme.FontTileValue;
            lblPayableValue.ForeColor = UiTheme.Primary;

            lblSummaryNote.Font = UiTheme.FontSmall;
            lblSummaryNote.ForeColor = UiTheme.TextMuted;
            lblSplitNote.Font = UiTheme.FontSmall;
            lblSplitNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StylePrimary(btnUpdateQuantity);
            UiTheme.StyleDanger(btnRemoveLine);
            UiTheme.StyleSecondary(btnClearCart);
            UiTheme.StyleSuccess(btnCheckout);
            btnCheckout.Font = UiTheme.FontButtonLarge;
            UiTheme.StyleSecondary(btnContinueShopping);

            UiTheme.StyleGrid(dgvCart);
            UiTheme.EnableEmptyMessage(dgvCart, "Your cart is empty. Browse the catalogue and add something to it.");
            dgvCart.DataBindingComplete += dgvCart_DataBindingComplete;
        }

        // ---------------------------------------------------------------------

        /// <summary>
        /// Reloads the basket and the summary. The same line stays selected
        /// (or <paramref name="selectMedicineId"/> when given), and
        /// <paramref name="statusMessage"/> is shown after the reload so the
        /// reload cannot overwrite it.
        /// </summary>
        private void LoadCart(string statusMessage = null, int selectMedicineId = 0)
        {
            try
            {
                if (selectMedicineId == 0) selectMedicineId = SelectedMedicineId();

                DataTable lines = _cart.GetLinesTable(UserSession.UserId);
                DataTable groups = _cart.GetPharmacyGroups(UserSession.UserId, DeliveryCharge);

                _binding = true;
                try
                {
                    dgvCart.DataSource = lines;

                    if (dgvCart.Columns.Count > 0)
                    {
                        dgvCart.Columns["CartId"].Visible = false;
                        dgvCart.Columns["MedicineId"].HeaderText = "ID";
                        dgvCart.Columns["MedicineName"].HeaderText = "Medicine";
                        dgvCart.Columns["Strength"].HeaderText = "Strength";
                        dgvCart.Columns["PharmacyId"].Visible = false;
                        dgvCart.Columns["PharmacyName"].HeaderText = "Sold by";
                        dgvCart.Columns["Quantity"].HeaderText = "Qty";
                        dgvCart.Columns["ListPrice"].HeaderText = "List (Tk)";
                        dgvCart.Columns["DiscountPercent"].HeaderText = "Off %";
                        dgvCart.Columns["DiscountPercent"].DefaultCellStyle.Format = "0.##";
                        dgvCart.Columns["PriceYouPay"].HeaderText = "You pay (Tk)";
                        dgvCart.Columns["LineTotal"].HeaderText = "Line total (Tk)";
                        dgvCart.Columns["Stock"].HeaderText = "In stock";
                        dgvCart.Columns["RequiresRx"].HeaderText = "Rx";
                        dgvCart.Columns["CanBuy"].Visible = false;

                        UiTheme.SizeColumn(dgvCart, "MedicineId", 28, 40);
                        UiTheme.SizeColumn(dgvCart, "MedicineName", 100, 110);
                        UiTheme.SizeColumn(dgvCart, "Strength", 45, 60);
                        UiTheme.SizeColumn(dgvCart, "PharmacyName", 90, 110);
                        UiTheme.SizeColumn(dgvCart, "Quantity", 30, 40);
                        UiTheme.SizeColumn(dgvCart, "ListPrice", 42, 60);
                        UiTheme.SizeColumn(dgvCart, "DiscountPercent", 32, 45);
                        UiTheme.SizeColumn(dgvCart, "PriceYouPay", 48, 70);
                        UiTheme.SizeColumn(dgvCart, "LineTotal", 52, 80);
                        UiTheme.SizeColumn(dgvCart, "Stock", 38, 55);
                        UiTheme.SizeColumn(dgvCart, "RequiresRx", 24, 34);
                    }

                    SelectLine(selectMedicineId);
                }
                finally
                {
                    _binding = false;
                }

                // SelectionChanged was ignored while rebinding, so bring the
                // quantity box and the line buttons in line with the row that is
                // now highlighted; otherwise a selected row sat beside disabled
                // Update and Remove buttons until the customer clicked it again.
                UpdateLineButtons();

                // One query, one computation: every figure below is a sum over the groups.
                decimal itemsTotal = 0m, beforeDiscount = 0m;
                foreach (DataRow group in groups.Rows)
                {
                    itemsTotal += DbHelper.GetDecimal(group, "ItemsTotal");
                    beforeDiscount += DbHelper.GetDecimal(group, "BeforeDiscount");
                }
                decimal discount = beforeDiscount - itemsTotal;
                int pharmacyCount = groups.Rows.Count;
                decimal delivery = DeliveryCharge * pharmacyCount;

                lblItemsValue.Text = UiTheme.Money(itemsTotal);
                lblDiscountValue.Text = discount > 0m ? "- " + UiTheme.Money(discount) : UiTheme.Money(0m);
                lblDiscountValue.ForeColor = discount > 0m ? UiTheme.Success : UiTheme.TextDark;
                lblDeliveryValue.Text = UiTheme.Money(delivery);
                lblPayableValue.Text = UiTheme.Money(itemsTotal + delivery);

                lblSummaryNote.Text = pharmacyCount <= 1
                    ? "One delivery charge of " + UiTheme.Money(DeliveryCharge) + " applies.\r\n\r\n" +
                      "Items total already includes today's offers. The discount line shows how much they save you."
                    : "Your basket spans " + pharmacyCount + " pharmacies, so " + pharmacyCount +
                      " separate orders will be created at checkout - one per pharmacy, each with its own " +
                      "delivery charge of " + UiTheme.Money(DeliveryCharge) + " and its own invoice.";

                BuildSplitNote(groups);

                int blocked = CountBlockedLines(lines);
                bool hasLines = lines.Rows.Count > 0;
                btnCheckout.Enabled = hasLines && blocked == 0;
                btnClearCart.Enabled = hasLines;

                lblTitle.Text = hasLines ? "My Cart  (" + lines.Rows.Count + " line(s))" : "My Cart";

                string blockedNote = blocked > 0
                    ? blocked + " line(s) highlighted in red can't be bought as they are - lower the quantity or remove them to check out."
                    : null;

                string defaultNote = hasLines
                    ? "Change a quantity and press Update quantity. Enter 0 to remove a line."
                    : "Your cart is empty. Browse the catalogue and add something to it.";

                lblStatus.ForeColor = blocked > 0 ? UiTheme.Danger : UiTheme.TextMuted;
                lblStatus.Text = statusMessage != null
                    ? statusMessage + (blockedNote != null ? "   " + blockedNote : "")
                    : blockedNote ?? defaultNote;

                UpdateLineButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your cart could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Lines the checkout would refuse: more than the shelf holds, or no longer buyable.</summary>
        private static int CountBlockedLines(DataTable lines)
        {
            int blocked = 0;
            foreach (DataRow row in lines.Rows)
            {
                if (DbHelper.GetInt(row, "Quantity") > DbHelper.GetInt(row, "Stock") ||
                    DbHelper.GetInt(row, "CanBuy") == 0)
                    blocked++;
            }
            return blocked;
        }

        private void SelectLine(int medicineId)
        {
            if (medicineId <= 0) return;
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                if (Convert.ToInt32(row.Cells["MedicineId"].Value) == medicineId)
                {
                    dgvCart.CurrentCell = row.Cells["MedicineName"];
                    return;
                }
            }
        }

        private void BuildSplitNote(DataTable groups)
        {
            if (groups.Rows.Count == 0)
            {
                lblSplitNote.Text = "";
                return;
            }

            string text = "Checkout will create " + groups.Rows.Count + " order(s):" + Environment.NewLine;

            int index = 1;
            foreach (DataRow row in groups.Rows)
            {
                decimal itemsTotal = DbHelper.GetDecimal(row, "ItemsTotal");
                text += "   " + index + ".  " + row["PharmacyName"] + "  -  " +
                        row["Lines"] + " line(s), " + row["Units"] + " unit(s), items " +
                        UiTheme.Money(itemsTotal) + " + delivery " + UiTheme.Money(DeliveryCharge) +
                        "  =  " + UiTheme.Money(itemsTotal + DeliveryCharge) + Environment.NewLine;
                index++;
            }

            lblSplitNote.Text = text;
        }

        /// <summary>
        /// Row colours are set once per binding instead of in CellFormatting,
        /// which ran for every cell on every repaint. Red: the checkout would
        /// refuse this line. Green: an offer is running on it.
        /// </summary>
        private void dgvCart_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvCart.Columns.Count == 0) return;

            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                int stock = Convert.ToInt32(row.Cells["Stock"].Value);
                bool canBuy = Convert.ToInt32(row.Cells["CanBuy"].Value) == 1;
                decimal discount = Convert.ToDecimal(row.Cells["DiscountPercent"].Value);

                if (quantity > stock || !canBuy) row.DefaultCellStyle.BackColor = UiTheme.LowStockBack;
                else if (discount > 0m) row.DefaultCellStyle.BackColor = UiTheme.DeliveredBack;
                else row.DefaultCellStyle.BackColor = Color.Empty;
            }
        }

        private void dgvCart_SelectionChanged(object sender, EventArgs e)
        {
            if (_binding) return;
            UpdateLineButtons();
        }

        private void UpdateLineButtons()
        {
            int medicineId = SelectedMedicineId();
            bool hasRow = medicineId > 0;

            btnUpdateQuantity.Enabled = hasRow;
            btnRemoveLine.Enabled = hasRow;
            txtQuantity.Enabled = hasRow;

            // Only refill the box when a different line is selected, so a
            // reload does not throw away what the customer has just typed.
            if (medicineId != _quantityShownFor)
            {
                txtQuantity.Text = hasRow ? Convert.ToString(dgvCart.CurrentRow.Cells["Quantity"].Value) : "";
                _quantityShownFor = medicineId;
            }
        }

        private int SelectedMedicineId()
        {
            if (dgvCart.CurrentRow == null || dgvCart.Columns.Count == 0) return 0;
            object value = dgvCart.CurrentRow.Cells["MedicineId"].Value;
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        // ---------------------------------------------------------------------

        private void btnUpdateQuantity_Click(object sender, EventArgs e)
        {
            int medicineId = SelectedMedicineId();
            if (medicineId == 0) return;

            int quantity;
            if (!int.TryParse(txtQuantity.Text.Trim(), out quantity))
            {
                MessageBox.Show("Enter a whole number. Zero removes the line.", "Check the quantity",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string message;
            bool changed;
            try
            {
                changed = _cart.SetQuantity(UserSession.UserId, medicineId, quantity, out message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The quantity could not be updated.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (changed)
            {
                LoadCart(message, medicineId);
            }
            else
            {
                MessageBox.Show(message, "Cannot update the quantity", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadCart(null, medicineId);
            }
        }

        private void btnRemoveLine_Click(object sender, EventArgs e)
        {
            int medicineId = SelectedMedicineId();
            if (medicineId == 0) return;

            string name = Convert.ToString(dgvCart.CurrentRow.Cells["MedicineName"].Value);

            DialogResult answer = MessageBox.Show("Remove " + name + " from your cart?", "Remove line",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            bool removed;
            try
            {
                removed = _cart.Remove(UserSession.UserId, medicineId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The line could not be removed.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadCart(removed ? name + " removed from your cart." : name + " was already gone from your cart.");
        }

        private void btnClearCart_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show("Remove everything from your cart?", "Empty the cart",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes) return;

            try
            {
                _cart.ClearAll(UserSession.UserId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your cart could not be emptied.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadCart("Your cart has been emptied.");
        }

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            using (CheckoutForm checkout = new CheckoutForm())
            {
                checkout.ShowDialog(this);
            }

            LoadCart();

            // When the whole basket has been paid for there is nothing left to
            // show, so the cart closes and returns to the catalogue.
            try
            {
                if (_cart.CountLines(UserSession.UserId) == 0) Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = DbHelper.Describe(ex);
            }
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
