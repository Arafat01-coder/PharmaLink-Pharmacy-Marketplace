using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// The pharmacy owner's hub (requirements 10 to 18).
    ///
    /// The rule that governs this whole branch of the application is
    /// requirement 18: every query behind every form reachable from here
    /// carries WHERE PharmacyId = UserSession.PharmacyId, taken from the login,
    /// so one pharmacy owner can never read another owner's medicines, orders
    /// or earnings. The rule lives in the queries, not in hidden buttons.
    ///
    /// Login checks the account and the pharmacy once. Because the Super Admin
    /// can suspend either while the owner is working, the dashboard checks
    /// again before it opens any screen and on Refresh, and signs the owner
    /// out exactly like Log out when the session is no longer valid.
    /// </summary>
    public partial class AdminDashboard : Form
    {
        private readonly OrderService _orders = new OrderService();
        private readonly MedicineService _medicines = new MedicineService();
        private readonly ReportService _reports = new ReportService();
        private readonly PrescriptionService _prescriptions = new PrescriptionService();
        private readonly AuthService _auth = new AuthService();

        private Panel[] _tiles;
        private Label _tileOrders;
        private Label _tileRevenue;
        private Label _tileCommission;
        private Label _tileLowStock;

        private bool _loading = true;

        public AdminDashboard()
        {
            InitializeComponent();
        }

        private void AdminDashboard_Load(object sender, EventArgs e)
        {
            UiTheme.MakeResizable(this, Size);
            ApplyTheme();
            BuildTiles();
            Resize += (s, args) => LayoutTiles();

            cmbOrderStatus.Items.AddRange(new object[] { "All orders", "Placed", "Confirmed", "Delivered", "Cancelled" });
            cmbOrderStatus.SelectedIndex = 0;

            _loading = false;
            LoadEverything();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Pharmacy Owner");

            panelSide.BackColor = UiTheme.Sidebar;
            lblBrand.Font = UiTheme.FontBrand;
            lblBrand.ForeColor = Color.White;
            lblRole.Font = UiTheme.FontCaption;
            lblRole.ForeColor = UiTheme.SidebarRole;
            lblShopName.Font = UiTheme.FontSmall;
            lblShopName.ForeColor = UiTheme.SidebarUser;
            lblShopName.Text = UserSession.PharmacyName + Environment.NewLine + UserSession.FullName;

            foreach (Button button in new[] { btnMedicines, btnInventory, btnPrescriptions, btnEarnings,
                                              btnOffers, btnReviews, btnShopProfile, btnMyProfile })
            {
                UiTheme.StyleSidebarButton(button);
            }

            UiTheme.StyleSidebarButton(btnLogout);
            btnLogout.BackColor = UiTheme.Danger;
            btnLogout.FlatAppearance.MouseOverBackColor = UiTheme.DangerHover;

            UiTheme.StyleHeader(panelHeader, lblHeaderTitle, lblHeaderSub);
            UiTheme.StyleSecondary(btnRefresh);

            lblOrdersTitle.Font = UiTheme.FontHeading;
            lblOrdersTitle.ForeColor = UiTheme.TextDark;
            lblOrdersHint.Font = UiTheme.FontSmall;
            lblOrdersHint.ForeColor = UiTheme.TextMuted;
            lblOrderActionHint.Font = UiTheme.FontSmall;
            lblOrderActionHint.ForeColor = UiTheme.Warning;
            lblLowStockTitle.Font = UiTheme.FontHeading;
            lblLowStockTitle.ForeColor = UiTheme.TextDark;

            UiTheme.StyleGrid(dgvOrders);
            UiTheme.StyleGrid(dgvLowStock);
            UiTheme.EnableEmptyMessage(dgvOrders, "No orders match this filter.");
            UiTheme.EnableEmptyMessage(dgvLowStock, "Every medicine is at or above its minimum stock level.");

            // Row colours are applied once per binding rather than in
            // CellFormatting, which runs for every cell on every repaint.
            dgvOrders.DataBindingComplete += dgvOrders_DataBindingComplete;
            dgvLowStock.DataBindingComplete += dgvLowStock_DataBindingComplete;

            UiTheme.StyleSuccess(btnConfirmOrder);
            UiTheme.StylePrimary(btnDeliverOrder);
            UiTheme.StyleDanger(btnCancelOrder);
            UiTheme.StyleSecondary(btnViewInvoice);
        }

        /// <summary>
        /// Revenue and commission count only Confirmed and Delivered orders
        /// (item lines, without the delivery charge), so the captions say so:
        /// a Placed order may still be cancelled and is not money yet.
        /// </summary>
        private void BuildTiles()
        {
            // Short enough to fit a tile at 125% and 150% display scaling; the
            // "confirmed + delivered, no delivery charge" detail is in the hint
            // under the orders heading.
            Panel t1 = UiTheme.BuildTile("CONFIRMED + DELIVERED ORDERS", UiTheme.Accent, out _tileOrders);
            Panel t2 = UiTheme.BuildTile("ITEM SALES (NO DELIVERY)", UiTheme.Primary, out _tileRevenue);
            Panel t3 = UiTheme.BuildTile("PHARMALINK COMMISSION", UiTheme.Warning, out _tileCommission);
            Panel t4 = UiTheme.BuildTile("MEDICINES BELOW MIN STOCK", UiTheme.Danger, out _tileLowStock);

            _tiles = new[] { t1, t2, t3, t4 };
            LayoutTiles();
        }

        /// <summary>
        /// Places the tiles from the real bounds of the sidebar and the header,
        /// not from fixed pixels, so they cannot overlap either one whatever
        /// size or DPI the window ends up at.
        /// </summary>
        private void LayoutTiles()
        {
            if (_tiles == null) return;
            UiTheme.LayoutTileRow(this, panelSide, panelHeader, ClientSize.Width - dgvOrders.Right, _tiles);
        }

        // ---------------------------------------------------------------------
        //  SESSION
        // ---------------------------------------------------------------------

        /// <summary>
        /// Re-reads the account and pharmacy status. When the Super Admin has
        /// suspended either, the owner is told why and signed out the same way
        /// Log out does it. A database error does not sign anyone out; it only
        /// stops the screen from opening.
        /// </summary>
        private bool SessionStillValid()
        {
            try
            {
                string message;
                if (_auth.CheckSessionStillValid(out message)) return true;

                MessageBox.Show(message, "Signed out", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UserSession.Clear();
                Close();
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(DbHelper.Describe(ex), "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ---------------------------------------------------------------------
        //  DATA
        // ---------------------------------------------------------------------

        private void LoadEverything()
        {
            if (_loading) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                int orders, pendingOrders;
                decimal revenue, commission;
                _reports.GetPharmacyTotals(UserSession.PharmacyId, out orders, out revenue,
                                           out commission, out pendingOrders);

                int lowStock = _medicines.CountLowStock(UserSession.PharmacyId);
                int pendingRx = _prescriptions.CountPending(UserSession.PharmacyId);

                _tileOrders.Text = orders.ToString("N0");
                _tileRevenue.Text = UiTheme.Money(revenue);
                _tileCommission.Text = UiTheme.Money(commission);
                _tileLowStock.Text = lowStock.ToString("N0");

                lblHeaderSub.Text = UserSession.PharmacyName +
                                    "   |   " + _medicines.CountMedicines(UserSession.PharmacyId) + " medicines listed" +
                                    "   |   " + pendingOrders + " order(s) waiting to be confirmed" +
                                    "   |   " + pendingRx + " prescription(s) to verify";

                LoadOrders();

                dgvLowStock.DataSource = _medicines.GetLowStock(UserSession.PharmacyId);
                if (dgvLowStock.Columns.Count > 0)
                {
                    dgvLowStock.Columns["MedicineId"].HeaderText = "ID";
                    dgvLowStock.Columns["MedicineName"].HeaderText = "Medicine";
                    dgvLowStock.Columns["Strength"].HeaderText = "Strength";
                    dgvLowStock.Columns["CategoryName"].HeaderText = "Category";
                    dgvLowStock.Columns["Stock"].HeaderText = "In stock";
                    dgvLowStock.Columns["MinStock"].HeaderText = "Minimum";
                    dgvLowStock.Columns["ShortfallUnits"].HeaderText = "Order at least";

                    UiTheme.SizeColumn(dgvLowStock, "MedicineId", 30, 45);
                    UiTheme.SizeColumn(dgvLowStock, "MedicineName", 120, 140);
                    UiTheme.SizeColumn(dgvLowStock, "Strength", 60, 80);
                    UiTheme.SizeColumn(dgvLowStock, "CategoryName", 90, 110);
                    UiTheme.SizeColumn(dgvLowStock, "Stock", 50, 70);
                    UiTheme.SizeColumn(dgvLowStock, "MinStock", 50, 80);
                    UiTheme.SizeColumn(dgvLowStock, "ShortfallUnits", 60, 110);
                }

                lblLowStockTitle.Text = lowStock == 0
                    ? "Low stock alert  -  every medicine is above its minimum level"
                    : "Low stock alert  (" + lowStock + ")  -  double click a row to restock";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load the dashboard.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void LoadOrders()
        {
            string status = cmbOrderStatus.SelectedIndex <= 0 ? "" : cmbOrderStatus.SelectedItem.ToString();
            dgvOrders.DataSource = _orders.GetOrdersForPharmacy(UserSession.PharmacyId, status);

            if (dgvOrders.Columns.Count > 0)
            {
                dgvOrders.Columns["OrderId"].HeaderText = "Order";
                dgvOrders.Columns["OrderDate"].HeaderText = "Placed";
                dgvOrders.Columns["OrderDate"].DefaultCellStyle.Format = "dd MMM, HH:mm";
                dgvOrders.Columns["CustomerName"].HeaderText = "Customer";
                dgvOrders.Columns["CustomerPhone"].HeaderText = "Mobile";
                dgvOrders.Columns["Items"].HeaderText = "Lines";
                dgvOrders.Columns["ItemsTotal"].HeaderText = "Items (Tk)";
                dgvOrders.Columns["DeliveryCharge"].HeaderText = "Delivery";
                dgvOrders.Columns["TotalAmount"].HeaderText = "Total (Tk)";
                dgvOrders.Columns["PaymentMethod"].HeaderText = "Payment";
                dgvOrders.Columns["Status"].HeaderText = "Status";
                dgvOrders.Columns["RxState"].HeaderText = "Prescription";
                dgvOrders.Columns["DeliveryAddress"].Visible = false;

                // Weights share the width; the minimums stop short columns such
                // as Delivery and Status being cut to "Delivei" and "Confi...".
                UiTheme.SizeColumn(dgvOrders, "OrderId", 45, 60);
                UiTheme.SizeColumn(dgvOrders, "OrderDate", 85, 110);
                UiTheme.SizeColumn(dgvOrders, "CustomerName", 110, 110);
                UiTheme.SizeColumn(dgvOrders, "CustomerPhone", 85, 100);
                UiTheme.SizeColumn(dgvOrders, "Items", 40, 55);
                UiTheme.SizeColumn(dgvOrders, "ItemsTotal", 65, 85);
                UiTheme.SizeColumn(dgvOrders, "DeliveryCharge", 60, 75);
                UiTheme.SizeColumn(dgvOrders, "TotalAmount", 65, 85);
                UiTheme.SizeColumn(dgvOrders, "PaymentMethod", 85, 105);
                UiTheme.SizeColumn(dgvOrders, "Status", 65, 85);
                UiTheme.SizeColumn(dgvOrders, "RxState", 80, 100);
            }

            UpdateOrderButtons();
        }

        private void dgvOrders_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvOrders.Columns.Contains("Status") || !dgvOrders.Columns.Contains("RxState")) return;

            foreach (DataGridViewRow row in dgvOrders.Rows)
            {
                string status = Convert.ToString(row.Cells["Status"].Value);
                string rx = Convert.ToString(row.Cells["RxState"].Value);

                if (status == "Placed" && rx != "Not needed" && rx != "Approved")
                    row.DefaultCellStyle.BackColor = UiTheme.PendingBack;
                else if (status == "Delivered")
                    row.DefaultCellStyle.BackColor = UiTheme.DeliveredBack;
                else if (status == "Cancelled")
                    row.DefaultCellStyle.BackColor = UiTheme.InactiveBack;
                else
                    row.DefaultCellStyle.BackColor = Color.Empty;   // keep the alternating shade
            }
        }

        private void dgvLowStock_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvLowStock.Rows)
                row.DefaultCellStyle.BackColor = UiTheme.LowStockBack;
        }

        // ---------------------------------------------------------------------
        //  ORDER ACTIONS
        // ---------------------------------------------------------------------

        private void dgvOrders_SelectionChanged(object sender, EventArgs e) => UpdateOrderButtons();

        /// <summary>
        /// Confirm is only offered for a Placed order whose prescription is not
        /// needed or already approved. OrderService.Confirm enforces the same
        /// rule in its UPDATE, so this only saves the owner a refused click, and
        /// the hint next to the buttons says what the order is waiting for.
        /// </summary>
        private void UpdateOrderButtons()
        {
            DataGridViewRow row = dgvOrders.CurrentRow;
            bool hasRow = row != null && dgvOrders.Columns.Contains("Status") && row.Cells["Status"].Value != null;

            string status = hasRow ? row.Cells["Status"].Value.ToString() : "";
            string rxState = hasRow && dgvOrders.Columns.Contains("RxState")
                ? Convert.ToString(row.Cells["RxState"].Value) : "";
            bool rxOk = rxState == "Not needed" || rxState == "Approved";

            btnConfirmOrder.Enabled = hasRow && status == "Placed" && rxOk;
            btnDeliverOrder.Enabled = hasRow && status == "Confirmed";
            btnCancelOrder.Enabled = hasRow && status != "Delivered" && status != "Cancelled";
            btnViewInvoice.Enabled = hasRow;

            string hint = "";
            if (hasRow && status == "Placed" && !rxOk)
            {
                switch (rxState)
                {
                    case "Missing": hint = "Waiting for the customer's prescription"; break;
                    case "Rejected": hint = "Prescription rejected - waiting for a new upload"; break;
                    default: hint = "Prescription needs your review in Verify Prescriptions"; break;
                }
            }
            lblOrderActionHint.Text = hint;
        }

        private int SelectedOrderId()
        {
            if (dgvOrders.CurrentRow == null || !dgvOrders.Columns.Contains("OrderId")) return 0;
            object value = dgvOrders.CurrentRow.Cells["OrderId"].Value;
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private void btnConfirmOrder_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;

            try
            {
                bool changed = _orders.Confirm(orderId, UserSession.PharmacyId);
                LoadEverything();

                if (changed)
                {
                    MessageBox.Show("Order " + orderId + " confirmed and is ready for dispatch.",
                        "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Order " + orderId + " was not confirmed.\r\n\r\n" +
                        "Either its prescription is not approved yet, or the order changed since the list " +
                        "was loaded (for example it was cancelled). The list has been refreshed.",
                        "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The order could not be confirmed.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeliverOrder_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;

            try
            {
                bool changed = _orders.MarkDelivered(orderId, UserSession.PharmacyId);
                LoadEverything();

                if (!changed)
                {
                    MessageBox.Show(
                        "Order " + orderId + " was not marked delivered because it is no longer Confirmed. " +
                        "The list has been refreshed.",
                        "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The order could not be updated.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelOrder_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;

            DialogResult answer = MessageBox.Show(
                "Cancel order " + orderId + "?\r\n\r\n" +
                "The units on this order are put back in stock.",
                "Cancel order", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes) return;

            try
            {
                bool changed = _orders.Cancel(orderId, UserSession.PharmacyId);
                LoadEverything();

                if (!changed)
                {
                    MessageBox.Show(
                        "Order " + orderId + " was not cancelled because it has already been delivered or cancelled. " +
                        "The list has been refreshed.",
                        "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The order could not be cancelled.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewInvoice_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;
            if (!SessionStillValid()) return;

            using (InvoiceForm invoice = new InvoiceForm(orderId))
            {
                invoice.ShowDialog(this);
            }
        }

        private void dgvLowStock_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!SessionStillValid()) return;

            int medicineId = Convert.ToInt32(dgvLowStock.Rows[e.RowIndex].Cells["MedicineId"].Value);

            using (MedicineEditorForm editor = new MedicineEditorForm(medicineId, true))
            {
                editor.ShowDialog(this);
            }
            LoadEverything();
        }

        // ---------------------------------------------------------------------
        //  NAVIGATION
        // ---------------------------------------------------------------------

        private void OpenChild(Form child)
        {
            using (child)
            {
                if (!SessionStillValid()) return;
                child.ShowDialog(this);
            }
            LoadEverything();
        }

        private void btnMedicines_Click(object sender, EventArgs e) => OpenChild(new AdminMedicineForm());
        private void btnInventory_Click(object sender, EventArgs e) => OpenChild(new AdminInventoryForm());
        private void btnPrescriptions_Click(object sender, EventArgs e) => OpenChild(new VerifyPrescriptionForm());
        private void btnEarnings_Click(object sender, EventArgs e) => OpenChild(new AdminEarningsForm());
        private void btnOffers_Click(object sender, EventArgs e) => OpenChild(new DiscountOffersForm());
        private void btnReviews_Click(object sender, EventArgs e) => OpenChild(new AdminReviewsForm());
        private void btnShopProfile_Click(object sender, EventArgs e) => OpenChild(new PharmacyProfileForm());
        private void btnMyProfile_Click(object sender, EventArgs e) => OpenChild(new MyProfileForm());

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!SessionStillValid()) return;
            LoadEverything();
        }

        private void cmbOrderStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            try
            {
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(DbHelper.Describe(ex), "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show("Log out of PharmaLink?", "Log out",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer == DialogResult.Yes)
            {
                UserSession.Clear();
                Close();
            }
        }
    }
}
