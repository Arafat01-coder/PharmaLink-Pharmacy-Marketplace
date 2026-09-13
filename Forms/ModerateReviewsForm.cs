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
    /// </summary>
    public partial class ModerateReviewsForm : Form
    {
        private const int ReportedFilterIndex = 3;

        private readonly ReviewService _reviews = new ReviewService();
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

        private void LoadGrid()
        {
            if (_loading) return;

            try
            {
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

                    UiTheme.SizeColumn(dgvReviews, "ReviewId", 30, 40);
                    UiTheme.SizeColumn(dgvReviews, "Rating", 32, 50);
                    UiTheme.SizeColumn(dgvReviews, "Comment", 150, 140);
                    UiTheme.SizeColumn(dgvReviews, "ReviewDate", 60, 90);
                    UiTheme.SizeColumn(dgvReviews, "OrderId", 40, 55);
                    UiTheme.SizeColumn(dgvReviews, "IsHidden", 40, 60);
                    UiTheme.SizeColumn(dgvReviews, "IsReported", 45, 70);
                    UiTheme.SizeColumn(dgvReviews, "ReportReason", 110, 120);
                }

                lblStatus.Text = table.Rows.Count + " review(s) in the queue.";
                UpdateSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The moderation queue could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void Filter_Changed(object sender, EventArgs e) => LoadGrid();
        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
