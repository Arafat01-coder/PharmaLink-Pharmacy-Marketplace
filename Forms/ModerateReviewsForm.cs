using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 8. The moderation queue, filtered by default to one and two
    /// star reviews, which is where abuse usually sits, with a second view of
    /// the reviews pharmacy owners have reported and why.
    ///
    /// Hide Review sets IsHidden to 1 rather than deleting the row, so the
    /// review disappears from the customer screens and from every average rating
    /// calculation while remaining restorable. Hiding a reported review also
    /// closes the report; Dismiss report closes it and leaves the review visible.
    ///
    /// A review is often a symptom of how the shop behind it is run, so the
    /// Super Admin can also warn that pharmacy (the owner sees the notice on
    /// their dashboard until they acknowledge it) or open the pharmacy's own
    /// record in Manage Pharmacies to suspend it or check its history.
    /// </summary>
    public partial class ModerateReviewsForm : Form
    {
        private const int ReportedFilterIndex = 3;

        /// <summary>How much of the review's comment is quoted in a pre-filled warning.</summary>
        private const int WarningExcerptLength = 120;

        private readonly ReviewService _reviews = new ReviewService();
        private readonly PharmacyService _pharmacies = new PharmacyService();
        private bool _loading = true;

        public ModerateReviewsForm()
        {
            InitializeComponent();
        }

        private void ModerateReviewsForm_Load(object sender, EventArgs e)
        {
            UiTheme.MakeResizable(this, Size);
            ApplyTheme();

            cmbRating.Items.AddRange(new object[]
            {
                "1 and 2 stars only  (default)",
                "3 stars and below",
                "All ratings",
                "Reported by pharmacy"
            });
            cmbRating.SelectedIndex = 0;

            _loading = false;
            LoadGrid();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Moderate Reviews");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSecondary(btnRefresh);
            UiTheme.StyleDanger(btnHide);
            UiTheme.StyleSuccess(btnUnhide);
            UiTheme.StyleAccent(btnDismissReport);
            UiTheme.StylePrimary(btnWarnPharmacy);
            UiTheme.StyleSecondary(btnOpenPharmacy);
            UiTheme.StyleGrid(dgvReviews);
            UiTheme.EnableEmptyMessage(dgvReviews, "Nothing in this queue right now.");
            dgvReviews.DataBindingComplete += dgvReviews_DataBindingComplete;

            txtComment.BackColor = UiTheme.CardBack;
            txtComment.Font = UiTheme.FontBody;
            lblNote.Font = UiTheme.FontSmall;
            lblNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;
        }

        private int MaxRating()
        {
            switch (cmbRating.SelectedIndex)
            {
                case 0: return 2;
                case 1: return 3;
                default: return 5;
            }
        }

        /// <summary>
        /// Rebinds the queue and keeps the same review selected when it is still
        /// listed, so warning a shop or coming back from its record does not
        /// throw the Super Admin back to the top of a long queue.
        /// </summary>
        private void LoadGrid()
        {
            if (_loading) return;

            try
            {
                int keepReviewId = SelectedInt("ReviewId");

                bool reportedOnly = cmbRating.SelectedIndex == ReportedFilterIndex;
                DataTable table = _reviews.GetModerationQueue(MaxRating(), chkIncludeHidden.Checked, reportedOnly);
                dgvReviews.DataSource = table;

                if (dgvReviews.Columns.Count > 0)
                {
                    dgvReviews.Columns["ReviewId"].HeaderText = "ID";
                    dgvReviews.Columns["Reviewer"].HeaderText = "Reviewer";
                    dgvReviews.Columns["MedicineName"].HeaderText = "Medicine";
                    dgvReviews.Columns["PharmacyName"].HeaderText = "Pharmacy";
                    dgvReviews.Columns["Rating"].HeaderText = "Stars";
                    dgvReviews.Columns["Comment"].HeaderText = "Comment";
                    dgvReviews.Columns["ReviewDate"].HeaderText = "Written on";
                    dgvReviews.Columns["ReviewDate"].DefaultCellStyle.Format = "dd MMM yyyy";
                    dgvReviews.Columns["OrderId"].HeaderText = "Order";
                    dgvReviews.Columns["IsHidden"].HeaderText = "Hidden";
                    dgvReviews.Columns["IsReported"].HeaderText = "Reported";
                    dgvReviews.Columns["ReportReason"].HeaderText = "Pharmacy's reason";
                    dgvReviews.Columns["ReportedAt"].Visible = false;
                    dgvReviews.Columns["PharmacyId"].Visible = false;

                    UiTheme.SizeColumn(dgvReviews, "ReviewId", 30, 40);
                    UiTheme.SizeColumn(dgvReviews, "Rating", 32, 50);
                    UiTheme.SizeColumn(dgvReviews, "Comment", 150, 140);
                    UiTheme.SizeColumn(dgvReviews, "ReviewDate", 60, 90);
                    UiTheme.SizeColumn(dgvReviews, "OrderId", 40, 55);
                    UiTheme.SizeColumn(dgvReviews, "IsHidden", 40, 60);
                    UiTheme.SizeColumn(dgvReviews, "IsReported", 45, 70);
                    UiTheme.SizeColumn(dgvReviews, "ReportReason", 110, 120);
                }

                if (keepReviewId > 0) SelectReview(keepReviewId);
                UiTheme.EnsureCurrentCell(dgvReviews, "ReviewId");

                lblStatus.Text = table.Rows.Count + " review(s) in the queue.";
                UpdateSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The moderation queue could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectReview(int reviewId)
        {
            foreach (DataGridViewRow row in dgvReviews.Rows)
            {
                object value = row.Cells["ReviewId"].Value;
                if (value != null && value != DBNull.Value && Convert.ToInt32(value) == reviewId)
                {
                    dgvReviews.CurrentCell = row.Cells["ReviewId"];
                    return;
                }
            }
        }

        /// <summary>An integer cell of the selected row, or 0 when there is no row or no such column.</summary>
        private int SelectedInt(string column)
        {
            DataGridViewRow row = dgvReviews.CurrentRow;
            if (row == null || !dgvReviews.Columns.Contains(column)) return 0;
            object value = row.Cells[column].Value;
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static bool Flag(DataGridViewRow row, string column)
        {
            object value = row.Cells[column].Value;
            return value != null && value != DBNull.Value && Convert.ToBoolean(value);
        }

        private void dgvReviews_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvReviews.Columns.Contains("IsHidden") || !dgvReviews.Columns.Contains("IsReported")) return;

            foreach (DataGridViewRow row in dgvReviews.Rows)
            {
                object rating = row.Cells["Rating"].Value;

                if (Flag(row, "IsHidden"))
                {
                    row.DefaultCellStyle.BackColor = UiTheme.InactiveBack;
                    row.DefaultCellStyle.ForeColor = UiTheme.TextMuted;
                }
                else if (Flag(row, "IsReported"))
                {
                    row.DefaultCellStyle.BackColor = UiTheme.WarningBack;
                    row.DefaultCellStyle.ForeColor = UiTheme.TextDark;
                }
                else if (rating != null && rating != DBNull.Value && Convert.ToInt32(rating) <= 2)
                {
                    row.DefaultCellStyle.BackColor = UiTheme.LowStockBack;
                    row.DefaultCellStyle.ForeColor = UiTheme.TextDark;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.Empty;
                    row.DefaultCellStyle.ForeColor = UiTheme.TextDark;
                }
            }
        }

        private void dgvReviews_SelectionChanged(object sender, EventArgs e) => UpdateSelection();

        private void UpdateSelection()
        {
            DataGridViewRow row = dgvReviews.CurrentRow;

            if (row == null || !dgvReviews.Columns.Contains("ReviewId") || row.Cells["ReviewId"].Value == null)
            {
                txtComment.Clear();
                btnHide.Enabled = false;
                btnUnhide.Enabled = false;
                btnDismissReport.Enabled = false;
                btnWarnPharmacy.Enabled = false;
                btnOpenPharmacy.Enabled = false;
                return;
            }

            object comment = row.Cells["Comment"].Value;
            string text = comment == null || comment == DBNull.Value ? "(no written comment)" : comment.ToString();

            bool hidden = Flag(row, "IsHidden");
            bool reported = Flag(row, "IsReported");

            if (reported)
            {
                object reportedAt = row.Cells["ReportedAt"].Value;
                text += Environment.NewLine + Environment.NewLine +
                        "Reported by " + Convert.ToString(row.Cells["PharmacyName"].Value) +
                        (reportedAt == null || reportedAt == DBNull.Value
                            ? "" : " on " + Convert.ToDateTime(reportedAt).ToString("dd MMM yyyy")) +
                        ":  " + Convert.ToString(row.Cells["ReportReason"].Value);
            }
            txtComment.Text = text;

            btnHide.Enabled = !hidden;
            btnUnhide.Enabled = hidden;
            btnDismissReport.Enabled = reported;

            bool hasPharmacy = SelectedInt("PharmacyId") > 0;
            btnWarnPharmacy.Enabled = hasPharmacy;
            btnOpenPharmacy.Enabled = hasPharmacy;
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dgvReviews.CurrentRow;
            if (row == null) return;

            int reviewId = Convert.ToInt32(row.Cells["ReviewId"].Value);
            string pharmacy = Convert.ToString(row.Cells["PharmacyName"].Value);
            bool reported = Flag(row, "IsReported");

            DialogResult answer = MessageBox.Show(
                "Hide this review from the customer screens?\r\n\r\n" +
                "The review is kept, so " + pharmacy + "'s rating history stays complete and it can be restored later." +
                (reported ? "\r\n\r\nThe pharmacy's report on it will be closed." : ""),
                "Hide review", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes) return;

            try
            {
                bool changed = _reviews.SetHidden(reviewId, true);
                LoadGrid();
                lblStatus.Text = changed
                    ? "Review " + reviewId + " hidden. It no longer counts towards " + pharmacy + "'s average rating."
                    : "Review " + reviewId + " was not changed - it no longer exists.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The review could not be hidden.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUnhide_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dgvReviews.CurrentRow;
            if (row == null) return;

            int reviewId = Convert.ToInt32(row.Cells["ReviewId"].Value);

            try
            {
                bool changed = _reviews.SetHidden(reviewId, false);
                LoadGrid();
                lblStatus.Text = changed
                    ? "Review " + reviewId + " restored and is visible to customers again."
                    : "Review " + reviewId + " was not changed - it no longer exists.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The review could not be restored.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDismissReport_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dgvReviews.CurrentRow;
            if (row == null) return;

            int reviewId = Convert.ToInt32(row.Cells["ReviewId"].Value);

            try
            {
                bool changed = _reviews.DismissReport(reviewId);
                LoadGrid();
                lblStatus.Text = changed
                    ? "Report on review " + reviewId + " dismissed. The review stays visible."
                    : "Review " + reviewId + " had no open report any more - nothing changed.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The report could not be dismissed.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------------------------
        //  THE PHARMACY BEHIND THE REVIEW
        // ---------------------------------------------------------------------

        /// <summary>
        /// Sends the selected review's pharmacy a warning. The text starts as a
        /// draft quoting the rating and a short excerpt of the comment, so the
        /// owner knows what prompted it, and the Super Admin can rewrite it.
        /// A new warning replaces any earlier one and shows on the owner's
        /// dashboard until the owner acknowledges it.
        /// </summary>
        private void btnWarnPharmacy_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dgvReviews.CurrentRow;
            int pharmacyId = SelectedInt("PharmacyId");
            if (row == null || pharmacyId == 0) return;

            string pharmacy = Convert.ToString(row.Cells["PharmacyName"].Value);
            string message = AskForWarning(pharmacy, DraftWarning(row));
            if (message == null) return;

            DialogResult answer = MessageBox.Show(
                "Send this warning to " + pharmacy + "?\r\n\r\n\"" + message + "\"\r\n\r\n" +
                "It replaces any earlier warning and stays on the owner's dashboard until they mark it as read. " +
                "Nothing else about the pharmacy changes.",
                "Warn pharmacy", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes) return;

            int changed;
            try
            {
                changed = _pharmacies.WarnPharmacy(pharmacyId, message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The warning could not be sent.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadGrid();

            if (changed > 0)
            {
                lblStatus.Text = "Warning sent to " + pharmacy + ". The owner will see it on their dashboard.";
            }
            else
            {
                lblStatus.Text = "No warning was sent to " + pharmacy + ".";
                MessageBox.Show("The warning was not saved. The pharmacy may have been deleted since the queue was loaded.",
                    "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>The editable starting text for a warning about the selected review.</summary>
        private static string DraftWarning(DataGridViewRow row)
        {
            int stars = row.Cells["Rating"].Value == null || row.Cells["Rating"].Value == DBNull.Value
                ? 0 : Convert.ToInt32(row.Cells["Rating"].Value);
            string medicine = Convert.ToString(row.Cells["MedicineName"].Value);

            object value = row.Cells["Comment"].Value;
            string comment = value == null || value == DBNull.Value ? "" : value.ToString().Trim();
            comment = comment.Replace("\r", " ").Replace("\n", " ");
            if (comment.Length > WarningExcerptLength)
                comment = comment.Substring(0, WarningExcerptLength - 3).TrimEnd() + "...";

            string draft = "A customer gave " + medicine + " from your pharmacy " + stars + " star" +
                           (stars == 1 ? "" : "s") +
                           (comment.Length > 0 ? ": \"" + comment + "\"" : "") +
                           ". Please look into this order and make sure it does not happen again.";

            return draft.Length <= PharmacyService.WarningMaxLength
                ? draft : draft.Substring(0, PharmacyService.WarningMaxLength);
        }

        /// <summary>
        /// A small modal prompt for the warning text, built the same way as the
        /// report-reason prompt on the owner's Customer Reviews screen. Returns
        /// null when the Super Admin cancels. The text must be between 10 and 500
        /// characters, the size Pharmacies.WarningMessage holds.
        /// </summary>
        private string AskForWarning(string pharmacy, string draft)
        {
            using (Form dialog = new Form())
            {
                // Same scaling contract as the designer forms (laid out at 7x15 per
                // the default 9pt font): without it the pixel sizes below stayed
                // fixed while the text grew at 125% display scaling, and the
                // second line of the prompt was cut off.
                dialog.SuspendLayout();
                dialog.AutoScaleDimensions = new SizeF(7F, 15F);
                dialog.AutoScaleMode = AutoScaleMode.Font;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ClientSize = new Size(520, 270);
                dialog.BackColor = UiTheme.PageBack;
                dialog.ForeColor = UiTheme.TextDark;
                dialog.Text = "PharmaLink  -  Warn pharmacy";

                Label prompt = new Label
                {
                    AutoSize = false,
                    Location = new Point(16, 14),
                    Size = new Size(488, 40),
                    Font = UiTheme.FontBody,
                    UseMnemonic = false,
                    Text = "Warning for " + pharmacy + ". The owner sees this text on their dashboard until they " +
                           "mark it as read. Edit it as needed."
                };

                TextBox txtMessage = new TextBox
                {
                    Name = "txtWarningMessage",
                    Location = new Point(16, 60),
                    Size = new Size(488, 120),
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    MaxLength = PharmacyService.WarningMaxLength,
                    Font = UiTheme.FontBody,
                    Text = draft
                };

                Label counter = new Label
                {
                    AutoSize = false,
                    Location = new Point(16, 186),
                    Size = new Size(488, 18),
                    Font = UiTheme.FontSmall,
                    ForeColor = UiTheme.TextMuted
                };

                Button ok = new Button { Name = "btnSendWarning", Location = new Point(250, 218), Size = new Size(150, 36), Text = "Send warning" };
                Button cancel = new Button { Name = "btnCancelWarning", Location = new Point(410, 218), Size = new Size(94, 36), Text = "Cancel", DialogResult = DialogResult.Cancel };
                UiTheme.StylePrimary(ok);
                UiTheme.StyleSecondary(cancel);

                // The counter doubles as the error line: grey while the text is a
                // valid length, red with the rule once it is not.
                void Recount()
                {
                    int length = txtMessage.Text.Trim().Length;
                    bool valid = length >= PharmacyService.WarningMinLength && length <= PharmacyService.WarningMaxLength;
                    counter.ForeColor = valid ? UiTheme.TextMuted : UiTheme.Danger;
                    counter.Text = valid
                        ? length + " of " + PharmacyService.WarningMaxLength + " characters"
                        : "Write between " + PharmacyService.WarningMinLength + " and " +
                          PharmacyService.WarningMaxLength + " characters (now " + length + ").";
                    ok.Enabled = valid;
                }

                txtMessage.TextChanged += (s, e) => Recount();
                Recount();

                ok.Click += (s, e) =>
                {
                    Recount();
                    if (!ok.Enabled)
                    {
                        txtMessage.Focus();
                        return;
                    }
                    dialog.DialogResult = DialogResult.OK;
                };

                dialog.Controls.AddRange(new Control[] { prompt, txtMessage, counter, ok, cancel });
                dialog.CancelButton = cancel;
                dialog.ResumeLayout(false);

                return dialog.ShowDialog(this) == DialogResult.OK ? txtMessage.Text.Trim() : null;
            }
        }

        /// <summary>
        /// Opens the pharmacy's record in Manage Pharmacies with that shop
        /// selected (the same entry point the Low Rated report uses). Its filters
        /// start at "All", so the shop is listed whatever its status. The queue
        /// is reloaded afterwards because suspending the shop there changes
        /// nothing here, but hiding it would, and the Super Admin should never be
        /// looking at stale rows.
        /// </summary>
        private void btnOpenPharmacy_Click(object sender, EventArgs e)
        {
            int pharmacyId = SelectedInt("PharmacyId");
            if (pharmacyId == 0) return;

            using (SuperAdminManageShopsForm form = new SuperAdminManageShopsForm(pharmacyId))
            {
                form.ShowDialog(this);
            }
            LoadGrid();
        }

        private void Filter_Changed(object sender, EventArgs e) => LoadGrid();
        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
