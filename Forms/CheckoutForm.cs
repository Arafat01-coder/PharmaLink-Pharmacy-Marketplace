using System.Configuration;
using System.Data;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirements 25 and 26.
    ///
    /// A basket that spans two pharmacies becomes two orders, so this form runs
    /// once per pharmacy and the heading says "Order 1 of 2". Confirm runs a
    /// single transaction that writes the order header, writes one OrderItems
    /// row per cart line at the price the customer was shown, records the
    /// prescription when one is needed, reduces the stock, freezes the
    /// commission and clears only that pharmacy's cart lines.
    ///
    /// The basket is re-read from the database every time a pharmacy is shown
    /// and after every attempt, never cached from when the form opened: prices,
    /// offers and stock can change while the customer is paying for the first
    /// half of a split basket.
    /// </summary>
    public partial class CheckoutForm : Form
    {
        private readonly CartService _cart = new CartService();
        private readonly OrderService _orders = new OrderService();
        private readonly AuthService _auth = new AuthService();

        /// <summary>Fresh per-pharmacy totals; row 0 is always the pharmacy being checked out.</summary>
        private DataTable _pharmacyGroups;
        private int _ordersPlaced;
        private bool _placing;

        private int _rxPharmacyId;
        private string _pendingRxImagePath = "";
        private string _pendingRxDoctorName = "";

        public CheckoutForm()
        {
            InitializeComponent();
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

        private void CheckoutForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();

            cmbPayment.Items.AddRange(new object[]
            {
                "- choose how you will pay -",
                "Cash on delivery",
                "bKash",
                "Nagad",
                "Card"
            });
            cmbPayment.SelectedIndex = 0;

            try
            {
                // The delivery address is pre-filled from the profile but stays editable.
                User me = _auth.GetUser(UserSession.UserId);
                if (me != null) txtAddress.Text = me.Address;

                if (!ReloadGroups())
                {
                    MessageBox.Show("Your cart is empty.", "PharmaLink",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The checkout could not be opened.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            ShowCurrentPharmacy();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Checkout");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);
            lblTitle.Font = UiTheme.FontTitleLarge;

            foreach (GroupBox group in new[] { grpDelivery, grpReview })
            {
                group.Font = UiTheme.FontHeading;
                group.ForeColor = UiTheme.Primary;
                group.BackColor = UiTheme.CardBack;

                foreach (Control child in group.Controls)
                {
                    child.Font = UiTheme.FontBody;
                    child.ForeColor = UiTheme.TextDark;
                    if (child is Label label && label.Name.EndsWith("Error"))
                    {
                        label.Font = UiTheme.FontSmall;
                        label.ForeColor = UiTheme.Danger;
                    }
                }
            }

            lblPayableCaption.Font = UiTheme.FontHeading;
            lblPayableValue.Font = UiTheme.FontTileValue;
            lblPayableValue.ForeColor = UiTheme.Primary;

            lblRxWarning.Font = UiTheme.FontSmall;
            lblRxState.Font = UiTheme.FontSmall;
            lblHint.Font = UiTheme.FontSmall;
            lblHint.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleSecondary(btnCancel);
            UiTheme.StyleAccent(btnUploadRx);
            UiTheme.StyleSuccess(btnConfirm);
            btnConfirm.Font = UiTheme.FontButtonLarge;
            UiTheme.StyleGrid(dgvReview);

            lblHint.Text =
                "Check the delivery address and the items on the right, then press Confirm order." + Environment.NewLine +
                "If your cart has items from more than one pharmacy, each pharmacy is a separate order with its own " +
                "delivery charge and invoice." + Environment.NewLine +
                "Stock and prices are checked again when you confirm. If anything has changed, nothing is charged " +
                "and you will be told what to update.";
        }

        // ---------------------------------------------------------------------
        //  ONE PHARMACY AT A TIME
        // ---------------------------------------------------------------------

        private DataRow CurrentGroup => _pharmacyGroups.Rows[0];
        private int CurrentPharmacyId => Convert.ToInt32(CurrentGroup["PharmacyId"]);
        private string CurrentPharmacyName => CurrentGroup["PharmacyName"].ToString();

        /// <summary>
        /// Re-reads the per-pharmacy totals. Pharmacies already paid for have
        /// left the cart, so the first row is always the next one to check out.
        /// Returns false when nothing is left.
        /// </summary>
        private bool ReloadGroups()
        {
            _pharmacyGroups = _cart.GetPharmacyGroups(UserSession.UserId, DeliveryCharge);
            return _pharmacyGroups.Rows.Count > 0;
        }

        private void ShowCurrentPharmacy()
        {
            // An attachment belongs to one pharmacy's order; moving on clears it.
            if (_rxPharmacyId != CurrentPharmacyId)
            {
                _pendingRxImagePath = "";
                _pendingRxDoctorName = "";
                _rxPharmacyId = CurrentPharmacyId;
            }

            int totalOrders = _ordersPlaced + _pharmacyGroups.Rows.Count;
            lblTitle.Text = "Order " + (_ordersPlaced + 1) + " of " + totalOrders;
            lblSubtitle.Text = totalOrders > 1
                ? CurrentPharmacyName + "   -   this part of your basket is a separate order with its own invoice."
                : CurrentPharmacyName;

            try
            {
                LoadReviewGrid();

                bool needsRx = _cart.ContainsPrescriptionItem(UserSession.UserId, CurrentPharmacyId);

                lblRxWarning.Visible = needsRx;
                btnUploadRx.Visible = needsRx;
                lblRxState.Visible = needsRx;

                if (needsRx)
                {
                    lblRxWarning.Text = "This order contains a prescription only medicine. Please attach a photograph " +
                                        "of your doctor's prescription (JPG or PNG, under 2 MB) before confirming.";
                    lblRxWarning.ForeColor = UiTheme.Warning;
                    ShowRxState();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Your basket could not be loaded: " + DbHelper.Describe(ex);
                lblStatus.ForeColor = UiTheme.Danger;
            }

            ValidateAll();
        }

        private void ShowRxState()
        {
            if (string.IsNullOrEmpty(_pendingRxImagePath))
            {
                lblRxState.Text = "No prescription attached yet.";
                lblRxState.ForeColor = UiTheme.Danger;
                return;
            }

            // The doctor's name is shown as typed; it usually already starts with "Dr".
            lblRxState.Text = "Attached: " + Path.GetFileName(_pendingRxImagePath) +
                              (string.IsNullOrWhiteSpace(_pendingRxDoctorName)
                                  ? "" : "   (" + _pendingRxDoctorName + ")");
            lblRxState.ForeColor = UiTheme.Success;
        }

        private void LoadReviewGrid()
        {
            DataTable allLines = _cart.GetLinesTable(UserSession.UserId);

            DataView view = new DataView(allLines);
            view.RowFilter = "PharmacyId = " + CurrentPharmacyId;
            DataTable mine = view.ToTable();

            dgvReview.DataSource = mine;

            if (dgvReview.Columns.Count > 0)
            {
                foreach (DataGridViewColumn column in dgvReview.Columns) column.Visible = false;

                dgvReview.Columns["MedicineName"].Visible = true;
                dgvReview.Columns["MedicineName"].HeaderText = "Medicine";
                dgvReview.Columns["Strength"].Visible = true;
                dgvReview.Columns["Strength"].HeaderText = "Strength";
                dgvReview.Columns["Strength"].FillWeight = 45;
                dgvReview.Columns["Quantity"].Visible = true;
                dgvReview.Columns["Quantity"].HeaderText = "Qty";
                dgvReview.Columns["Quantity"].FillWeight = 30;
                dgvReview.Columns["PriceYouPay"].Visible = true;
                dgvReview.Columns["PriceYouPay"].HeaderText = "Unit (Tk)";
                dgvReview.Columns["PriceYouPay"].FillWeight = 45;
                dgvReview.Columns["LineTotal"].Visible = true;
                dgvReview.Columns["LineTotal"].HeaderText = "Line total (Tk)";
                dgvReview.Columns["LineTotal"].FillWeight = 55;
            }

            // Totals come from the freshly reloaded group row, which uses the
            // same rounded unit price as the lines above and as the order itself.
            decimal itemsTotal = Convert.ToDecimal(CurrentGroup["ItemsTotal"]);
            decimal beforeDiscount = Convert.ToDecimal(CurrentGroup["BeforeDiscount"]);

            lblItemsValue.Text = UiTheme.Money(itemsTotal);
            lblDeliveryValue.Text = UiTheme.Money(DeliveryCharge);
            lblPayableValue.Text = UiTheme.Money(itemsTotal + DeliveryCharge);

            lblStatus.ForeColor = UiTheme.TextMuted;
            lblStatus.Text = beforeDiscount > itemsTotal
                ? "Today's offers have already taken " + UiTheme.Money(beforeDiscount - itemsTotal) +
                  " off this order. The prices above are the prices you pay."
                : "The prices above are the prices you pay, and they will appear on your invoice.";
        }

        // ---------------------------------------------------------------------
        //  VALIDATION
        // ---------------------------------------------------------------------

        private bool NeedsMobileNumber =>
            cmbPayment.SelectedIndex == 2 || cmbPayment.SelectedIndex == 3;   // bKash or Nagad

        /// <summary>
        /// False until the customer has picked from the payment list, so a
        /// freshly opened checkout does not greet them with a red error.
        /// Confirm stays disabled either way until a method is chosen.
        /// </summary>
        private bool _paymentTouched;

        private void cmbPayment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPayment.SelectedIndex > 0) _paymentTouched = true;

            // Choosing bKash or Nagad reveals the mobile number field.
            lblMobile.Visible = NeedsMobileNumber;
            txtMobile.Visible = NeedsMobileNumber;
            lblMobileError.Visible = false;

            if (NeedsMobileNumber)
                lblMobile.Text = cmbPayment.SelectedItem.ToString() + " number (11 digits)";

            ValidateAll();
        }

        private void Field_Changed(object sender, EventArgs e) => ValidateAll();

        /// <summary>
        /// Enables Confirm only when every field is valid, a prescription is
        /// attached when one is needed, and no order is being placed right now.
        /// A disabled styled button greys itself, so no colour is set here.
        /// </summary>
        private bool ValidateAll()
        {
            bool ok = true;

            ok &= Check(!Validator.IsBlank(txtAddress.Text), lblAddressError, txtAddress,
                        "We need somewhere to deliver to.");

            if (cmbPayment.SelectedIndex > 0 || _paymentTouched)
            {
                ok &= Check(cmbPayment.SelectedIndex > 0, lblPaymentError, cmbPayment,
                            "Choose a payment method.");
            }
            else
            {
                UiTheme.ClearError(lblPaymentError, cmbPayment);
                ok = false;
            }

            if (NeedsMobileNumber)
            {
                ok &= Check(Validator.IsMobile(txtMobile.Text), lblMobileError, txtMobile,
                            "Enter the 11 digit number the payment will come from.");
            }
            else
            {
                UiTheme.ClearError(lblMobileError, txtMobile);
            }

            bool needsRx = lblRxWarning.Visible;
            if (needsRx && string.IsNullOrEmpty(_pendingRxImagePath)) ok = false;

            btnConfirm.Enabled = ok && !_placing;
            return ok;
        }

        private bool Check(bool rulePassed, Label errorLabel, Control field, string message)
        {
            if (rulePassed) UiTheme.ClearError(errorLabel, field);
            else UiTheme.ShowError(errorLabel, field, message);
            return rulePassed;
        }

        private string PaymentMethodForDatabase()
        {
            switch (cmbPayment.SelectedIndex)
            {
                case 1: return "CashOnDelivery";
                case 2: return "bKash";
                case 3: return "Nagad";
                case 4: return "Card";
                default: return "";
            }
        }

        // ---------------------------------------------------------------------
        //  PRESCRIPTION
        // ---------------------------------------------------------------------

        private void btnUploadRx_Click(object sender, EventArgs e)
        {
            using (UploadPrescriptionForm upload = new UploadPrescriptionForm(CurrentPharmacyName))
            {
                if (upload.ShowDialog(this) == DialogResult.OK)
                {
                    _pendingRxImagePath = upload.SelectedImagePath;
                    _pendingRxDoctorName = upload.DoctorName;
                    _rxPharmacyId = CurrentPharmacyId;
                    ShowRxState();
                }
            }

            ValidateAll();
        }

        // ---------------------------------------------------------------------
        //  CONFIRM
        // ---------------------------------------------------------------------

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (_placing || !ValidateAll()) return;

            // Confirm, Cancel and Upload stay disabled until the attempt is
            // over, so a double click cannot place the same order twice.
            _placing = true;
            btnConfirm.Enabled = false;
            btnCancel.Enabled = false;
            btnUploadRx.Enabled = false;
            Cursor = Cursors.WaitCursor;

            int orderId = 0;
            string message = "";
            try
            {
                orderId = _orders.Checkout(UserSession.UserId, CurrentPharmacyId,
                                           txtAddress.Text.Trim(), PaymentMethodForDatabase(),
                                           NeedsMobileNumber ? txtMobile.Text.Trim() : null,
                                           DeliveryCharge,
                                           lblRxWarning.Visible ? _pendingRxImagePath : null,
                                           _pendingRxDoctorName, out message);
            }
            catch (Exception ex)
            {
                message = "The order could not be placed. " + DbHelper.Describe(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
                _placing = false;
                btnCancel.Enabled = true;
                btnUploadRx.Enabled = true;
            }

            if (orderId == 0)
            {
                MessageBox.Show(message, "Order not placed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RefreshAfterAttempt();
                return;
            }

            _ordersPlaced++;
            _pendingRxImagePath = "";
            _pendingRxDoctorName = "";

            using (InvoiceForm invoice = new InvoiceForm(orderId))
            {
                invoice.ShowDialog(this);
            }

            MoveToNextPharmacyOrFinish();
        }

        /// <summary>After a refused order: show today's real basket, keeping what the customer typed.</summary>
        private void RefreshAfterAttempt()
        {
            try
            {
                if (!ReloadGroups())
                {
                    MessageBox.Show("There is nothing left in your cart to check out.", "PharmaLink",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = _ordersPlaced > 0 ? DialogResult.OK : DialogResult.Cancel;
                    Close();
                    return;
                }
                ShowCurrentPharmacy();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Your basket could not be reloaded: " + DbHelper.Describe(ex);
                lblStatus.ForeColor = UiTheme.Danger;
                ValidateAll();
            }
        }

        private void MoveToNextPharmacyOrFinish()
        {
            bool more;
            try
            {
                more = ReloadGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your order is placed, but the rest of your basket could not be loaded.\r\n\r\n" +
                                DbHelper.Describe(ex), "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            if (more)
            {
                MessageBox.Show(
                    "That order is placed.\r\n\r\n" +
                    "Your basket also contains items from " + CurrentPharmacyName +
                    ", which is a separate pharmacy and therefore a separate order. " +
                    "Let us finish that one now.",
                    "Next pharmacy", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ShowCurrentPharmacy();
                return;
            }

            MessageBox.Show(
                "All done. Every order has been placed and your cart is now empty.\r\n\r\n" +
                "You can reopen any invoice at any time from My Orders.",
                "Thank you", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show(
                "Leave the checkout?\r\n\r\nOrders you have already placed stay placed; " +
                "the rest of your basket is left untouched.",
                "Leave checkout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes) return;

            DialogResult = _ordersPlaced > 0 ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }
    }
}
