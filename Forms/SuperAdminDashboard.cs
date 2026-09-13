using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// The platform operator's hub (requirements 1 to 9).
    ///
    /// Four summary tiles, the queue of pharmacies waiting for approval, and the
    /// low rated pharmacy panel that comes straight from the
    /// HAVING AVG(Rating) &lt; 2.5 query. The left menu is the entry point to
    /// every other Super Admin form, and every one of them has a Back button.
    ///
    /// Before any child screen opens, and on Refresh, the session is checked
    /// again against the database, so a Super Admin account suspended or
    /// deleted mid-session is signed out at the next click.
    /// </summary>
    public partial class SuperAdminDashboard : Form
    {
        private readonly PharmacyService _pharmacies = new PharmacyService();
        private readonly ReportService _reports = new ReportService();
        private readonly AuthService _auth = new AuthService();

        private Label _tilePharmacies;
        private Label _tileCustomers;
        private Label _tileOrders;
        private Label _tileCommission;
        private Panel[] _tiles;

        public SuperAdminDashboard()
        {
            InitializeComponent();
        }

        private void SuperAdminDashboard_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            BuildTiles();
            UiTheme.MakeResizable(this, Size);
            LayoutTiles();
            Resize += (s, args) => LayoutTiles();
            LoadEverything();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Super Admin");

            panelSide.BackColor = UiTheme.Sidebar;
            lblBrand.Font = UiTheme.FontBrand;
            lblBrand.ForeColor = Color.White;
            lblRole.Font = UiTheme.FontCaption;
            lblRole.ForeColor = UiTheme.SidebarRole;
            lblUserName.Font = UiTheme.FontSmall;
            lblUserName.ForeColor = UiTheme.SidebarUser;
            lblUserName.Text = UserSession.FullName;

            foreach (Button button in new[] { btnManagePharmacies, btnManageUsers, btnCategories,
                                              btnSalesReport, btnLowRated, btnModerateReviews })
            {
                UiTheme.StyleSidebarButton(button);
            }

            UiTheme.StyleSidebarButton(btnLogout);
            btnLogout.BackColor = UiTheme.Danger;
            btnLogout.FlatAppearance.MouseOverBackColor = UiTheme.DangerHover;

            UiTheme.StyleHeader(panelHeader, lblHeaderTitle, lblHeaderSub);
            UiTheme.StyleSecondary(btnRefresh);

            lblPendingTitle.Font = UiTheme.FontHeading;
            lblPendingTitle.ForeColor = UiTheme.TextDark;
            lblPendingHint.Font = UiTheme.FontSmall;
            lblPendingHint.ForeColor = UiTheme.TextMuted;

            lblLowRatedTitle.Font = UiTheme.FontHeading;
            lblLowRatedTitle.ForeColor = UiTheme.TextDark;
            lblLowRatedHint.Font = UiTheme.FontSmall;
            lblLowRatedHint.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleGrid(dgvPending);
            UiTheme.StyleGrid(dgvLowRated);
            UiTheme.EnableEmptyMessage(dgvPending, "No pharmacy registrations are waiting for approval.");
            UiTheme.EnableEmptyMessage(dgvLowRated, "No pharmacy is rated below 2.5 with at least 2 reviews.");
            dgvLowRated.CellFormatting += dgvLowRated_CellFormatting;

            UiTheme.StyleSuccess(btnApprove);
            UiTheme.StyleDanger(btnReject);
            UiTheme.StyleSecondary(btnOpenPharmacies);
        }

        /// <summary>
        /// The four headline figures. They are built in code, and positioned by
        /// UiTheme.LayoutTileRow from the real sidebar and header, so they can
        /// never overlap either one, and they share the width again whenever the
        /// window is resized.
        /// </summary>
        private void BuildTiles()
        {
            Panel t1 = UiTheme.BuildTile("APPROVED PHARMACIES", UiTheme.Primary, out _tilePharmacies);
            Panel t2 = UiTheme.BuildTile("ACTIVE CUSTOMERS", UiTheme.Accent, out _tileCustomers);
            Panel t3 = UiTheme.BuildTile("CONFIRMED + DELIVERED ORDERS", UiTheme.Success, out _tileOrders);
            Panel t4 = UiTheme.BuildTile("COMMISSION EARNED", UiTheme.Warning, out _tileCommission);
            _tiles = new[] { t1, t2, t3, t4 };
        }

        private void LayoutTiles()
        {
            if (_tiles == null) return;
            UiTheme.LayoutTileRow(this, panelSide, panelHeader, ClientSize.Width - dgvPending.Right, _tiles);
        }

        // ---------------------------------------------------------------------
        //  DATA
        // ---------------------------------------------------------------------

        private void LoadEverything()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                int pharmacies, pending, customers, orders;
                decimal revenue, commission;
                _reports.GetPlatformTotals(out pharmacies, out pending, out customers,
                                           out orders, out revenue, out commission);

                _tilePharmacies.Text = pharmacies.ToString();
                _tileCustomers.Text = customers.ToString();
                _tileOrders.Text = orders.ToString();
                _tileCommission.Text = UiTheme.Money(commission);

                lblHeaderSub.Text = "Item sales (confirmed + delivered) " + UiTheme.Money(revenue) +
                                    "   |   " + pending + " pharmacy registration(s) waiting for approval";

                dgvPending.DataSource = _pharmacies.GetPending();
                LabelPendingColumns();

                dgvLowRated.DataSource = _reports.GetLowRatedPharmacies(2.5m, 2);
                LabelLowRatedColumns();

                bool hasPending = dgvPending.Rows.Count > 0;
                btnApprove.Enabled = hasPending;
                btnReject.Enabled = hasPending;
                lblPendingTitle.Text = hasPending
                    ? "Pharmacies waiting for approval  (" + dgvPending.Rows.Count + ")"
                    : "Pharmacies waiting for approval  -  none right now";
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

        private void LabelPendingColumns()
        {
            if (dgvPending.Columns.Count == 0) return;
            dgvPending.Columns["PharmacyId"].HeaderText = "ID";
            dgvPending.Columns["PharmacyId"].FillWeight = 30;
            dgvPending.Columns["PharmacyName"].HeaderText = "Pharmacy";
            dgvPending.Columns["OwnerName"].HeaderText = "Owner";
            dgvPending.Columns["LicenseNo"].HeaderText = "DGDA licence";
            dgvPending.Columns["Area"].HeaderText = "Area";
            dgvPending.Columns["ContactPhone"].HeaderText = "Contact";
            dgvPending.Columns["RegisteredAt"].HeaderText = "Applied on";
        }

        private void LabelLowRatedColumns()
        {
            if (dgvLowRated.Columns.Count == 0) return;
            dgvLowRated.Columns["PharmacyId"].HeaderText = "ID";
            dgvLowRated.Columns["PharmacyId"].FillWeight = 30;
            dgvLowRated.Columns["PharmacyName"].HeaderText = "Pharmacy";
            dgvLowRated.Columns["Area"].HeaderText = "Area";
            dgvLowRated.Columns["OwnerName"].HeaderText = "Owner";
            dgvLowRated.Columns["OwnerPhone"].HeaderText = "Owner phone";
            dgvLowRated.Columns["TotalReviews"].HeaderText = "Reviews";
            dgvLowRated.Columns["AverageRating"].HeaderText = "Avg rating";
            dgvLowRated.Columns["Status"].HeaderText = "Status";
        }

        /// <summary>Poor performers are tinted red through the CellFormatting event.</summary>
        private void dgvLowRated_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            e.CellStyle.BackColor = UiTheme.LowStockBack;
        }

        // ---------------------------------------------------------------------
        //  SESSION
        // ---------------------------------------------------------------------

        /// <summary>
        /// Re-reads this account's status. When the session is no longer valid
        /// the user is told why, the session is cleared and the dashboard closes,
        /// exactly like Log out (LoginForm is watching FormClosed).
        /// </summary>
        private bool EnsureSessionValid()
        {
            string message;
            try
            {
                if (_auth.CheckSessionStillValid(out message)) return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not check your session.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            MessageBox.Show(message, "Signed out", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            UserSession.Clear();
            Close();
            return false;
        }

        // ---------------------------------------------------------------------
        //  ACTIONS ON THE PENDING QUEUE
        // ---------------------------------------------------------------------

        private int SelectedPendingPharmacyId()
        {
            if (dgvPending.CurrentRow == null || !dgvPending.Columns.Contains("PharmacyId")) return 0;
            return Convert.ToInt32(dgvPending.CurrentRow.Cells["PharmacyId"].Value);
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            int pharmacyId = SelectedPendingPharmacyId();
            if (pharmacyId == 0)
            {
                MessageBox.Show("Select a pharmacy first.", "PharmaLink",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = dgvPending.CurrentRow.Cells["PharmacyName"].Value.ToString();
            string licence = dgvPending.CurrentRow.Cells["LicenseNo"].Value.ToString();

            DialogResult answer = MessageBox.Show(
                "Approve " + name + "?\r\n\r\nDGDA licence: " + licence + "\r\n\r\n" +
                "The pharmacy becomes Approved, the owner's account becomes Active and can log in, " +
                "and the medicines the shop lists become visible to customers.",
                "Approve pharmacy", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes) return;
            if (!EnsureSessionValid()) return;

            int changed;
            try
            {
                changed = _pharmacies.Approve(pharmacyId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(name + " could not be approved.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadEverything();

            if (changed == 1)
                MessageBox.Show(name + " has been approved. The owner can now log in.", "Pharmacy approved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(name + " was not changed: it is no longer waiting for approval. " +
                                "The list has been refreshed to show the current state.", "Nothing changed",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            int pharmacyId = SelectedPendingPharmacyId();
            if (pharmacyId == 0)
            {
                MessageBox.Show("Select a pharmacy first.", "PharmaLink",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = dgvPending.CurrentRow.Cells["PharmacyName"].Value.ToString();

            DialogResult answer = MessageBox.Show(
                "Reject " + name + "?\r\n\r\nThe registration is marked Rejected and the owner's account is " +
                "suspended. Nothing is deleted, so the licence number stays taken and the pharmacy can still " +
                "be approved later from Manage Pharmacies.",
                "Reject registration", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes) return;
            if (!EnsureSessionValid()) return;

            int changed;
            try
            {
                changed = _pharmacies.Reject(pharmacyId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(name + " could not be rejected.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadEverything();

            if (changed == 1)
                MessageBox.Show(name + " has been rejected.", "Registration rejected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(name + " was not changed: it is no longer waiting for approval. " +
                                "The list has been refreshed to show the current state.", "Nothing changed",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgvLowRated_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int pharmacyId = Convert.ToInt32(dgvLowRated.Rows[e.RowIndex].Cells["PharmacyId"].Value);
            OpenChild(new SuperAdminManageShopsForm(pharmacyId));
        }

        // ---------------------------------------------------------------------
        //  NAVIGATION
        // ---------------------------------------------------------------------

        /// <summary>
        /// Every child form is opened as a modal dialog and the dashboard
        /// refreshes when it closes, which is how the navigation diagram's
        /// "labelled Back arrow" is implemented in practice. The session is
        /// checked first; if it has ended the child is never shown.
        /// </summary>
        private void OpenChild(Form child)
        {
            using (child)
            {
                if (!EnsureSessionValid()) return;
                child.ShowDialog(this);
            }
            LoadEverything();
        }

        private void btnManagePharmacies_Click(object sender, EventArgs e) => OpenChild(new SuperAdminManageShopsForm(0));
        private void btnManageUsers_Click(object sender, EventArgs e) => OpenChild(new SuperAdminManageUsersForm());
        private void btnCategories_Click(object sender, EventArgs e) => OpenChild(new ManageCategoriesForm());
        private void btnSalesReport_Click(object sender, EventArgs e) => OpenChild(new SuperAdminSalesReportForm());
        private void btnLowRated_Click(object sender, EventArgs e) => OpenChild(new SuperAdminLowRatedShopsForm());
        private void btnModerateReviews_Click(object sender, EventArgs e) => OpenChild(new ModerateReviewsForm());

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (EnsureSessionValid()) LoadEverything();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show("Log out of PharmaLink?", "Log out",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer == DialogResult.Yes)
            {
                UserSession.Clear();
                Close();      // LoginForm is watching FormClosed and shows itself again
            }
        }
    }
}
