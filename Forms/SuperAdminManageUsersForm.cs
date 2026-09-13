using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 4. Every Admin and Customer in one DataGridView, with the
    /// pharmacy name and status filled in beside an owner's row through a LEFT
    /// JOIN on Pharmacies. Both filters are optional, and an empty box means no
    /// filter rather than no results.
    ///
    /// Suspending or activating a pharmacy owner keeps the shop in step with
    /// the account (see AuthService.SetUserStatus): the account switch never
    /// leaves an Approved shop behind a suspended owner.
    /// </summary>
    public partial class SuperAdminManageUsersForm : Form
    {
        private readonly AuthService _auth = new AuthService();
        private bool _loading = true;

        public SuperAdminManageUsersForm()
        {
            InitializeComponent();
        }

        private void SuperAdminManageUsersForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            UiTheme.MakeResizable(this, Size);

            cmbStatus.Items.AddRange(new object[] { "All statuses", "Pending", "Active", "Suspended" });
            cmbStatus.SelectedIndex = 0;

            cmbUserType.Items.AddRange(new object[] { "All users", "Admin", "Customer" });
            cmbUserType.SelectedIndex = 0;

            _loading = false;
            LoadGrid();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Manage Users");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSecondary(btnSearch);
            UiTheme.StyleSecondary(btnClear);
            UiTheme.StyleDanger(btnSuspend);
            UiTheme.StyleSuccess(btnActivate);
            UiTheme.StyleGrid(dgvUsers);
            UiTheme.EnableEmptyMessage(dgvUsers, "No account matches these filters.");
            dgvUsers.CellFormatting += dgvUsers_CellFormatting;

            lblNote.Font = UiTheme.FontSmall;
            lblNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;
        }

        private void SetStatus(string text, bool isProblem)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = isProblem ? UiTheme.Danger : UiTheme.TextMuted;
        }

        /// <summary>Reloads the grid, keeping the same account selected. Returns false when loading failed.</summary>
        private bool LoadGrid()
        {
            if (_loading) return false;

            try
            {
                int keepId = SelectedUserId();

                string status = cmbStatus.SelectedIndex <= 0 ? "" : cmbStatus.SelectedItem.ToString();
                string type = cmbUserType.SelectedIndex <= 0 ? "" : cmbUserType.SelectedItem.ToString();

                DataTable table = _auth.SearchUsers(txtSearch.Text.Trim(), status, type);
                dgvUsers.DataSource = table;

                if (dgvUsers.Columns.Count > 0)
                {
                    dgvUsers.Columns["UserId"].HeaderText = "ID";
                    dgvUsers.Columns["UserId"].FillWeight = 30;
                    dgvUsers.Columns["FullName"].HeaderText = "Name";
                    dgvUsers.Columns["Email"].HeaderText = "Email";
                    dgvUsers.Columns["Phone"].HeaderText = "Mobile";
                    dgvUsers.Columns["UserType"].HeaderText = "Role";
                    dgvUsers.Columns["UserType"].FillWeight = 45;
                    dgvUsers.Columns["Status"].HeaderText = "Status";
                    dgvUsers.Columns["Status"].FillWeight = 45;
                    dgvUsers.Columns["PharmacyName"].HeaderText = "Pharmacy (owners only)";
                    dgvUsers.Columns["PharmacyStatus"].HeaderText = "Pharmacy status";
                    dgvUsers.Columns["PharmacyStatus"].FillWeight = 55;
                    dgvUsers.Columns["CreatedAt"].HeaderText = "Member since";
                }

                if (keepId > 0)
                {
                    foreach (DataGridViewRow row in dgvUsers.Rows)
                    {
                        if (Convert.ToInt32(row.Cells["UserId"].Value) == keepId)
                        {
                            dgvUsers.CurrentCell = row.Cells["FullName"];
                            break;
                        }
                    }
                }

                SetStatus(table.Rows.Count + " account(s) shown.", false);
                UpdateButtons();
                return true;
            }
            catch (Exception ex)
            {
                SetStatus("The account list could not be loaded.", true);
                MessageBox.Show("The account list could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void dgvUsers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || !dgvUsers.Columns.Contains("Status")) return;

            object value = dgvUsers.Rows[e.RowIndex].Cells["Status"].Value;
            if (value == null) return;

            switch (value.ToString())
            {
                case "Pending": e.CellStyle.BackColor = UiTheme.PendingBack; break;
                case "Suspended": e.CellStyle.BackColor = UiTheme.LowStockBack; break;
            }
        }

        private void dgvUsers_SelectionChanged(object sender, EventArgs e) => UpdateButtons();

        private int SelectedUserId()
        {
            if (dgvUsers.CurrentRow == null || !dgvUsers.Columns.Contains("UserId")) return 0;
            return Convert.ToInt32(dgvUsers.CurrentRow.Cells["UserId"].Value);
        }

        private void UpdateButtons()
        {
            DataGridViewRow row = dgvUsers.CurrentRow;
            bool hasRow = row != null && dgvUsers.Columns.Contains("Status") && row.Cells["Status"].Value != null;
            string status = hasRow ? row.Cells["Status"].Value.ToString() : "";

            btnSuspend.Enabled = hasRow && status != "Suspended";
            btnActivate.Enabled = hasRow && status != "Active";
        }

        private void ChangeStatus(string newStatus)
        {
            DataGridViewRow row = dgvUsers.CurrentRow;
            if (row == null) return;

            int userId = Convert.ToInt32(row.Cells["UserId"].Value);
            string name = row.Cells["FullName"].Value.ToString();
            string role = row.Cells["UserType"].Value.ToString();
            string pharmacy = row.Cells["PharmacyName"].Value.ToString();
            string pharmacyStatus = row.Cells["PharmacyStatus"].Value.ToString();

            // Tell the Super Admin what else will happen before asking.
            string consequence = "";
            if (role == "Admin" && newStatus == "Suspended" && pharmacyStatus == "Approved")
                consequence = "\r\n\r\nTheir pharmacy, " + pharmacy + ", will be suspended too, so customers can no longer see it.";
            else if (role == "Admin" && newStatus == "Active" && pharmacyStatus == "Suspended")
                consequence = "\r\n\r\nTheir pharmacy, " + pharmacy + ", will be reinstated at the same time.";

            DialogResult answer = MessageBox.Show(
                "Set " + name + "'s account to " + newStatus + "?" + consequence,
                "Change account status", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes) return;

            string message;
            bool changed;
            try
            {
                changed = _auth.SetUserStatus(userId, newStatus, out message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not change " + name + "'s account.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!LoadGrid()) return;

            SetStatus(name + ": " + message, !changed);
            if (!changed)
                MessageBox.Show(message, "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSuspend_Click(object sender, EventArgs e) => ChangeStatus("Suspended");
        private void btnActivate_Click(object sender, EventArgs e) => ChangeStatus("Active");
        private void Filter_Changed(object sender, EventArgs e) => LoadGrid();
        private void btnSearch_Click(object sender, EventArgs e) => LoadGrid();

        private void btnClear_Click(object sender, EventArgs e)
        {
            _loading = true;
            txtSearch.Clear();
            cmbStatus.SelectedIndex = 0;
            cmbUserType.SelectedIndex = 0;
            _loading = false;
            LoadGrid();
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
