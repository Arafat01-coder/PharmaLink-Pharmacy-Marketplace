using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 15. The reviews written about this pharmacy's medicines.
    ///
    /// The form is read only on purpose: an owner can read the ratings but can
    /// neither edit nor delete them. If he believes a review is abusive he uses
    /// the Report button, which records a short reason on the review
    /// (Reviews.IsReported) and puts it in the Super Admin's "Reported by
    /// pharmacy" queue, rather than removing it himself. A review that is
    /// already reported cannot be reported again until the Super Admin decides.
    /// </summary>
    public partial class AdminReviewsForm : Form
    {
        private readonly ReviewService _reviews = new ReviewService();
        private bool _loading = true;

        public AdminReviewsForm()
        {
            InitializeComponent();
        }

        private void AdminReviewsForm_Load(object sender, EventArgs e)
        {
            UiTheme.MakeResizable(this, Size);
            ApplyTheme();

            cmbRating.Items.AddRange(new object[]
            {
                "All ratings",
                "5 stars only",
                "4 stars and above",
                "3 stars and below",
                "1 and 2 stars only"
            });
            cmbRating.SelectedIndex = 0;

            _loading = false;
            LoadGrid();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Customer Reviews");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            lblAverage.Font = UiTheme.FontSubheading;
            lblAverage.ForeColor = UiTheme.TextDark;

            lblReadOnlyNote.Font = UiTheme.FontSmall;
            lblReadOnlyNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;
            txtComment.Font = UiTheme.FontBody;
            txtComment.BackColor = UiTheme.CardBack;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSecondary(btnRefresh);
            UiTheme.StyleAccent(btnReport);
            UiTheme.StyleGrid(dgvReviews);
            UiTheme.EnableEmptyMessage(dgvReviews, "No reviews match this filter yet.");
            dgvReviews.DataBindingComplete += dgvReviews_DataBindingComplete;
        }

        private void RatingRange(out int min, out int max)
        {
            switch (cmbRating.SelectedIndex)
            {
                case 1: min = 5; max = 5; break;
                case 2: min = 4; max = 5; break;
                case 3: min = 1; max = 3; break;
                case 4: min = 1; max = 2; break;
                default: min = 1; max = 5; break;
            }
        }

        private void LoadGrid()
        {
            if (_loading) return;

            try
            {
                int min, max;
                RatingRange(out min, out max);

                DataTable table = _reviews.GetForPharmacy(UserSession.PharmacyId, min, max);
                dgvReviews.DataSource = table;

                if (dgvReviews.Columns.Count > 0)
                {
                    dgvReviews.Columns["ReviewId"].HeaderText = "ID";
                    dgvReviews.Columns["ReviewerName"].HeaderText = "Reviewer";
                    dgvReviews.Columns["MedicineName"].HeaderText = "Medicine";
                    dgvReviews.Columns["Strength"].HeaderText = "Strength";
                    dgvReviews.Columns["Rating"].HeaderText = "Stars";
                    dgvReviews.Columns["Comment"].HeaderText = "Comment";
                    dgvReviews.Columns["ReviewDate"].HeaderText = "Written on";
                    dgvReviews.Columns["ReviewDate"].DefaultCellStyle.Format = "dd MMM yyyy";
                    dgvReviews.Columns["OrderId"].HeaderText = "Order";
                    dgvReviews.Columns["IsReported"].HeaderText = "Reported";

                    UiTheme.SizeColumn(dgvReviews, "ReviewId", 28, 40);
                    UiTheme.SizeColumn(dgvReviews, "Strength", 45, 65);
                    UiTheme.SizeColumn(dgvReviews, "Rating", 32, 50);
                    UiTheme.SizeColumn(dgvReviews, "Comment", 170, 160);
                    UiTheme.SizeColumn(dgvReviews, "ReviewDate", 60, 90);
                    UiTheme.SizeColumn(dgvReviews, "OrderId", 40, 55);
                    UiTheme.SizeColumn(dgvReviews, "IsReported", 45, 70);
                }

                decimal average = _reviews.GetAverageForPharmacy(UserSession.PharmacyId);
                int total = _reviews.CountForPharmacy(UserSession.PharmacyId);

                lblAverage.Text = total == 0
                    ? "No reviews yet"
                    : "Average rating  " + average.ToString("N2") + " / 5   from " + total + " review(s)";

                lblAverage.ForeColor = average > 0 && average < 2.5m ? UiTheme.Danger : UiTheme.TextDark;

                lblStatus.Text = average > 0 && average < 2.5m && total >= 2
                    ? "Warning: your average is below 2.5 with " + total + " reviews, which puts your shop on the Super Admin's low rated report."
                    : table.Rows.Count + " review(s) shown. Every review is tied to a delivered order.";

                UpdateSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The reviews could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvReviews_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvReviews.Columns.Contains("Rating")) return;

            foreach (DataGridViewRow row in dgvReviews.Rows)
            {
                object rating = row.Cells["Rating"].Value;
                if (rating == null || rating == DBNull.Value) continue;

                int stars = Convert.ToInt32(rating);
                if (stars <= 2) row.DefaultCellStyle.BackColor = UiTheme.LowStockBack;
                else if (stars >= 4) row.DefaultCellStyle.BackColor = UiTheme.DeliveredBack;
                else row.DefaultCellStyle.BackColor = Color.Empty;
            }
        }

        private void dgvReviews_SelectionChanged(object sender, EventArgs e) => UpdateSelection();

        private static bool IsReported(DataGridViewRow row)
        {
            object value = row.Cells["IsReported"].Value;
            return value != null && value != DBNull.Value && Convert.ToBoolean(value);
        }

        private void UpdateSelection()
        {
            DataGridViewRow row = dgvReviews.CurrentRow;

            if (row == null || !dgvReviews.Columns.Contains("ReviewId") || row.Cells["ReviewId"].Value == null)
            {
                txtComment.Clear();
                btnReport.Enabled = false;
                btnReport.Text = "Report this review to the Super Admin";
                return;
            }

            object comment = row.Cells["Comment"].Value;
            txtComment.Text = comment == null || comment == DBNull.Value
                ? "(this customer left a rating but no written comment)"
                : comment.ToString();

            bool reported = IsReported(row);
            btnReport.Enabled = !reported;
            btnReport.Text = reported
                ? "Already reported - waiting for the Super Admin"
                : "Report this review to the Super Admin";
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dgvReviews.CurrentRow;
            if (row == null || IsReported(row)) return;

            int reviewId = Convert.ToInt32(row.Cells["ReviewId"].Value);
            string medicine = Convert.ToString(row.Cells["MedicineName"].Value);

            string reason = AskForReason(medicine);
            if (reason == null) return;

            try
            {
                bool reported = _reviews.Report(reviewId, UserSession.PharmacyId, reason);
                LoadGrid();

                if (reported)
                {
                    lblStatus.Text = "Review " + reviewId + " reported. It stays visible to customers until the Super Admin decides.";
                }
                else
                {
                    lblStatus.Text = "Review " + reviewId + " was not reported.";
                    MessageBox.Show("This review could not be reported. It may already have been reported or " +
                                    "hidden by the Super Admin. The list has been refreshed.",
                        "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The review could not be reported.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// A small modal prompt for the report reason. Returns null when the
        /// owner cancels. The reason is required and capped at the 200
        /// characters Reviews.ReportReason can hold.
        /// </summary>
        private string AskForReason(string medicine)
        {
            using (Form dialog = new Form())
            {
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ClientSize = new Size(460, 214);
                dialog.BackColor = UiTheme.PageBack;
                dialog.ForeColor = UiTheme.TextDark;
                dialog.Text = "PharmaLink  -  Report review";

                Label prompt = new Label
                {
                    AutoSize = false,
                    Location = new Point(16, 14),
                    Size = new Size(428, 40),
                    Font = UiTheme.FontBody,
                    Text = "Why should the Super Admin look at this review of " + medicine + "? " +
                           "For example: abusive language, not about this medicine, or a false claim."
                };

                TextBox txtReason = new TextBox
                {
                    Location = new Point(16, 60),
                    Size = new Size(428, 70),
                    Multiline = true,
                    MaxLength = 200,
                    Font = UiTheme.FontBody
                };

                Label error = new Label
                {
                    AutoSize = false,
                    Location = new Point(16, 134),
                    Size = new Size(428, 18),
                    Font = UiTheme.FontSmall,
                    ForeColor = UiTheme.Danger
                };

                Button ok = new Button { Location = new Point(208, 162), Size = new Size(130, 36), Text = "Report" };
                Button cancel = new Button { Location = new Point(346, 162), Size = new Size(98, 36), Text = "Cancel", DialogResult = DialogResult.Cancel };
                UiTheme.StyleAccent(ok);
                UiTheme.StyleSecondary(cancel);

                ok.Click += (s, e) =>
                {
                    if (txtReason.Text.Trim().Length < 5)
                    {
                        error.Text = "Give a short reason (at least 5 characters).";
                        txtReason.Focus();
                        return;
                    }
                    dialog.DialogResult = DialogResult.OK;
                };

                dialog.Controls.AddRange(new Control[] { prompt, txtReason, error, ok, cancel });
                dialog.CancelButton = cancel;

                return dialog.ShowDialog(this) == DialogResult.OK ? txtReason.Text.Trim() : null;
            }
        }

        private void Filter_Changed(object sender, EventArgs e) => LoadGrid();
        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
