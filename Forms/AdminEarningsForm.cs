using System.Data;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 13. The owner's own sales, line by line, with the totals
    /// PharmaLink settles against.
    ///
    /// Every figure comes from ReportService with UserSession.PharmacyId in
    /// the WHERE clause, so an owner only ever sees his own sales. The tiles
    /// count Confirmed and Delivered orders only, item lines without the
    /// delivery charge, and follow the same date range and medicine filter as
    /// the grid, so the numbers on top always describe the rows underneath.
    /// </summary>
    public partial class AdminEarningsForm : Form
    {
        private readonly ReportService _reports = new ReportService();
        private readonly MedicineService _medicines = new MedicineService();

        private Panel[] _tiles;
        private Label _tileGross;
        private Label _tileCommission;
        private Label _tileNet;
        private Label _tileUnits;

        // index 0 of the ComboBox is "All my medicines"; index i is _medicineList[i - 1]
        private List<Medicine> _medicineList = new List<Medicine>();
        private DataTable _current;

        public AdminEarningsForm()
        {
            InitializeComponent();
        }

        private void AdminEarningsForm_Load(object sender, EventArgs e)
        {
            UiTheme.MakeResizable(this, Size);
            ApplyTheme();
            BuildTiles();
            Resize += (s, args) => LayoutTiles();

            dtpFrom.Value = DateTime.Today.AddDays(-30);
            dtpTo.Value = DateTime.Today;

            try
            {
                LoadMedicineFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(DbHelper.Describe(ex), "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Generate();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Sales and Earnings");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StylePrimary(btnGenerate);
            UiTheme.StyleAccent(btnExport);
            UiTheme.StyleGrid(dgvSales);
            UiTheme.EnableEmptyMessage(dgvSales, "No sales in this date range for the chosen medicine.");

            lblNote.Font = UiTheme.FontSmall;
            lblNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;
        }

        private void BuildTiles()
        {
            Panel t1 = UiTheme.BuildTile("ITEM SALES (CONFIRMED + DELIVERED)", UiTheme.Primary, out _tileGross);
            Panel t2 = UiTheme.BuildTile("PHARMALINK COMMISSION", UiTheme.Warning, out _tileCommission);
            Panel t3 = UiTheme.BuildTile("NET EARNINGS (YOURS)", UiTheme.Success, out _tileNet);
            Panel t4 = UiTheme.BuildTile("UNITS SOLD", UiTheme.Accent, out _tileUnits);

            _tiles = new[] { t1, t2, t3, t4 };
            LayoutTiles();
        }

        /// <summary>The tile row sits under the filter bar and follows the window width.</summary>
        private void LayoutTiles()
        {
            if (_tiles == null) return;
            UiTheme.LayoutTileRow(this, null, btnGenerate, ClientSize.Width - dgvSales.Right, _tiles);
        }

        /// <summary>
        /// Every medicine this pharmacy has ever listed, delisted ones included
        /// and marked, because a medicine taken off sale last week still has
        /// sales inside last month's range.
        /// </summary>
        private void LoadMedicineFilter()
        {
            _medicineList = _medicines.GetSimpleListForPharmacy(UserSession.PharmacyId, true);

            cmbMedicine.Items.Clear();
            cmbMedicine.Items.Add("All my medicines");
            foreach (Medicine medicine in _medicineList)
            {
                cmbMedicine.Items.Add((medicine.MedicineName + " " + medicine.Strength).Trim() +
                                      (medicine.IsActive ? "" : "  (delisted)"));
            }

            cmbMedicine.SelectedIndex = 0;
        }

        private int SelectedMedicineId()
        {
            int index = cmbMedicine.SelectedIndex;
            if (index <= 0 || index > _medicineList.Count) return 0;
            return _medicineList[index - 1].MedicineId;
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

                int medicineId = SelectedMedicineId();

                decimal gross, commission, net;
                int units;
                _reports.GetEarningsTotals(UserSession.PharmacyId, dtpFrom.Value, dtpTo.Value, medicineId,
                                           out gross, out commission, out net, out units);

                _tileGross.Text = UiTheme.Money(gross);
                _tileCommission.Text = UiTheme.Money(commission);
                _tileNet.Text = UiTheme.Money(net);
                _tileUnits.Text = units.ToString("N0");

                _current = _reports.GetSalesDetail(UserSession.PharmacyId, dtpFrom.Value, dtpTo.Value, medicineId);
                dgvSales.DataSource = _current;

                if (dgvSales.Columns.Count > 0)
                {
                    dgvSales.Columns["OrderId"].HeaderText = "Order";
                    dgvSales.Columns["OrderDate"].HeaderText = "Date and time";
                    dgvSales.Columns["OrderDate"].DefaultCellStyle.Format = "dd MMM yyyy, HH:mm";
                    dgvSales.Columns["Customer"].HeaderText = "Bought by";
                    dgvSales.Columns["MedicineName"].HeaderText = "Medicine";
                    dgvSales.Columns["Strength"].HeaderText = "Strength";
                    dgvSales.Columns["Quantity"].HeaderText = "Qty";
                    dgvSales.Columns["UnitPrice"].HeaderText = "Unit price (Tk)";
                    dgvSales.Columns["Subtotal"].HeaderText = "Line total (Tk)";
                    dgvSales.Columns["PaymentMethod"].HeaderText = "Payment";
                    dgvSales.Columns["Status"].HeaderText = "Status";

                    UiTheme.SizeColumn(dgvSales, "OrderId", 42, 60);
                    UiTheme.SizeColumn(dgvSales, "OrderDate", 100, 140);
                    UiTheme.SizeColumn(dgvSales, "Strength", 50, 75);
                    UiTheme.SizeColumn(dgvSales, "Quantity", 32, 50);
                    UiTheme.SizeColumn(dgvSales, "PaymentMethod", 80, 110);
                    UiTheme.SizeColumn(dgvSales, "Status", 55, 85);
                }

                lblStatus.Text = _current.Rows.Count + " sale line(s) between " +
                                 dtpFrom.Value.ToString("dd MMM yyyy") + " and " +
                                 dtpTo.Value.ToString("dd MMM yyyy") +
                                 ".   PharmaLink keeps " + UiTheme.Money(commission) +
                                 " and settles " + UiTheme.Money(net) + " to " + UserSession.PharmacyName + ".";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The report could not be generated.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e) => Generate();

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_current == null || _current.Rows.Count == 0)
            {
                MessageBox.Show("There is nothing to export for this date range.", "PharmaLink",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV file (*.csv)|*.csv";
                dialog.FileName = "PharmaLink-Earnings-" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    string message;
                    bool saved = ReportService.ExportToCsv(_current, dialog.FileName, out message);
                    lblStatus.Text = message;
                    if (!saved)
                        MessageBox.Show(message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The file could not be saved.\r\n\r\n" + DbHelper.Describe(ex),
                        "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
