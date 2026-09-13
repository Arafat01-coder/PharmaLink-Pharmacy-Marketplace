using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 27. Past orders with a status pill, the invoice and the Rate
    /// and Review action.
    ///
    /// The CanReview flag comes from the query itself: an order qualifies only
    /// when its status is 'Delivered' and at least one of its medicines has not
    /// been reviewed yet. The button follows that flag rather than guessing.
    ///
    /// The Prescription column shows the order's current prescription state.
    /// While an order is still 'Placed' the customer can cancel it, and can
    /// upload a (new) prescription when it is Missing or was Rejected; the
    /// database re-checks both rules, so the buttons are only a convenience.
    /// </summary>
    public partial class OrderHistoryForm : Form
    {
        /// <summary>Load and Refresh both start from the same window: the last six months.</summary>
        private const int DefaultMonthsBack = 6;

        private readonly OrderService _orders = new OrderService();
        private readonly PrescriptionService _prescriptions = new PrescriptionService();
        private bool _loading = true;

        /// <summary>True while the grid is being rebound, so SelectionChanged does not hit the database per row.</summary>
        private bool _binding;

        /// <summary>The order whose items are on screen, so re-selecting it does not query again.</summary>
        private int _itemsShownFor = -1;

        public OrderHistoryForm()
        {
            InitializeComponent();
        }

        private void OrderHistoryForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            UiTheme.MakeResizable(this, Size);

            cmbStatus.Items.AddRange(new object[] { "All statuses", "Placed", "Confirmed", "Delivered", "Cancelled" });

            try
            {
                LoadPharmacyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The pharmacy list could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            ResetFilters();

            _loading = false;
            LoadOrders();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "My Orders");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            lblItemsTitle.Font = UiTheme.FontHeading;
            lblItemsTitle.ForeColor = UiTheme.TextDark;
            lblNote.Font = UiTheme.FontSmall;
            lblNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSecondary(btnRefresh);
            UiTheme.StyleAccent(btnViewInvoice);
            UiTheme.StylePrimary(btnRateReview);
            UiTheme.StyleSecondary(btnUploadRx);
            UiTheme.StyleDanger(btnCancelOrder);
            UiTheme.StyleGrid(dgvOrders);
            UiTheme.StyleGrid(dgvOrderItems);
            UiTheme.EnableEmptyMessage(dgvOrders, "No orders match these filters.");
            dgvOrders.DataBindingComplete += dgvOrders_DataBindingComplete;
        }

        /// <summary>
        /// Every pharmacy the customer has ordered from, whatever its status
        /// today. Using the approved list here hid the orders of a shop that was
        /// later suspended.
        /// </summary>
        private void LoadPharmacyFilter()
        {
            cmbPharmacy.Items.Clear();
            cmbPharmacy.Items.Add("All pharmacies");
            foreach (DataRow row in _orders.GetPharmaciesForCustomer(UserSession.UserId).Rows)
                cmbPharmacy.Items.Add(DbHelper.GetInt(row, "PharmacyId") + " - " + DbHelper.GetString(row, "PharmacyName"));
        }

        private void ResetFilters()
        {
            if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;
            if (cmbPharmacy.Items.Count > 0) cmbPharmacy.SelectedIndex = 0;
            dtpFrom.Value = DateTime.Today.AddMonths(-DefaultMonthsBack);
            dtpTo.Value = DateTime.Today;
        }

        private int SelectedPharmacyId()
        {
            if (cmbPharmacy.SelectedIndex <= 0) return 0;
            string text = cmbPharmacy.SelectedItem.ToString();
            return int.Parse(text.Substring(0, text.IndexOf(' ')));
        }

        // ---------------------------------------------------------------------

        /// <summary>Rebinds the grid, keeps the same order selected, then shows <paramref name="statusMessage"/> if given.</summary>
        private void LoadOrders(string statusMessage = null)
        {
            if (_loading) return;

            if (dtpFrom.Value.Date > dtpTo.Value.Date)
            {
                lblStatus.ForeColor = UiTheme.Danger;
                lblStatus.Text = "The 'From' date must be on or before the 'To' date.";
                return;
            }

            try
            {
                string status = cmbStatus.SelectedIndex <= 0 ? "" : cmbStatus.SelectedItem.ToString();
                int keepOrderId = SelectedOrderId();

                DataTable table = _orders.GetHistoryForCustomer(
                    UserSession.UserId, status, SelectedPharmacyId(), dtpFrom.Value, dtpTo.Value);

                _binding = true;
                try
                {
                    dgvOrders.DataSource = table;

                    if (dgvOrders.Columns.Count > 0)
                    {
                        dgvOrders.Columns["OrderId"].HeaderText = "Invoice";
                        dgvOrders.Columns["OrderDate"].HeaderText = "Placed on";
                        dgvOrders.Columns["PharmacyName"].HeaderText = "Pharmacy";
                        dgvOrders.Columns["Items"].HeaderText = "Lines";
                        dgvOrders.Columns["ItemsTotal"].HeaderText = "Items (Tk)";
                        dgvOrders.Columns["DeliveryCharge"].HeaderText = "Delivery";
                        dgvOrders.Columns["TotalAmount"].HeaderText = "Paid (Tk)";
                        dgvOrders.Columns["PaymentMethod"].HeaderText = "Method";
                        dgvOrders.Columns["Status"].HeaderText = "Status";
                        dgvOrders.Columns["RxState"].HeaderText = "Prescription";
                        dgvOrders.Columns["RejectReason"].Visible = false;
                        dgvOrders.Columns["CanReview"].Visible = false;

                        UiTheme.SizeColumn(dgvOrders, "OrderId", 40, 60);
                        UiTheme.SizeColumn(dgvOrders, "OrderDate", 80, 120);
                        UiTheme.SizeColumn(dgvOrders, "PharmacyName", 100, 130);
                        UiTheme.SizeColumn(dgvOrders, "Items", 30, 45);
                        UiTheme.SizeColumn(dgvOrders, "ItemsTotal", 50, 75);
                        UiTheme.SizeColumn(dgvOrders, "DeliveryCharge", 45, 65);
                        UiTheme.SizeColumn(dgvOrders, "TotalAmount", 50, 75);
                        UiTheme.SizeColumn(dgvOrders, "PaymentMethod", 60, 90);
                        UiTheme.SizeColumn(dgvOrders, "Status", 50, 75);
                        UiTheme.SizeColumn(dgvOrders, "RxState", 60, 90);
                    }

                    SelectOrder(keepOrderId);
                }
                finally
                {
                    _binding = false;
                }

                _itemsShownFor = -1;
                UpdateSelection();

                decimal lifetime = 0m;
                foreach (DataRow row in table.Rows)
                    if (row["TotalAmount"] != DBNull.Value && row["Status"].ToString() != "Cancelled")
                        lifetime += Convert.ToDecimal(row["TotalAmount"]);

                lblStatus.ForeColor = UiTheme.TextMuted;
                lblStatus.Text = statusMessage ??
                                 table.Rows.Count + " order(s) between " +
                                 dtpFrom.Value.ToString("dd MMM yyyy") + " and " +
                                 dtpTo.Value.ToString("dd MMM yyyy") +
                                 ".   Total spent in this period: " + UiTheme.Money(lifetime) + ".";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your orders could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectOrder(int orderId)
        {
            if (orderId <= 0) return;
            foreach (DataGridViewRow row in dgvOrders.Rows)
            {
                if (Convert.ToInt32(row.Cells["OrderId"].Value) == orderId)
                {
                    dgvOrders.CurrentCell = row.Cells["OrderId"];
                    return;
                }
            }
        }

        /// <summary>
        /// The status pill: delivered green, cancelled grey, waiting amber, and a
        /// placed order that still needs a prescription from the customer in a
        /// stronger warning tint. Applied once per binding rather than in
        /// CellFormatting, which ran on every repaint.
        /// </summary>
        private void dgvOrders_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvOrders.Columns.Count == 0) return;

            foreach (DataGridViewRow row in dgvOrders.Rows)
            {
                string status = Convert.ToString(row.Cells["Status"].Value);
                string rxState = Convert.ToString(row.Cells["RxState"].Value);

                Color back;
                switch (status)
                {
                    case "Delivered": back = UiTheme.DeliveredBack; break;
                    case "Cancelled": back = UiTheme.InactiveBack; break;
                    case "Placed":
                        back = rxState == "Missing" || rxState == "Rejected" ? UiTheme.WarningBack : UiTheme.PendingBack;
                        break;
                    default: back = Color.Empty; break;
                }
                row.DefaultCellStyle.BackColor = back;
            }
        }

        private void dgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (_binding) return;
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            DataGridViewRow row = dgvOrders.CurrentRow;
            int orderId = SelectedOrderId();

            if (row == null || orderId == 0)
            {
                dgvOrderItems.DataSource = null;
                _itemsShownFor = -1;
                btnViewInvoice.Enabled = false;
                btnRateReview.Enabled = false;
                btnRateReview.Text = "Rate and review";
                btnUploadRx.Enabled = false;
                btnCancelOrder.Enabled = false;
                lblNote.Text = DefaultNote;
                return;
            }

            if (orderId != _itemsShownFor)
            {
                try
                {
                    dgvOrderItems.DataSource = _orders.GetOrderItems(orderId);

                    if (dgvOrderItems.Columns.Count > 0)
                    {
                        dgvOrderItems.Columns["MedicineName"].HeaderText = "Medicine";
                        dgvOrderItems.Columns["Strength"].HeaderText = "Strength";
                        dgvOrderItems.Columns["Quantity"].HeaderText = "Qty";
                        dgvOrderItems.Columns["UnitPrice"].HeaderText = "Unit price paid (Tk)";
                        dgvOrderItems.Columns["Subtotal"].HeaderText = "Line total (Tk)";
                    }
                    _itemsShownFor = orderId;
                }
                catch (Exception ex)
                {
                    dgvOrderItems.DataSource = null;
                    lblStatus.Text = "The items on order " + orderId + " could not be loaded: " + DbHelper.Describe(ex);
                }
            }

            btnViewInvoice.Enabled = true;

            bool canReview = row.Cells["CanReview"].Value != DBNull.Value &&
                             Convert.ToInt32(row.Cells["CanReview"].Value) == 1;
            string status = Convert.ToString(row.Cells["Status"].Value);
            string rxState = Convert.ToString(row.Cells["RxState"].Value);
            string reason = Convert.ToString(row.Cells["RejectReason"].Value);

            btnRateReview.Enabled = canReview;
            if (canReview) btnRateReview.Text = "Rate and review";
            else if (status != "Delivered") btnRateReview.Text = "Not delivered yet";
            else btnRateReview.Text = "Already reviewed";

            bool placed = status == "Placed";
            btnCancelOrder.Enabled = placed;
            btnUploadRx.Enabled = placed && (rxState == "Missing" || rxState == "Rejected");
            btnUploadRx.Text = rxState == "Rejected" ? "Upload new prescription" : "Upload prescription";

            lblNote.Text = PrescriptionNote(rxState, reason, placed);
        }

        private const string DefaultNote =
            "Rate and review opens once an order is delivered. You can cancel an order until the pharmacy confirms it.";

        /// <summary>A plain sentence about the selected order's prescription.</summary>
        private static string PrescriptionNote(string rxState, string reason, bool placed)
        {
            switch (rxState)
            {
                case "Pending":
                    return "Your prescription is waiting for the pharmacy to check it.";
                case "Approved":
                    return "Your prescription has been approved.";
                case "Rejected":
                    return "The pharmacy could not accept your prescription" +
                           (string.IsNullOrWhiteSpace(reason) ? "." : ": " + reason + ".") +
                           (placed ? " Please upload a clearer photo." : "");
                case "Missing":
                    return placed
                        ? "This order needs a prescription. Upload one so the pharmacy can confirm it."
                        : "No prescription was attached to this order.";
                default:
                    return DefaultNote;
            }
        }

        private int SelectedOrderId()
        {
            if (dgvOrders.CurrentRow == null || dgvOrders.Columns.Count == 0) return 0;
            object value = dgvOrders.CurrentRow.Cells["OrderId"].Value;
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        // ---------------------------------------------------------------------

        private void btnViewInvoice_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;

            using (InvoiceForm invoice = new InvoiceForm(orderId))
            {
                invoice.ShowDialog(this);
            }
        }

        private void dgvOrders_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnViewInvoice_Click(sender, EventArgs.Empty);
        }

        private void btnRateReview_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;

            bool posted;
            using (GiveRatingForm rating = new GiveRatingForm(orderId))
            {
                posted = rating.ShowDialog(this) == DialogResult.OK;
            }

            LoadOrders(posted ? "Thank you - your review is now visible on the medicine's details screen." : null);
        }

        private void btnUploadRx_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;

            string pharmacyName = Convert.ToString(dgvOrders.CurrentRow.Cells["PharmacyName"].Value);

            string imagePath, doctorName;
            using (UploadPrescriptionForm upload = new UploadPrescriptionForm(pharmacyName))
            {
                if (upload.ShowDialog(this) != DialogResult.OK) return;
                imagePath = upload.SelectedImagePath;
                doctorName = upload.DoctorName;
            }

            string message;
            bool uploaded;
            try
            {
                uploaded = _prescriptions.Upload(orderId, UserSession.UserId, imagePath, doctorName, out message);
            }
            catch (Exception ex)
            {
                uploaded = false;
                message = DbHelper.Describe(ex);
            }

            if (!uploaded)
                MessageBox.Show(message, "Prescription not uploaded", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            LoadOrders(uploaded ? message : null);
        }

        private void btnCancelOrder_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;

            DialogResult answer = MessageBox.Show(
                "Cancel order #" + orderId + "?\r\n\r\nThis cannot be undone.",
                "Cancel order", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (answer != DialogResult.Yes) return;

            bool cancelled;
            try
            {
                cancelled = _orders.CancelByCustomer(orderId, UserSession.UserId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The order could not be cancelled.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadOrders();
                return;
            }

            LoadOrders(cancelled
                ? "Order #" + orderId + " has been cancelled."
                : "Order #" + orderId + " could not be cancelled - the pharmacy may already have confirmed it.");
        }

        private void Filter_Changed(object sender, EventArgs e) => LoadOrders();

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _loading = true;
            try
            {
                LoadPharmacyFilter();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "The pharmacy list could not be refreshed: " + DbHelper.Describe(ex);
            }
            ResetFilters();
            _loading = false;
            LoadOrders();
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
