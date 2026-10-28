using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// The delivery man's home screen, opened by LoginForm for
    /// UserType = 'DeliveryMan'.
    ///
    /// One grid, three views chosen from the sidebar:
    ///  - Available Orders: confirmed orders nobody has taken. Accept order
    ///    makes one his.
    ///  - My Deliveries: what he has accepted and not finished. Picked up when
    ///    he leaves the pharmacy with it, Delivered at the customer's door, or
    ///    Give back (before pickup only) to return it to the pool.
    ///  - Delivery History: finished jobs between two dates, with the cash he
    ///    collected and the delivery fees he earned.
    ///
    /// Every action re-checks the session first, like the other dashboards,
    /// so a delivery man the Super Admin suspends is signed out at his next
    /// click. DeliveryService re-checks the job itself, so a button that was
    /// enabled on a stale list changes nothing and says why.
    /// </summary>
    public partial class DeliveryDashboard : Form
    {
        private enum View { Available, Mine, History }

        private readonly DeliveryService _deliveries = new DeliveryService();
        private readonly AuthService _auth = new AuthService();

        private View _view = View.Available;
        private bool _loading = true;

        private Panel[] _tiles;
        private Label _tileInHand;
        private Label _tileDeliveredToday;
        private Label _tileCashToday;
        private Label _tileFeesMonth;

        public DeliveryDashboard()
        {
            InitializeComponent();
        }

        private void DeliveryDashboard_Load(object sender, EventArgs e)
        {
            UiTheme.MakeResizable(this, Size);
            ApplyTheme();
            BuildTiles();
            Resize += (s, args) => LayoutTiles();

            dtpFrom.Value = DateTime.Today.AddDays(-30);
            dtpTo.Value = DateTime.Today;

            _loading = false;

            // Open where the work is: his own jobs if he has any, else the pool.
            try
            {
                bool hasJobs = _deliveries.GetMyActiveDeliveries(UserSession.UserId).Rows.Count > 0;
                ShowView(hasJobs ? View.Mine : View.Available);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load the dashboard.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Delivery Man");

            panelSide.BackColor = UiTheme.Sidebar;
            lblBrand.Font = UiTheme.FontBrand;
            lblBrand.ForeColor = Color.White;
            lblRole.Font = UiTheme.FontCaption;
            lblRole.ForeColor = UiTheme.SidebarRole;
            lblUserName.Font = UiTheme.FontSmall;
            lblUserName.ForeColor = UiTheme.SidebarUser;
            lblUserName.Text = UserSession.FullName;

            foreach (Button button in new[] { btnAvailable, btnMyDeliveries, btnHistory, btnMyProfile })
                UiTheme.StyleSidebarButton(button);

            UiTheme.StyleSidebarButton(btnLogout);
            btnLogout.BackColor = UiTheme.Danger;
            btnLogout.FlatAppearance.MouseOverBackColor = UiTheme.DangerHover;

            UiTheme.StyleHeader(panelHeader, lblHeaderTitle, lblHeaderSub);
            UiTheme.StyleSecondary(btnRefresh);

            lblSectionTitle.Font = UiTheme.FontHeading;
            lblSectionTitle.ForeColor = UiTheme.TextDark;
            lblSectionHint.Font = UiTheme.FontSmall;
            lblSectionHint.ForeColor = UiTheme.TextMuted;
            lblActionHint.Font = UiTheme.FontSmall;
            lblActionHint.ForeColor = UiTheme.Warning;
            lblFrom.Font = UiTheme.FontBody;
            lblTo.Font = UiTheme.FontBody;

            UiTheme.StyleGrid(dgvJobs);
            dgvJobs.DataBindingComplete += dgvJobs_DataBindingComplete;

            UiTheme.StyleSuccess(btnAccept);
            UiTheme.StylePrimary(btnPickedUp);
            UiTheme.StyleSuccess(btnDelivered);
            UiTheme.StyleSecondary(btnRelease);
            UiTheme.StyleSecondary(btnShowHistory);
        }

        private void BuildTiles()
        {
            Panel t1 = UiTheme.BuildTile("ORDERS IN HAND", UiTheme.Accent, out _tileInHand);
            Panel t2 = UiTheme.BuildTile("DELIVERED TODAY", UiTheme.Success, out _tileDeliveredToday);
            Panel t3 = UiTheme.BuildTile("CASH COLLECTED TODAY", UiTheme.Warning, out _tileCashToday);
            Panel t4 = UiTheme.BuildTile("DELIVERY FEES THIS MONTH", UiTheme.Primary, out _tileFeesMonth);

            _tiles = new[] { t1, t2, t3, t4 };
            LayoutTiles();
        }

        private void LayoutTiles()
        {
            if (_tiles == null) return;
            UiTheme.LayoutTileRow(this, panelSide, panelHeader, ClientSize.Width - dgvJobs.Right, _tiles);
        }

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
        //  LOADING
        // ---------------------------------------------------------------------

        private void ShowView(View view)
        {
            _view = view;

            foreach (Button button in new[] { btnAvailable, btnMyDeliveries, btnHistory })
                button.BackColor = UiTheme.Sidebar;
            (view == View.Available ? btnAvailable : view == View.Mine ? btnMyDeliveries : btnHistory)
                .BackColor = UiTheme.SidebarHover;

            bool history = view == View.History;
            lblFrom.Visible = history;
            dtpFrom.Visible = history;
            lblTo.Visible = history;
            dtpTo.Visible = history;
            btnShowHistory.Visible = history;

            btnAccept.Visible = view == View.Available;
            btnPickedUp.Visible = view == View.Mine;
            btnDelivered.Visible = view == View.Mine;
            btnRelease.Visible = view == View.Mine;

            LoadEverything();
        }

        private void LoadEverything()
        {
            if (_loading) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                DataRow summary = _deliveries.GetSummary(UserSession.UserId);
                _tileInHand.Text = DbHelper.GetInt(summary, "InHand").ToString("N0");
                _tileDeliveredToday.Text = DbHelper.GetInt(summary, "DeliveredToday").ToString("N0");
                _tileCashToday.Text = UiTheme.Money(DbHelper.GetDecimal(summary, "CashToday"));
                _tileFeesMonth.Text = UiTheme.Money(DbHelper.GetDecimal(summary, "FeesThisMonth"));

                DataTable available = _deliveries.GetAvailableOrders();
                lblHeaderSub.Text = UserSession.FullName +
                                    "   |   " + available.Rows.Count + " order(s) waiting for a delivery man" +
                                    "   |   " + DbHelper.GetInt(summary, "InHand") + " in your hands";

                switch (_view)
                {
                    case View.Available: BindAvailable(available); break;
                    case View.Mine: BindMine(); break;
                    default: BindHistory(); break;
                }
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

            UpdateButtons();
        }

        private void BindAvailable(DataTable table)
        {
            lblSectionTitle.Text = "Available orders  (" + table.Rows.Count + ")";
            lblSectionHint.Text = "Confirmed orders no delivery man has taken yet. Cash to collect is zero when the " +
                                  "customer has already paid online.";
            UiTheme.EnableEmptyMessage(dgvJobs, "No orders are waiting for a delivery man right now.");

            dgvJobs.DataSource = table;
            if (dgvJobs.Columns.Count == 0) return;

            dgvJobs.Columns["OrderId"].HeaderText = "Order";
            dgvJobs.Columns["OrderDate"].HeaderText = "Placed";
            dgvJobs.Columns["OrderDate"].DefaultCellStyle.Format = "dd MMM, HH:mm";
            dgvJobs.Columns["PharmacyName"].HeaderText = "Pick up from";
            dgvJobs.Columns["PickupArea"].HeaderText = "Area";
            dgvJobs.Columns["PickupAddress"].Visible = false;
            dgvJobs.Columns["DeliveryAddress"].HeaderText = "Deliver to";
            dgvJobs.Columns["Units"].HeaderText = "Units";
            dgvJobs.Columns["CashToCollect"].HeaderText = "Cash to collect";
            dgvJobs.Columns["CashToCollect"].DefaultCellStyle.Format = "N2";
            dgvJobs.Columns["DeliveryFee"].HeaderText = "Fee (Tk)";
            dgvJobs.Columns["DeliveryFee"].DefaultCellStyle.Format = "N2";

            UiTheme.SizeColumn(dgvJobs, "OrderId", 40, 55);
            UiTheme.SizeColumn(dgvJobs, "OrderDate", 70, 96);
            UiTheme.SizeColumn(dgvJobs, "PharmacyName", 100, 120);
            UiTheme.SizeColumn(dgvJobs, "PickupArea", 60, 80);
            UiTheme.SizeColumn(dgvJobs, "DeliveryAddress", 170, 160);
            UiTheme.SizeColumn(dgvJobs, "Units", 35, 50);
            UiTheme.SizeColumn(dgvJobs, "CashToCollect", 60, 100);
            UiTheme.SizeColumn(dgvJobs, "DeliveryFee", 45, 70);
        }

        private void BindMine()
        {
            DataTable table = _deliveries.GetMyActiveDeliveries(UserSession.UserId);

            lblSectionTitle.Text = "My deliveries  (" + table.Rows.Count + ")";
            lblSectionHint.Text = "Collect each order from the pharmacy and press Picked up, then press Delivered " +
                                  "when the customer has it.";
            UiTheme.EnableEmptyMessage(dgvJobs, "You have no deliveries in hand. Accept one from Available Orders.");

            dgvJobs.DataSource = table;
            if (dgvJobs.Columns.Count == 0) return;

            dgvJobs.Columns["OrderId"].HeaderText = "Order";
            dgvJobs.Columns["Status"].HeaderText = "Stage";
            dgvJobs.Columns["AcceptedAt"].HeaderText = "Accepted";
            dgvJobs.Columns["AcceptedAt"].DefaultCellStyle.Format = "dd MMM, HH:mm";
            dgvJobs.Columns["PickedUpAt"].Visible = false;
            dgvJobs.Columns["PharmacyName"].HeaderText = "Pharmacy";
            dgvJobs.Columns["PickupAddress"].HeaderText = "Pick up at";
            dgvJobs.Columns["PharmacyPhone"].HeaderText = "Pharmacy phone";
            dgvJobs.Columns["CustomerName"].HeaderText = "Customer";
            dgvJobs.Columns["CustomerPhone"].HeaderText = "Customer phone";
            dgvJobs.Columns["DeliveryAddress"].HeaderText = "Deliver to";
            dgvJobs.Columns["CashToCollect"].HeaderText = "Cash to collect";
            dgvJobs.Columns["CashToCollect"].DefaultCellStyle.Format = "N2";
            dgvJobs.Columns["PaymentMethod"].Visible = false;

            UiTheme.SizeColumn(dgvJobs, "OrderId", 40, 55);
            UiTheme.SizeColumn(dgvJobs, "Status", 55, 70);
            UiTheme.SizeColumn(dgvJobs, "AcceptedAt", 65, 96);
            UiTheme.SizeColumn(dgvJobs, "PharmacyName", 80, 100);
            UiTheme.SizeColumn(dgvJobs, "PickupAddress", 110, 120);
            UiTheme.SizeColumn(dgvJobs, "PharmacyPhone", 70, 90);
            UiTheme.SizeColumn(dgvJobs, "CustomerName", 75, 90);
            UiTheme.SizeColumn(dgvJobs, "CustomerPhone", 70, 90);
            UiTheme.SizeColumn(dgvJobs, "DeliveryAddress", 110, 120);
            UiTheme.SizeColumn(dgvJobs, "CashToCollect", 60, 90);
        }

        private void BindHistory()
        {
            if (dtpFrom.Value.Date > dtpTo.Value.Date)
            {
                lblSectionHint.ForeColor = UiTheme.Danger;
                lblSectionHint.Text = "The 'From' date must be on or before the 'To' date.";
                return;
            }
            lblSectionHint.ForeColor = UiTheme.TextMuted;

            DataTable table = _deliveries.GetHistory(UserSession.UserId, dtpFrom.Value, dtpTo.Value);

            decimal cash = 0m, fees = 0m;
            int delivered = 0;
            foreach (DataRow row in table.Rows)
            {
                cash += DbHelper.GetDecimal(row, "CashCollected");
                fees += DbHelper.GetDecimal(row, "DeliveryFee");
                if (DbHelper.GetString(row, "Status") == "Delivered") delivered++;
            }

            lblSectionTitle.Text = "Delivery history";
            lblSectionHint.Text = delivered + " delivered between " + dtpFrom.Value.ToString("dd MMM") + " and " +
                                  dtpTo.Value.ToString("dd MMM yyyy") + ".   Cash collected " + UiTheme.Money(cash) +
                                  ",   fees earned " + UiTheme.Money(fees) + ".";
            UiTheme.EnableEmptyMessage(dgvJobs, "No finished deliveries in this period.");

            dgvJobs.DataSource = table;
            if (dgvJobs.Columns.Count == 0) return;

            dgvJobs.Columns["OrderId"].HeaderText = "Order";
            dgvJobs.Columns["Status"].HeaderText = "Outcome";
            dgvJobs.Columns["FinishedAt"].HeaderText = "When";
            dgvJobs.Columns["FinishedAt"].DefaultCellStyle.Format = "dd MMM yyyy, HH:mm";
            dgvJobs.Columns["PharmacyName"].HeaderText = "Pharmacy";
            dgvJobs.Columns["DeliveryAddress"].HeaderText = "Delivered to";
            dgvJobs.Columns["CashCollected"].HeaderText = "Cash collected";
            dgvJobs.Columns["CashCollected"].DefaultCellStyle.Format = "N2";
            dgvJobs.Columns["DeliveryFee"].HeaderText = "Fee (Tk)";
            dgvJobs.Columns["DeliveryFee"].DefaultCellStyle.Format = "N2";

            UiTheme.SizeColumn(dgvJobs, "OrderId", 40, 55);
            UiTheme.SizeColumn(dgvJobs, "Status", 55, 80);
            UiTheme.SizeColumn(dgvJobs, "FinishedAt", 80, 130);
            UiTheme.SizeColumn(dgvJobs, "PharmacyName", 90, 120);
            UiTheme.SizeColumn(dgvJobs, "DeliveryAddress", 170, 160);
            UiTheme.SizeColumn(dgvJobs, "CashCollected", 60, 100);
            UiTheme.SizeColumn(dgvJobs, "DeliveryFee", 45, 70);
        }

        private void dgvJobs_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvJobs.Columns.Contains("Status")) return;

            foreach (DataGridViewRow row in dgvJobs.Rows)
            {
                string status = Convert.ToString(row.Cells["Status"].Value);
                if (status == "Assigned")
                    row.DefaultCellStyle.BackColor = UiTheme.PendingBack;     // still to collect
                else if (status == "Delivered")
                    row.DefaultCellStyle.BackColor = UiTheme.DeliveredBack;
                else if (status == "Released" || status == "Cancelled")
                    row.DefaultCellStyle.BackColor = UiTheme.InactiveBack;
                else
                    row.DefaultCellStyle.BackColor = Color.Empty;
            }
        }

        // ---------------------------------------------------------------------
        //  ACTIONS
        // ---------------------------------------------------------------------

        private void dgvJobs_SelectionChanged(object sender, EventArgs e) => UpdateButtons();

        private string SelectedStatus()
        {
            DataGridViewRow row = dgvJobs.CurrentRow;
            if (row == null || !dgvJobs.Columns.Contains("Status")) return "";
            return Convert.ToString(row.Cells["Status"].Value);
        }

        private decimal SelectedCashToCollect()
        {
            DataGridViewRow row = dgvJobs.CurrentRow;
            if (row == null || !dgvJobs.Columns.Contains("CashToCollect")) return 0m;
            object value = row.Cells["CashToCollect"].Value;
            return value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value);
        }

        private int SelectedOrderId()
        {
            if (dgvJobs.CurrentRow == null || !dgvJobs.Columns.Contains("OrderId")) return 0;
            object value = dgvJobs.CurrentRow.Cells["OrderId"].Value;
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private void UpdateButtons()
        {
            bool hasRow = SelectedOrderId() != 0;
            string status = SelectedStatus();

            btnAccept.Enabled = _view == View.Available && hasRow;
            btnPickedUp.Enabled = _view == View.Mine && status == "Assigned";
            btnDelivered.Enabled = _view == View.Mine && status == "PickedUp";
            btnRelease.Enabled = _view == View.Mine && status == "Assigned";

            string hint = "";
            if (_view == View.Mine && hasRow)
            {
                decimal cash = SelectedCashToCollect();
                hint = status == "Assigned"
                    ? "Go to the pharmacy and collect the order, then press Picked up."
                    : cash > 0
                        ? "Collect " + UiTheme.Money(cash) + " in cash from the customer before pressing Delivered."
                        : "Already paid online - collect no money. Press Delivered when the customer has it.";
            }
            lblActionHint.Text = hint;
        }

        /// <summary>Runs one delivery step and reloads, or explains why nothing changed.</summary>
        private void RunStep(Func<int, bool> step, string refusedMessage, string doneMessage)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;
            if (!SessionStillValid()) return;

            try
            {
                bool changed = step(orderId);
                LoadEverything();

                if (!changed)
                    MessageBox.Show("Order " + orderId + " " + refusedMessage + " The list has been refreshed.",
                        "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else if (doneMessage != null)
                    MessageBox.Show(doneMessage.Replace("{order}", orderId.ToString()),
                        "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The delivery could not be updated.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;
            if (!SessionStillValid()) return;

            try
            {
                string message;
                bool accepted = _deliveries.Accept(orderId, UserSession.UserId, out message);

                // A taken job belongs on My Deliveries; a lost race stays here, refreshed.
                if (accepted) ShowView(View.Mine);
                else LoadEverything();

                MessageBox.Show(message, accepted ? "Order accepted" : "Not available",
                    MessageBoxButtons.OK, accepted ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The order could not be accepted.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPickedUp_Click(object sender, EventArgs e)
        {
            RunStep(id => _deliveries.MarkPickedUp(id, UserSession.UserId),
                    "could not be marked picked up. It may have been cancelled by the pharmacy.", null);
        }

        private void btnDelivered_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;

            decimal cash = SelectedCashToCollect();
            string question = cash > 0
                ? "Have you collected " + UiTheme.Money(cash) + " in cash for order " + orderId + "?\r\n\r\n" +
                  "Press Yes only when the customer has the medicine and you have the money."
                : "Has the customer received order " + orderId + "?\r\n\r\nIt was paid online, so collect no money.";

            if (MessageBox.Show(question, "Confirm delivery", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes) return;

            RunStep(id => _deliveries.MarkDelivered(id, UserSession.UserId),
                    "could not be marked delivered because it is no longer in your hands.",
                    "Order {order} delivered." + (cash > 0 ? " " + UiTheme.Money(cash) + " cash recorded." : ""));
        }

        private void btnRelease_Click(object sender, EventArgs e)
        {
            int orderId = SelectedOrderId();
            if (orderId == 0) return;

            if (MessageBox.Show("Give order " + orderId + " back?\r\n\r\nIt goes back to the pool for another delivery man.",
                    "Give back", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            RunStep(id => _deliveries.Release(id, UserSession.UserId),
                    "could not be given back. Once an order is picked up it must be delivered.", null);
        }

        // ---------------------------------------------------------------------
        //  NAVIGATION
        // ---------------------------------------------------------------------

        private void btnAvailable_Click(object sender, EventArgs e)
        {
            if (SessionStillValid()) ShowView(View.Available);
        }

        private void btnMyDeliveries_Click(object sender, EventArgs e)
        {
            if (SessionStillValid()) ShowView(View.Mine);
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            if (SessionStillValid()) ShowView(View.History);
        }

        private void btnShowHistory_Click(object sender, EventArgs e)
        {
            if (SessionStillValid()) LoadEverything();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (SessionStillValid()) LoadEverything();
        }

        private void btnMyProfile_Click(object sender, EventArgs e)
        {
            using (MyProfileForm profile = new MyProfileForm())
            {
                if (!SessionStillValid()) return;
                profile.ShowDialog(this);
            }
            LoadEverything();
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
