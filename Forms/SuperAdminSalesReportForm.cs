using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 5. Gross item sales, platform commission, units sold, order
    /// count and average item price (the average unit price across order
    /// lines) for every pharmacy, filtered by date range, area and order status,
    /// with a bold total row at the bottom of the grid and a CSV export so the
    /// figures can be reconciled against bank settlements.
    ///
    /// The total row exists only in the grid. _current holds the report rows
    /// alone, which is what the CSV export writes, and the grid columns cannot
    /// be sorted, so the total row always stays last.
    /// </summary>
    public partial class SuperAdminSalesReportForm : Form
    {
        private readonly ReportService _reports = new ReportService();
        private readonly PharmacyService _pharmacies = new PharmacyService();

        private Label _tileGross;
        private Label _tileCommission;
        private Label _tileUnits;
        private Label _tileOrders;
        private Panel[] _tiles;

        /// <summary>The report rows only, without the total row.</summary>
        private DataTable _current;

        /// <summary>True when the grid's last row is the appended platform total.</summary>
        private bool _showsTotalRow;

        public SuperAdminSalesReportForm()
        {
            InitializeComponent();
        }

        private void SuperAdminSalesReportForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            BuildTiles();
            UiTheme.MakeResizable(this, Size);
            LayoutTiles();
            Resize += (s, args) => LayoutTiles();

            dtpFrom.Value = DateTime.Today.AddDays(-30);
            dtpTo.Value = DateTime.Today;

            cmbArea.Items.Add("All areas");
            try
            {
                foreach (string area in _pharmacies.GetAreas(false)) cmbArea.Items.Add(area);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The area filter could not be loaded, so only \"All areas\" is offered.\r\n\r\n" +
                                DbHelper.Describe(ex), "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            cmbArea.SelectedIndex = 0;

            cmbStatus.Items.AddRange(new object[] { "Confirmed + delivered", "Placed", "Confirmed", "Delivered" });
            cmbStatus.SelectedIndex = 0;

            Generate();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Sales and Commission");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StylePrimary(btnGenerate);
            UiTheme.StyleAccent(btnExport);
            UiTheme.StyleGrid(dgvReport);
            UiTheme.EnableEmptyMessage(dgvReport, "No sales match these filters.");
            dgvReport.CellFormatting += dgvReport_CellFormatting;
            dgvReport.DataBindingComplete += (s, args) => MakeColumnsNotSortable();

            lblNote.Font = UiTheme.FontSmall;
            lblNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;
        }

        private void BuildTiles()
        {
            Panel t1 = UiTheme.BuildTile("GROSS ITEM SALES", UiTheme.Primary, out _tileGross);
            Panel t2 = UiTheme.BuildTile("PLATFORM COMMISSION", UiTheme.Warning, out _tileCommission);
            Panel t3 = UiTheme.BuildTile("UNITS SOLD", UiTheme.Accent, out _tileUnits);
            Panel t4 = UiTheme.BuildTile("ORDERS", UiTheme.Success, out _tileOrders);
            _tiles = new[] { t1, t2, t3, t4 };
        }

        /// <summary>The tile row sits under the filter bar and spans the grid's width, also after a resize.</summary>
        private void LayoutTiles()
        {
            if (_tiles == null) return;
            UiTheme.LayoutTileRow(this, null, cmbStatus, ClientSize.Width - dgvReport.Right, _tiles);
        }

        // ---------------------------------------------------------------------

        private void Generate()
        {
            try
            {
                if (dtpTo.Value.Date < dtpFrom.Value.Date)
                {
                    MessageBox.Show("The end date cannot be earlier than the start date.",
                        "Check the date range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string area = cmbArea.SelectedIndex <= 0 ? "" : cmbArea.SelectedItem.ToString();
                string status = cmbStatus.SelectedIndex <= 0 ? "" : cmbStatus.SelectedItem.ToString();

                _current = _reports.GetEarnings(0, dtpFrom.Value, dtpTo.Value, area, status);

                decimal gross = 0m, commission = 0m;
                int units = 0, orders = 0;

                foreach (DataRow row in _current.Rows)
                {
                    gross += ValueOf(row, "GrossSales");
                    commission += ValueOf(row, "PlatformCommission");
                    units += (int)ValueOf(row, "UnitsSold");
                    orders += (int)ValueOf(row, "TotalOrders");
                }

                // The grid shows a copy with the total appended; _current stays
                // pure report rows for the export. No rows, no total row.
                DataTable display = _current.Copy();
                _showsTotalRow = _current.Rows.Count > 0;
                if (_showsTotalRow) AppendTotalRow(display, gross, commission, units, orders);

                dgvReport.DataSource = display;
                LabelColumns();
                MakeColumnsNotSortable();

                _tileGross.Text = UiTheme.Money(gross);
                _tileCommission.Text = UiTheme.Money(commission);
                _tileUnits.Text = units.ToString("N0");
                _tileOrders.Text = orders.ToString("N0");

                string range = dtpFrom.Value.ToString("dd MMM yyyy") + " and " + dtpTo.Value.ToString("dd MMM yyyy");
                string statusWords = status.Length == 0 ? "confirmed or delivered" : status.ToLowerInvariant();

                lblStatus.Text = _current.Rows.Count == 0
                    ? "No " + statusWords + " orders between " + range + "."
                    : _current.Rows.Count + " pharmac" + (_current.Rows.Count == 1 ? "y" : "ies") +
                      " with " + statusWords + " orders between " + range +
                      ".   PharmaLink keeps " + UiTheme.Money(commission) + " and settles " +
                      UiTheme.Money(gross - commission) + " to the pharmacies.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The report could not be generated.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static decimal ValueOf(DataRow row, string column)
        {
            if (row[column] == DBNull.Value) return 0m;
            return Convert.ToDecimal(row[column]);
        }

        /// <summary>The bold total row the grid ends with.</summary>
        private static void AppendTotalRow(DataTable table, decimal gross, decimal commission, int units, int orders)
        {
            DataRow total = table.NewRow();
            total["PharmacyId"] = 0;
            total["PharmacyName"] = "PLATFORM TOTAL";
            total["Area"] = "";
            total["TotalOrders"] = orders;
            total["UnitsSold"] = units;
            total["GrossSales"] = gross;
            total["PlatformCommission"] = commission;
            total["NetEarnings"] = gross - commission;
            total["AverageItemPrice"] = DBNull.Value;
            total["CommissionRate"] = DBNull.Value;
            table.Rows.Add(total);
        }

        /// <summary>Sorting would move the total row into the middle of the pharmacies, so it is switched off.</summary>
        private void MakeColumnsNotSortable()
        {
            foreach (DataGridViewColumn column in dgvReport.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void LabelColumns()
        {
            if (dgvReport.Columns.Count == 0) return;
            dgvReport.Columns["PharmacyId"].Visible = false;
            dgvReport.Columns["PharmacyName"].HeaderText = "Pharmacy";
            dgvReport.Columns["Area"].HeaderText = "Area";
            dgvReport.Columns["TotalOrders"].HeaderText = "Orders";
            dgvReport.Columns["UnitsSold"].HeaderText = "Units sold";
            dgvReport.Columns["GrossSales"].HeaderText = "Item sales (Tk)";
            dgvReport.Columns["PlatformCommission"].HeaderText = "Commission (Tk)";
            dgvReport.Columns["NetEarnings"].HeaderText = "Settled to shop (Tk)";
            dgvReport.Columns["AverageItemPrice"].HeaderText = "Avg item price";
            dgvReport.Columns["CommissionRate"].HeaderText = "Current rate %";
            dgvReport.Columns["CommissionRate"].FillWeight = 50;
        }

        /// <summary>The appended total row is drawn in bold so it reads as a summary, not a pharmacy.</summary>
        private void dgvReport_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || !_showsTotalRow || e.RowIndex != dgvReport.Rows.Count - 1) return;

            e.CellStyle.Font = UiTheme.FontBodyBold;
            e.CellStyle.BackColor = UiTheme.TotalRowBack;
        }

        // ---------------------------------------------------------------------

        private void btnGenerate_Click(object sender, EventArgs e) => Generate();

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_current == null)
            {
                MessageBox.Show("Generate the report first.", "PharmaLink",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_current.Rows.Count == 0)
            {
                MessageBox.Show("There is nothing to export for these filters.", "PharmaLink",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV file (*.csv)|*.csv";
                dialog.FileName = "PharmaLink-Sales-" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";

                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                // _current never contains the total row, so the file holds pharmacy rows only.
                string message;
                if (ReportService.ExportToCsv(_current, dialog.FileName, out message))
                {
                    lblStatus.Text = message;
                    MessageBox.Show(message, "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
