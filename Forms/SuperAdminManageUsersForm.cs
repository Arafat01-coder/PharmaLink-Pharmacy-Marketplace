using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
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
    ///
    /// This is also the Super Admin's end of the help-desk password reset.
    /// Requests made from the login screen show in the "Reset requested"
    /// column (and sort to the top); Reset password issues a random temporary
    /// password that is shown exactly once, to be read to the user over the
    /// phone on their registered mobile number, never stored in plain text.
    /// </summary>
    public partial class SuperAdminManageUsersForm : Form
    {
        /// <summary>Kept short so it fits the Status box without clipping.</summary>
        private const string ResetRequestedFilter = "Reset requested";

        private readonly AuthService _auth = new AuthService();
        private readonly bool _startWithResetRequests;
        private bool _loading = true;

        public SuperAdminManageUsersForm()
        {
            InitializeComponent();
        }

        /// <summary>Opens already filtered to waiting reset requests (the dashboard's link).</summary>
        public SuperAdminManageUsersForm(bool resetRequestsOnly) : this()
        {
            _startWithResetRequests = resetRequestsOnly;
        }

        private void SuperAdminManageUsersForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            UiTheme.MakeResizable(this, Size);

            cmbStatus.Items.AddRange(new object[] { "All statuses", "Pending", "Active", "Suspended", ResetRequestedFilter });
            cmbStatus.SelectedIndex = _startWithResetRequests ? cmbStatus.Items.IndexOf(ResetRequestedFilter) : 0;

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
            UiTheme.StyleAccent(btnResetPassword);
            UiTheme.StyleGrid(dgvUsers);
            UiTheme.EnableEmptyMessage(dgvUsers, "No account matches these filters.");
            dgvUsers.CellFormatting += dgvUsers_CellFormatting;

            lblNote.Font = UiTheme.FontSmall;
            lblNote.ForeColor = UiTheme.TextMuted;
            lblNote.Text =
                "Suspending a pharmacy owner also suspends their approved pharmacy; activating an owner reinstates " +
                "a suspended one (approve pending or rejected registrations from Manage Pharmacies). Reset password " +
                "gives the user a temporary password to read to them by phone; they must replace it when they sign in.";
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

                string selected = cmbStatus.SelectedIndex <= 0 ? "" : cmbStatus.SelectedItem.ToString();
                bool resetOnly = selected == ResetRequestedFilter;
                string status = resetOnly ? "" : selected;
                string type = cmbUserType.SelectedIndex <= 0 ? "" : cmbUserType.SelectedItem.ToString();

                DataTable table = _auth.SearchUsers(txtSearch.Text.Trim(), status, type, resetOnly);
                dgvUsers.DataSource = table;

                if (dgvUsers.Columns.Count > 0)
                {
                    dgvUsers.Columns["UserId"].HeaderText = "ID";
                    dgvUsers.Columns["FullName"].HeaderText = "Name";
                    dgvUsers.Columns["Email"].HeaderText = "Email";
                    dgvUsers.Columns["Phone"].HeaderText = "Mobile";
                    dgvUsers.Columns["UserType"].HeaderText = "Role";
                    dgvUsers.Columns["Status"].HeaderText = "Status";
                    // Short headers: with ten columns, longer ones wrapped to one word.
                    // A customer's row shows "-" here, so "owners only" goes without saying.
                    dgvUsers.Columns["PharmacyName"].HeaderText = "Pharmacy";
                    dgvUsers.Columns["PharmacyStatus"].HeaderText = "Shop status";
                    dgvUsers.Columns["CreatedAt"].HeaderText = "Joined";
                    dgvUsers.Columns["CreatedAt"].DefaultCellStyle.Format = "dd-MMM-yy";
                    dgvUsers.Columns["PasswordResetRequestedAt"].HeaderText = "Reset requested";
                    dgvUsers.Columns["PasswordResetRequestedAt"].DefaultCellStyle.Format = "dd-MMM-yy HH:mm";

                    // Ten columns share the grid, so each gets a floor that keeps
                    // its header and a typical value readable (the floors add up
                    // to well under the grid's width at 100% scaling).
                    UiTheme.SizeColumn(dgvUsers, "UserId", 25, 35);
                    UiTheme.SizeColumn(dgvUsers, "FullName", 95, 100);
                    UiTheme.SizeColumn(dgvUsers, "Email", 125, 130);
                    UiTheme.SizeColumn(dgvUsers, "Phone", 70, 90);
                    UiTheme.SizeColumn(dgvUsers, "UserType", 55, 75);
                    UiTheme.SizeColumn(dgvUsers, "Status", 50, 62);
                    UiTheme.SizeColumn(dgvUsers, "PharmacyName", 95, 130);
                    UiTheme.SizeColumn(dgvUsers, "PharmacyStatus", 65, 110);
                    UiTheme.SizeColumn(dgvUsers, "CreatedAt", 55, 80);
                    UiTheme.SizeColumn(dgvUsers, "PasswordResetRequestedAt", 80, 125);
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

                // Rebinding can leave the first row highlighted with no current
                // cell, which would keep every action button disabled.
                UiTheme.EnsureCurrentCell(dgvUsers, "FullName");

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

            // A waiting reset request is the one cell the Super Admin must act on.
            if (dgvUsers.Columns[e.ColumnIndex].Name == "PasswordResetRequestedAt")
            {
                if (e.Value != null && e.Value != DBNull.Value)
                {
                    e.CellStyle.BackColor = UiTheme.WarningBack;
                    return;
                }
            }

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
            string role = hasRow ? Convert.ToString(row.Cells["UserType"].Value) : "";

            btnSuspend.Enabled = hasRow && status != "Suspended";
            btnActivate.Enabled = hasRow && status != "Active";

            // The grid never lists the Super Admin, but the role is checked
            // anyway: the button must mean "Admin or Customer" by itself.
            btnResetPassword.Enabled = hasRow && (role == "Admin" || role == "Customer");
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

        // ---------------------------------------------------------------------
        //  PASSWORD RESET
        // ---------------------------------------------------------------------

        /// <summary>
        /// Confirms, issues the temporary password, and shows it once. The
        /// password lives only in a local variable and the dialog's text box; it
        /// is not written to the status line, a log or the grid.
        /// </summary>
        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dgvUsers.CurrentRow;
            if (row == null) return;

            int userId = Convert.ToInt32(row.Cells["UserId"].Value);
            string name = row.Cells["FullName"].Value.ToString();
            string phone = row.Cells["Phone"].Value.ToString();
            string role = row.Cells["UserType"].Value.ToString();
            string status = row.Cells["Status"].Value.ToString();
            bool requested = row.Cells["PasswordResetRequestedAt"].Value is DateTime;

            if (role != "Admin" && role != "Customer") return;

            string warning = requested
                ? ""
                : "\r\n\r\nThis user has NOT asked for a reset. Only continue if you have spoken to them.";
            if (status != "Active")
                warning += "\r\n\r\nThe account is " + status + ", so they still cannot sign in until it is active.";

            DialogResult answer = MessageBox.Show(
                "Issue a temporary password for " + name + "?\r\n\r\n" +
                "Their current password stops working immediately. You will see the temporary password once; " +
                "phone " + name + " on their registered mobile number " + phone + " and read it to them. " +
                "They must choose a new password when they next sign in." + warning,
                "Reset password", MessageBoxButtons.YesNo, requested ? MessageBoxIcon.Question : MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes) return;

            string tempPassword;
            bool issued;
            try
            {
                issued = _auth.IssueTemporaryPassword(userId, out tempPassword);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not reset " + name + "'s password.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!issued)
            {
                if (LoadGrid())
                    SetStatus(name + ": the password was not reset because the account no longer exists.", true);
                MessageBox.Show("The password was not reset because " + name + "'s account no longer exists.",
                    "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ShowTemporaryPassword(name, phone, tempPassword);
            tempPassword = null;

            if (LoadGrid())
                SetStatus(name + ": temporary password issued. They must choose a new password at their next sign-in.", false);
        }

        /// <summary>
        /// The one-time display of a temporary password: a read-only, selectable
        /// text box in a monospaced font (so it can be read out character by
        /// character), a Copy button, and the instruction to phone the user.
        ///
        /// Built in code rather than as a separate designer form because it is a
        /// few controls used from this one screen; it uses the same AutoScale
        /// settings as the designer forms so it scales with Windows display
        /// scaling like every other dialog. The text box is emptied as it closes.
        /// </summary>
        private void ShowTemporaryPassword(string name, string phone, string tempPassword)
        {
            using (Form dialog = new Form())
            {
                Panel header = UiTheme.BuildHeader("Temporary password", "For " + name);

                Label lblInstruction = new Label
                {
                    Name = "lblTempInstruction",
                    AutoSize = false,
                    Location = new Point(20, 84),
                    Size = new Size(420, 58),
                    Font = UiTheme.FontBody,
                    ForeColor = UiTheme.TextDark,
                    Text = "Phone " + name + " on their registered mobile number " + phone +
                           " and read this password to them. It is shown only this once - it cannot be looked up later."
                };

                TextBox txtTemp = new TextBox
                {
                    Name = "txtTempPassword",
                    ReadOnly = true,
                    Location = new Point(20, 150),
                    Size = new Size(290, 27),
                    Font = UiTheme.FontMono,
                    BackColor = UiTheme.ReadOnlyBack,
                    Text = tempPassword
                };

                Button btnCopy = new Button
                {
                    Name = "btnCopyTempPassword",
                    Location = new Point(320, 148),
                    Size = new Size(120, 32),
                    Text = "Copy"
                };

                Label lblCopied = new Label
                {
                    Name = "lblTempCopied",
                    AutoSize = false,
                    Location = new Point(20, 186),
                    Size = new Size(420, 36),
                    Font = UiTheme.FontSmall,
                    ForeColor = UiTheme.TextMuted,
                    Text = "When they sign in with it, PharmaLink makes them choose their own password straight away."
                };

                Button btnDone = new Button
                {
                    Name = "btnTempDone",
                    Location = new Point(320, 232),
                    Size = new Size(120, 38),
                    Text = "Done",
                    DialogResult = DialogResult.OK
                };

                btnCopy.Click += (s, args) =>
                {
                    try
                    {
                        Clipboard.SetText(txtTemp.Text);
                        lblCopied.ForeColor = UiTheme.Success;
                        lblCopied.Text = "Copied. Clear your clipboard once you have read it to the user.";
                    }
                    catch (ExternalException)
                    {
                        lblCopied.ForeColor = UiTheme.Danger;
                        lblCopied.Text = "The clipboard is busy. Select the password and press Ctrl+C instead.";
                    }
                };

                dialog.SuspendLayout();
                dialog.AutoScaleDimensions = new SizeF(7F, 15F);
                dialog.AutoScaleMode = AutoScaleMode.Font;
                dialog.ClientSize = new Size(460, 286);
                dialog.Controls.Add(btnDone);
                dialog.Controls.Add(lblCopied);
                dialog.Controls.Add(btnCopy);
                dialog.Controls.Add(txtTemp);
                dialog.Controls.Add(lblInstruction);
                dialog.Controls.Add(header);
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.Name = "TemporaryPasswordDialog";
                dialog.ResumeLayout(false);
                dialog.PerformLayout();

                UiTheme.StyleForm(dialog, "Temporary password");
                dialog.StartPosition = FormStartPosition.CenterParent;
                UiTheme.StyleAccent(btnCopy);
                UiTheme.StylePrimary(btnDone);
                btnDone.Font = UiTheme.FontButtonStrong;
                dialog.AcceptButton = btnDone;
                dialog.CancelButton = btnDone;
                dialog.Shown += (s, args) => { txtTemp.Focus(); txtTemp.SelectAll(); };
                dialog.FormClosed += (s, args) => txtTemp.Clear();

                dialog.ShowDialog(this);
            }
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
