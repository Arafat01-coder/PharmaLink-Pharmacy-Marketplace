using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirements 2, 3 and 9.
    ///
    /// Approve, reject, suspend and reinstate each change the pharmacy and its
    /// owner's account together, and each is only offered from the status it
    /// can start from (see PharmacyService). The commission rate is a column on
    /// Pharmacies rather than a constant in the code, so it can be changed for
    /// one shop without touching anyone else.
    /// </summary>
    public partial class SuperAdminManageShopsForm : Form
    {
        private readonly PharmacyService _pharmacies = new PharmacyService();
        private readonly int _preselectPharmacyId;
        private bool _loading = true;

        /// <summary>The preselected pharmacy is applied on the first load only; later reloads keep the user's own selection.</summary>
        private bool _firstLoadDone;

        /// <summary>Whether the selected row's status allows a commission change at all.</summary>
        private bool _commissionAllowed;

        /// <summary>Typing in the search box reloads the grid 300 ms after the last keystroke, not on every key.</summary>
        private readonly System.Windows.Forms.Timer _searchDelay = new System.Windows.Forms.Timer { Interval = 300 };

        public SuperAdminManageShopsForm(int preselectPharmacyId)
        {
            InitializeComponent();
            _preselectPharmacyId = preselectPharmacyId;
            _searchDelay.Tick += SearchDelay_Tick;
            FormClosed += (s, e) => _searchDelay.Dispose();
        }

        private void SuperAdminManageShopsForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            UiTheme.MakeResizable(this, Size);
            LoadFilters();
            _loading = false;
            LoadGrid();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Manage Pharmacies");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);
            UiTheme.StyleSecondary(btnBack);

            UiTheme.StyleGrid(dgvPharmacies);
            UiTheme.EnableEmptyMessage(dgvPharmacies, "No pharmacy matches these filters.");
            dgvPharmacies.CellFormatting += dgvPharmacies_CellFormatting;
            dgvPharmacies.CellToolTipTextNeeded += dgvPharmacies_CellToolTipTextNeeded;

            UiTheme.StyleSuccess(btnApprove);
            UiTheme.StyleDanger(btnSuspend);
            UiTheme.StyleAccent(btnReinstate);
            UiTheme.StyleDanger(btnDelete);
            UiTheme.StylePrimary(btnSetCommission);
            UiTheme.StyleSecondary(btnSearch);
            UiTheme.StyleSecondary(btnClear);

            lblNote.Font = UiTheme.FontSmall;
            lblNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;
            lblCommissionError.Font = UiTheme.FontSmall;
            lblCommissionError.ForeColor = UiTheme.Danger;
        }

        private void LoadFilters()
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.AddRange(new object[] { "All statuses", "Pending", "Approved", "Suspended", "Rejected" });
            cmbStatus.SelectedIndex = 0;

            cmbArea.Items.Clear();
            cmbArea.Items.Add("All areas");
            try
            {
                foreach (string area in _pharmacies.GetAreas(false)) cmbArea.Items.Add(area);
            }
            catch (Exception ex)
            {
                SetStatus("The area filter could not be loaded, so only \"All areas\" is offered. " +
                          DbHelper.Describe(ex).Replace("\r\n\r\n", " "), true);
            }
            cmbArea.SelectedIndex = 0;
        }

        private void SetStatus(string text, bool isProblem)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = isProblem ? UiTheme.Danger : UiTheme.TextMuted;
        }

        // ---------------------------------------------------------------------

        /// <summary>
        /// Reloads the grid and puts the selection back on the same pharmacy
        /// (or, on the very first load, on the pharmacy the caller asked for).
        /// Returns false when the list could not be loaded.
        /// </summary>
        private bool LoadGrid()
        {
            if (_loading) return false;

            try
            {
                int keepId = _firstLoadDone ? SelectedId() : _preselectPharmacyId;

                string status = cmbStatus.SelectedIndex <= 0 ? "" : cmbStatus.SelectedItem.ToString();
                string area = cmbArea.SelectedIndex <= 0 ? "" : cmbArea.SelectedItem.ToString();

                DataTable table = _pharmacies.Search(txtSearch.Text.Trim(), status, area);
                dgvPharmacies.DataSource = table;
                LabelColumns();

                if (keepId > 0) SelectPharmacy(keepId);
                _firstLoadDone = true;

                SetStatus(table.Rows.Count + " pharmac" + (table.Rows.Count == 1 ? "y" : "ies") +
                          " shown" + DescribeFilters(status, area), false);
                UpdateButtons();
                return true;
            }
            catch (Exception ex)
            {
                SetStatus("The pharmacy list could not be loaded.", true);
                MessageBox.Show("The pharmacy list could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private string DescribeFilters(string status, string area)
        {
            int active = 0;
            if (!string.IsNullOrEmpty(txtSearch.Text.Trim())) active++;
            if (!string.IsNullOrEmpty(status)) active++;
            if (!string.IsNullOrEmpty(area)) active++;
            return active == 0 ? "  (no filters applied)" : "  (" + active + " filter(s) applied)";
        }

        /// <summary>
        /// Twelve facts about a shop have to share one grid, so each column gets
        /// a DPI-scaled floor through UiTheme.SizeColumn: short numbers stay
        /// narrow and the licence number, which the Super Admin checks before
        /// approving, is never cut off. The owner's email is hidden because the
        /// owner's name and the licence already identify the shop, and the
        /// search box still matches on the owner.
        ///
        /// The Warning column ("Warned 13 Sep 26" / "Read 14 Sep 26") needed
        /// room, and the grid already filled its width at 125% scaling, so two
        /// low-value columns are hidden: the medicine count (Delete is decided
        /// by the order count) and the registration date. Hidden columns keep
        /// their values, so nothing that reads them from the row changes. The
        /// full warning text is the Warning cell's tooltip.
        /// </summary>
        private void LabelColumns()
        {
            if (dgvPharmacies.Columns.Count == 0) return;
            dgvPharmacies.Columns["PharmacyId"].HeaderText = "ID";
            dgvPharmacies.Columns["PharmacyName"].HeaderText = "Pharmacy";
            dgvPharmacies.Columns["OwnerName"].HeaderText = "Owner";
            dgvPharmacies.Columns["OwnerEmail"].Visible = false;
            dgvPharmacies.Columns["LicenseNo"].HeaderText = "DGDA licence";
            dgvPharmacies.Columns["Area"].HeaderText = "Area";
            dgvPharmacies.Columns["ContactPhone"].HeaderText = "Contact";
            dgvPharmacies.Columns["CommissionRate"].HeaderText = "Comm %";
            dgvPharmacies.Columns["Status"].HeaderText = "Status";
            dgvPharmacies.Columns["Medicines"].Visible = false;
            dgvPharmacies.Columns["Orders"].HeaderText = "Orders";
            dgvPharmacies.Columns["AverageRating"].HeaderText = "Rating";
            // The registration date gave up its column to Warning. At 125%
            // scaling twelve visible columns left every layout cutting one of
            // them by a few pixels; pending shops already sort to the top, and
            // approving one depends on the licence, not on the date.
            dgvPharmacies.Columns["RegisteredAt"].Visible = false;
            dgvPharmacies.Columns["WarningState"].HeaderText = "Warning";
            dgvPharmacies.Columns["WarningMessage"].Visible = false;

            // Floors add up to 908 (about 1135 px at 125%), well below the grid's
            // 1154 designed width (about 1318 px at 125%), so there is no
            // horizontal scrollbar and every value has room. Weights equal the
            // floors on purpose: when a weight's share falls below its floor,
            // DataGridView pins that column at the floor but still hands the
            // others their full share, and the row overflows.
            UiTheme.SizeColumn(dgvPharmacies, "PharmacyId", 32, 32);
            UiTheme.SizeColumn(dgvPharmacies, "PharmacyName", 128, 128);
            UiTheme.SizeColumn(dgvPharmacies, "OwnerName", 95, 95);
            UiTheme.SizeColumn(dgvPharmacies, "LicenseNo", 110, 110);
            UiTheme.SizeColumn(dgvPharmacies, "Area", 84, 84);
            UiTheme.SizeColumn(dgvPharmacies, "ContactPhone", 100, 100);
            UiTheme.SizeColumn(dgvPharmacies, "CommissionRate", 56, 56);
            UiTheme.SizeColumn(dgvPharmacies, "Status", 76, 76);
            UiTheme.SizeColumn(dgvPharmacies, "Orders", 54, 54);
            UiTheme.SizeColumn(dgvPharmacies, "AverageRating", 52, 52);
            UiTheme.SizeColumn(dgvPharmacies, "WarningState", 121, 121);
        }

        /// <summary>
        /// Status colour coding, so the queue reads at a glance, plus the full
        /// warning text as the Warning cell's tooltip (the cell shows only the
        /// date and whether the owner has read it).
        /// </summary>
        private void dgvPharmacies_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || !dgvPharmacies.Columns.Contains("Status")) return;

            object value = dgvPharmacies.Rows[e.RowIndex].Cells["Status"].Value;
            if (value == null) return;

            switch (value.ToString())
            {
                case "Pending":
                    e.CellStyle.BackColor = UiTheme.PendingBack;
                    break;
                case "Suspended":
                    e.CellStyle.BackColor = UiTheme.LowStockBack;
                    break;
                case "Rejected":
                    e.CellStyle.BackColor = UiTheme.InactiveBack;
                    break;
            }
        }

        /// <summary>
        /// The Warning cell only shows the date and whether the owner has read
        /// it; hovering it shows the full text the Super Admin sent.
        /// </summary>
        private void dgvPharmacies_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (!dgvPharmacies.Columns.Contains("WarningMessage") ||
                dgvPharmacies.Columns[e.ColumnIndex].Name != "WarningState") return;

            object warning = dgvPharmacies.Rows[e.RowIndex].Cells["WarningMessage"].Value;
            if (warning != null && warning != DBNull.Value) e.ToolTipText = warning.ToString();
        }

        private void SelectPharmacy(int pharmacyId)
        {
            foreach (DataGridViewRow row in dgvPharmacies.Rows)
            {
                if (Convert.ToInt32(row.Cells["PharmacyId"].Value) == pharmacyId)
                {
                    row.Selected = true;
                    dgvPharmacies.CurrentCell = row.Cells["PharmacyName"];
                    break;
                }
            }
        }

        // ---------------------------------------------------------------------
        //  SELECTION AND BUTTON STATE
        //  With no row selected every action button stays disabled, which is the
        //  rule the navigation diagram describes. With a row selected, only the
        //  actions its status allows are enabled:
        //      Pending   -> Approve (Reject is on the dashboard queue)
        //      Rejected  -> Approve
        //      Approved  -> Suspend
        //      Suspended -> Reinstate
        //  Delete is offered for any shop that has never taken an order, and
        //  the commission rate for any shop that is not Rejected.
        // ---------------------------------------------------------------------

        private void dgvPharmacies_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            DataGridViewRow row = dgvPharmacies.CurrentRow;
            bool hasRow = row != null && dgvPharmacies.Columns.Contains("Status") && row.Cells["Status"].Value != null;

            string status = hasRow ? row.Cells["Status"].Value.ToString() : "";
            int orders = hasRow ? Convert.ToInt32(row.Cells["Orders"].Value) : 0;

            btnApprove.Enabled = hasRow && (status == "Pending" || status == "Rejected");
            btnSuspend.Enabled = hasRow && status == "Approved";
            btnReinstate.Enabled = hasRow && status == "Suspended";
            btnDelete.Enabled = hasRow && orders == 0;

            _commissionAllowed = hasRow && status != "Rejected";
            txtCommission.Enabled = _commissionAllowed;
            txtCommission.Text = hasRow ? row.Cells["CommissionRate"].Value.ToString() : "";
            btnSetCommission.Enabled = _commissionAllowed && Validator.IsCommissionRate(txtCommission.Text, out _);
            UiTheme.ClearError(lblCommissionError, txtCommission);
        }

        private int SelectedId()
        {
            if (dgvPharmacies.CurrentRow == null || !dgvPharmacies.Columns.Contains("PharmacyId")) return 0;
            return Convert.ToInt32(dgvPharmacies.CurrentRow.Cells["PharmacyId"].Value);
        }

        private string SelectedCell(string column)
        {
            if (dgvPharmacies.CurrentRow == null || !dgvPharmacies.Columns.Contains(column)) return "";
            object value = dgvPharmacies.CurrentRow.Cells[column].Value;
            return value == null ? "" : value.ToString();
        }

        // ---------------------------------------------------------------------
        //  ACTIONS
        // ---------------------------------------------------------------------

        /// <summary>
        /// Runs one status change, reloads the grid, and then says what really
        /// happened: the service returns 0 when the shop was no longer in a
        /// state that allows the action (for example someone else already
        /// changed it), and the user is told so instead of seeing a success.
        /// </summary>
        private void RunStatusChange(Func<int> action, string failedVerb, string doneMessage, string nothingMessage)
        {
            int changed;
            try
            {
                changed = action();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not " + failedVerb + ".\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!LoadGrid()) return;

            if (changed > 0)
            {
                SetStatus(doneMessage, false);
            }
            else
            {
                SetStatus(nothingMessage, true);
                MessageBox.Show(nothingMessage, "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            int id = SelectedId();
            if (id == 0) return;

            string name = SelectedCell("PharmacyName");
            string licence = SelectedCell("LicenseNo");
            bool wasRejected = SelectedCell("Status") == "Rejected";

            // A previously rejected registration goes through exactly the same
            // licence check as a new one.
            DialogResult answer = MessageBox.Show(
                "Approve " + name + "?\r\n\r\nDGDA licence: " + licence + "\r\n\r\n" +
                (wasRejected ? "This registration was rejected earlier. " : "") +
                "Check the licence number before approving. The pharmacy becomes Approved and the owner's " +
                "account becomes Active, so the owner can log in and customers can see the medicines the shop lists.",
                "Approve pharmacy", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes) return;

            RunStatusChange(() => _pharmacies.Approve(id), "approve " + name,
                name + " approved. The owner can now log in.",
                name + " was not approved: it is no longer Pending or Rejected. The list now shows its current status.");
        }

        private void btnSuspend_Click(object sender, EventArgs e)
        {
            int id = SelectedId();
            if (id == 0) return;

            string name = SelectedCell("PharmacyName");
            string rating = SelectedCell("AverageRating");

            DialogResult answer = MessageBox.Show(
                "Suspend " + name + "?\r\n\r\nAverage customer rating: " + rating + "\r\n\r\n" +
                "The pharmacy and its owner's account both become Suspended: the owner is signed out and " +
                "customers can no longer see or order from the shop.\r\n\r\n" +
                "Nothing is deleted, so the sales history and the invoices customers already hold stay valid, " +
                "and the shop's medicine list is kept for if it is reinstated.",
                "Suspend pharmacy", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes) return;

            RunStatusChange(() => _pharmacies.Suspend(id), "suspend " + name,
                name + " suspended. Customers can no longer see it.",
                name + " was not suspended: it is no longer Approved. The list now shows its current status.");
        }

        private void btnReinstate_Click(object sender, EventArgs e)
        {
            int id = SelectedId();
            if (id == 0) return;

            string name = SelectedCell("PharmacyName");

            DialogResult answer = MessageBox.Show(
                "Put " + name + " back on the platform?\r\n\r\n" +
                "The pharmacy becomes Approved and the owner's account Active again, and the medicines " +
                "the shop lists become visible to customers again.",
                "Reinstate pharmacy", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes) return;

            RunStatusChange(() => _pharmacies.Reinstate(id), "reinstate " + name,
                name + " reinstated.",
                name + " was not reinstated: it is no longer Suspended. The list now shows its current status.");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int id = SelectedId();
            if (id == 0) return;

            string name = SelectedCell("PharmacyName");

            DialogResult answer = MessageBox.Show(
                "Permanently delete " + name + " and its owner account?\r\n\r\n" +
                "This is only possible for a shop that has never taken an order. " +
                "A shop with order history must be suspended instead.",
                "Delete pharmacy", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes) return;

            string message;
            bool deleted;
            try
            {
                deleted = _pharmacies.Delete(id, out message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not delete " + name + ".\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (deleted)
            {
                if (LoadGrid()) SetStatus(name + " and its owner account were deleted.", false);
            }
            else
            {
                MessageBox.Show(message, "Cannot delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGrid();
            }
        }

        // ---------------------------------------------------------------------
        //  COMMISSION RATE  (requirement 9)
        // ---------------------------------------------------------------------

        private void txtCommission_TextChanged(object sender, EventArgs e)
        {
            decimal rate;
            bool valid = Validator.IsCommissionRate(txtCommission.Text, out rate);

            if (string.IsNullOrWhiteSpace(txtCommission.Text))
            {
                UiTheme.ClearError(lblCommissionError, txtCommission);
                btnSetCommission.Enabled = false;
                return;
            }

            if (valid) UiTheme.ClearError(lblCommissionError, txtCommission);
            else UiTheme.ShowError(lblCommissionError, txtCommission, "The rate must be a number between 0 and 30.");

            btnSetCommission.Enabled = valid && _commissionAllowed;
        }

        private void btnSetCommission_Click(object sender, EventArgs e)
        {
            int id = SelectedId();
            if (id == 0) return;

            string name = SelectedCell("PharmacyName");

            decimal rate;
            if (!Validator.IsCommissionRate(txtCommission.Text, out rate))
            {
                UiTheme.ShowError(lblCommissionError, txtCommission, "The rate must be a number between 0 and 30.");
                return;
            }

            bool saved;
            try
            {
                saved = _pharmacies.SetCommissionRate(id, rate);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not change the commission rate.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!LoadGrid()) return;

            if (saved)
                SetStatus(name + " now pays " + rate.ToString("N2") + "% commission. Only orders placed from now on " +
                          "use the new rate; existing orders keep the commission worked out when they were placed.", false);
            else
                SetStatus("Nothing changed - " + name + " could not be found. It may have been deleted.", true);
        }

        // ---------------------------------------------------------------------

        private void Filter_Changed(object sender, EventArgs e) => LoadGrid();

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            _searchDelay.Stop();
            _searchDelay.Start();
        }

        private void SearchDelay_Tick(object sender, EventArgs e)
        {
            _searchDelay.Stop();
            LoadGrid();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            _searchDelay.Stop();
            LoadGrid();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _loading = true;
            txtSearch.Clear();
            cmbStatus.SelectedIndex = 0;
            cmbArea.SelectedIndex = 0;
            _loading = false;
            _searchDelay.Stop();
            LoadGrid();
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
