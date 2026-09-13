using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 6.
    ///
    /// Ratings sit on medicines, not on pharmacies, so the average has to be
    /// built by joining Pharmacies, Medicines and Reviews and grouping back up
    /// to the pharmacy. HAVING is the right clause because the condition is on
    /// the aggregate itself, and the second condition, a minimum number of
    /// reviews, is a deliberate fairness rule: one angry customer should not be
    /// enough to put a shop on the suspension list.
    ///
    /// The screen itself explains the rule in plain words, using whatever
    /// threshold and minimum the Super Admin has chosen.
    /// </summary>
    public partial class SuperAdminLowRatedShopsForm : Form
    {
        private readonly ReportService _reports = new ReportService();
        private DataTable _current;

        public SuperAdminLowRatedShopsForm()
        {
            InitializeComponent();
        }

        private void SuperAdminLowRatedShopsForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            UiTheme.MakeResizable(this, Size);
            Generate();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Low Rated Pharmacies");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StylePrimary(btnGenerate);
            UiTheme.StyleAccent(btnExport);
            UiTheme.StyleSecondary(btnOpenPharmacy);
            UiTheme.StyleGrid(dgvLowRated);
            UiTheme.EnableEmptyMessage(dgvLowRated, "No pharmacy matches these conditions.");
            dgvLowRated.CellFormatting += dgvLowRated_CellFormatting;

            lblSqlNote.Font = UiTheme.FontSmall;
            lblSqlNote.ForeColor = UiTheme.TextMuted;

            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;
        }

        /// <summary>The plain-language rule, rebuilt from the values the report actually used.</summary>
        private void DescribeRule(decimal threshold, int minimumReviews)
        {
            lblSqlNote.Text =
                "How this list is built: for each pharmacy, the star ratings on all of its medicines are averaged, " +
                "leaving out reviews hidden by moderation. A pharmacy is listed when that average is below " +
                threshold.ToString("N1") + " and it has at least " + minimumReviews + " review" +
                (minimumReviews == 1 ? "" : "s") + ", so a single bad review cannot put a shop on this list. " +
                "Change either value above and press Run report to apply a different rule.";
        }

        private void Generate()
        {
            decimal threshold = numThreshold.Value;
            int minimumReviews = (int)numMinReviews.Value;

            try
            {
                _current = _reports.GetLowRatedPharmacies(threshold, minimumReviews);
                dgvLowRated.DataSource = _current;

                if (dgvLowRated.Columns.Count > 0)
                {
                    dgvLowRated.Columns["PharmacyId"].HeaderText = "ID";
                    dgvLowRated.Columns["PharmacyId"].FillWeight = 30;
                    dgvLowRated.Columns["PharmacyName"].HeaderText = "Pharmacy";
                    dgvLowRated.Columns["Area"].HeaderText = "Area";
                    dgvLowRated.Columns["OwnerName"].HeaderText = "Owner";
                    dgvLowRated.Columns["OwnerPhone"].HeaderText = "Owner phone";
                    dgvLowRated.Columns["TotalReviews"].HeaderText = "Reviews";
                    dgvLowRated.Columns["TotalReviews"].FillWeight = 45;
                    dgvLowRated.Columns["AverageRating"].HeaderText = "Average rating";
                    dgvLowRated.Columns["AverageRating"].FillWeight = 55;
                    dgvLowRated.Columns["Status"].HeaderText = "Status";
                    dgvLowRated.Columns["Status"].FillWeight = 55;
                }

                btnOpenPharmacy.Enabled = _current.Rows.Count > 0;
                DescribeRule(threshold, minimumReviews);

                lblStatus.ForeColor = UiTheme.TextMuted;
                lblStatus.Text = _current.Rows.Count == 0
                    ? "No pharmacy is rated below " + threshold.ToString("N1") +
                      " with at least " + minimumReviews + " review(s). Nothing needs your attention."
                    : _current.Rows.Count + " pharmac" + (_current.Rows.Count == 1 ? "y" : "ies") +
                      " rated below " + threshold.ToString("N1") +
                      ". Double click a row to open it in Manage Pharmacies with the record already selected.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The report could not be generated.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvLowRated_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            e.CellStyle.BackColor = UiTheme.LowStockBack;
        }

        private void OpenSelectedPharmacy()
        {
            if (dgvLowRated.CurrentRow == null || !dgvLowRated.Columns.Contains("PharmacyId")) return;
            int pharmacyId = Convert.ToInt32(dgvLowRated.CurrentRow.Cells["PharmacyId"].Value);

            using (SuperAdminManageShopsForm form = new SuperAdminManageShopsForm(pharmacyId))
            {
                form.ShowDialog(this);
            }
            Generate();
        }

        private void dgvLowRated_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            OpenSelectedPharmacy();
        }

        private void btnOpenPharmacy_Click(object sender, EventArgs e) => OpenSelectedPharmacy();
        private void btnGenerate_Click(object sender, EventArgs e) => Generate();

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_current == null || _current.Rows.Count == 0)
            {
                MessageBox.Show("There is nothing to export.", "PharmaLink",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV file (*.csv)|*.csv";
                dialog.FileName = "PharmaLink-LowRated-" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                string message;
                if (ReportService.ExportToCsv(_current, dialog.FileName, out message))
                {
                    lblStatus.ForeColor = UiTheme.TextMuted;
                    lblStatus.Text = message;
                }
                else
                {
                    lblStatus.ForeColor = UiTheme.Danger;
                    lblStatus.Text = "Export failed.";
                    MessageBox.Show(message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
