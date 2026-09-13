using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 12. Summary tiles, the low stock alert panel and the full
    /// inventory grid.
    ///
    /// The alert compares two columns of the same row - Stock against MinStock -
    /// rather than using a fixed threshold, because ten boxes of a glucometer is
    /// plenty while ten strips of Napa is nothing. The shortfall column tells
    /// the owner how many units to order.
    ///
    /// Expiry is flagged too: a medicine that expires stops being sold to
    /// customers automatically, so stock that expires within 60 days is stock
    /// the owner should sell, return or stop reordering now.
    /// </summary>
    public partial class AdminInventoryForm : Form
    {
        /// <summary>How far ahead an expiry date counts as "expiring soon".</summary>
        private const int ExpiryWarningDays = 60;

        private readonly MedicineService _medicines = new MedicineService();

        private Panel[] _tiles;
        private Label _tileItems;
        private Label _tileLowStock;
        private Label _tileUnitsInStock;
        private Label _tileStockValue;
        private Label _tileExpiring;

        public AdminInventoryForm()
        {
            InitializeComponent();
        }

        private void AdminInventoryForm_Load(object sender, EventArgs e)
        {
            UiTheme.MakeResizable(this, Size);
            ApplyTheme();
            BuildTiles();
            Resize += (s, args) => LayoutTiles();
            LoadEverything();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Stock and Inventory");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            lblAlertTitle.Font = UiTheme.FontHeading;
            lblAlertTitle.ForeColor = UiTheme.Danger;
            lblInventoryTitle.Font = UiTheme.FontHeading;
            lblInventoryTitle.ForeColor = UiTheme.TextDark;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;
            lblRestockError.Font = UiTheme.FontSmall;
            lblRestockError.ForeColor = UiTheme.Danger;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSuccess(btnRestock);
            UiTheme.StyleSecondary(btnOpenEditor);
            UiTheme.StyleGrid(dgvLowStock);
            UiTheme.StyleGrid(dgvInventory);
            UiTheme.EnableEmptyMessage(dgvLowStock, "Nothing is below its minimum stock level right now.");
            UiTheme.EnableEmptyMessage(dgvInventory, "You have no medicines on sale yet. Add them from My Medicines.");

            dgvLowStock.DataBindingComplete += dgvLowStock_DataBindingComplete;
            dgvInventory.DataBindingComplete += dgvInventory_DataBindingComplete;
        }

        private void BuildTiles()
        {
            Panel t1 = UiTheme.BuildTile("MEDICINES ON SALE", UiTheme.Primary, out _tileItems);
            Panel t2 = UiTheme.BuildTile("BELOW MINIMUM STOCK", UiTheme.Danger, out _tileLowStock);
            Panel t3 = UiTheme.BuildTile("UNITS ON THE SHELF", UiTheme.Accent, out _tileUnitsInStock);
            Panel t4 = UiTheme.BuildTile("VALUE OF STOCK HELD", UiTheme.Success, out _tileStockValue);
            Panel t5 = UiTheme.BuildTile("EXPIRED OR EXPIRING IN " + ExpiryWarningDays + " DAYS", UiTheme.Warning, out _tileExpiring);

            _tiles = new[] { t1, t2, t3, t4, t5 };
            LayoutTiles();
        }

        private void LayoutTiles()
        {
            if (_tiles == null) return;
            UiTheme.LayoutTileRow(this, null, panelHeader, ClientSize.Width - dgvInventory.Right, _tiles);
        }

        private void LoadEverything()
        {
            try
            {
                DataTable lowStock = _medicines.GetLowStock(UserSession.PharmacyId);
                dgvLowStock.DataSource = lowStock;

                if (dgvLowStock.Columns.Count > 0)
                {
                    dgvLowStock.Columns["MedicineId"].HeaderText = "ID";
                    dgvLowStock.Columns["MedicineName"].HeaderText = "Medicine";
                    dgvLowStock.Columns["Strength"].HeaderText = "Strength";
                    dgvLowStock.Columns["CategoryName"].HeaderText = "Category";
                    dgvLowStock.Columns["Stock"].HeaderText = "In stock";
                    dgvLowStock.Columns["MinStock"].HeaderText = "Minimum level";
                    dgvLowStock.Columns["ShortfallUnits"].HeaderText = "Shortfall (units to order)";
                    UiTheme.SizeColumn(dgvLowStock, "MedicineId", 30, 45);
                    UiTheme.SizeColumn(dgvLowStock, "ShortfallUnits", 100, 170);
                }

                DataTable inventory = _medicines.GetInventory(UserSession.PharmacyId);
                dgvInventory.DataSource = inventory;

                if (dgvInventory.Columns.Count > 0)
                {
                    dgvInventory.Columns["MedicineId"].HeaderText = "ID";
                    dgvInventory.Columns["MedicineName"].HeaderText = "Medicine";
                    dgvInventory.Columns["Strength"].HeaderText = "Strength";
                    dgvInventory.Columns["CategoryName"].HeaderText = "Category";
                    dgvInventory.Columns["UnitsRemaining"].HeaderText = "Units left";
                    dgvInventory.Columns["UnitsSold"].HeaderText = "Units sold";
                    dgvInventory.Columns["MinStock"].HeaderText = "Min level";
                    dgvInventory.Columns["UnitPrice"].HeaderText = "Price (Tk)";
                    dgvInventory.Columns["StockValue"].HeaderText = "Stock value (Tk)";
                    dgvInventory.Columns["StockStatus"].HeaderText = "State";
                    dgvInventory.Columns["ExpiryDate"].HeaderText = "Expires";
                    dgvInventory.Columns["ExpiryDate"].DefaultCellStyle.Format = "dd MMM yyyy";

                    UiTheme.SizeColumn(dgvInventory, "MedicineId", 28, 45);
                    UiTheme.SizeColumn(dgvInventory, "Strength", 45, 70);
                    UiTheme.SizeColumn(dgvInventory, "ExpiryDate", 70, 100);
                }

                int units = 0;
                decimal value = 0m;
                int expiring = 0;
                DateTime warnBefore = DateTime.Today.AddDays(ExpiryWarningDays);
                foreach (DataRow row in inventory.Rows)
                {
                    units += DbHelper.GetInt(row, "UnitsRemaining");
                    value += DbHelper.GetDecimal(row, "StockValue");
                    if (DbHelper.GetDate(row, "ExpiryDate") <= warnBefore) expiring++;
                }

                _tileItems.Text = inventory.Rows.Count.ToString("N0");
                _tileLowStock.Text = lowStock.Rows.Count.ToString("N0");
                _tileUnitsInStock.Text = units.ToString("N0");
                _tileStockValue.Text = UiTheme.Money(value);
                _tileExpiring.Text = expiring.ToString("N0");

                lblAlertTitle.Text = lowStock.Rows.Count == 0
                    ? "Low stock alert  -  nothing is below its minimum level right now"
                    : "Low stock alert  (" + lowStock.Rows.Count + " medicine(s) need reordering)";

                lblInventoryTitle.Text = expiring == 0
                    ? "Full inventory - sold, remaining and stock value"
                    : "Full inventory - " + expiring + " medicine(s) expired or expiring within " +
                      ExpiryWarningDays + " days are marked in the Expires column";

                lblStatus.Text = "Double click any inventory row to open the medicine editor. " +
                                 "Expired medicines are hidden from customers automatically.";

                UpdateRestockButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The inventory could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Rows in the alert panel are all below minimum, so all of them are painted red.</summary>
        private void dgvLowStock_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvLowStock.Rows)
                row.DefaultCellStyle.BackColor = UiTheme.LowStockBack;
        }

        /// <summary>
        /// Low stock rows are painted red; the Expires cell is painted amber when
        /// the date is within the warning window and red with bold text once it
        /// has passed. Done once per binding rather than on every cell paint.
        /// </summary>
        private void dgvInventory_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvInventory.Columns.Contains("StockStatus") || !dgvInventory.Columns.Contains("ExpiryDate")) return;

            DateTime warnBefore = DateTime.Today.AddDays(ExpiryWarningDays);
            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                row.DefaultCellStyle.BackColor = Convert.ToString(row.Cells["StockStatus"].Value) == "Low Stock"
                    ? UiTheme.LowStockBack
                    : Color.Empty;

                DataGridViewCell expiryCell = row.Cells["ExpiryDate"];
                object raw = expiryCell.Value;
                if (raw == null || raw == DBNull.Value) continue;

                DateTime expiry = Convert.ToDateTime(raw);
                if (expiry <= DateTime.Today)
                {
                    expiryCell.Style.BackColor = UiTheme.LowStockBack;
                    expiryCell.Style.ForeColor = UiTheme.Danger;
                    expiryCell.Style.Font = UiTheme.FontBodyBold;
                    expiryCell.ToolTipText = "Expired - no longer sold to customers";
                }
                else if (expiry <= warnBefore)
                {
                    expiryCell.Style.BackColor = UiTheme.WarningBack;
                    expiryCell.ToolTipText = "Expires within " + ExpiryWarningDays + " days";
                }
            }
        }

        // ---------------------------------------------------------------------
        //  RESTOCK
        // ---------------------------------------------------------------------

        /// <summary>
        /// Moving to another alert row always puts that row's shortfall in the
        /// box. Keeping the previous number would restock the new medicine by
        /// the old medicine's shortfall.
        /// </summary>
        private void dgvLowStock_SelectionChanged(object sender, EventArgs e)
        {
            DataGridViewRow row = dgvLowStock.CurrentRow;
            if (row != null && dgvLowStock.Columns.Contains("ShortfallUnits"))
            {
                object shortfall = row.Cells["ShortfallUnits"].Value;
                txtRestockUnits.Text = shortfall == null || shortfall == DBNull.Value ? "" : shortfall.ToString();
            }
            UpdateRestockButton();
        }

        private void txtRestockUnits_TextChanged(object sender, EventArgs e)
        {
            int units;
            if (string.IsNullOrWhiteSpace(txtRestockUnits.Text))
            {
                UiTheme.ClearError(lblRestockError, txtRestockUnits);
            }
            else if (!Validator.IsPositiveInt(txtRestockUnits.Text, out units))
            {
                UiTheme.ShowError(lblRestockError, txtRestockUnits,
                    "Enter a whole number of units greater than zero.");
            }
            else
            {
                UiTheme.ClearError(lblRestockError, txtRestockUnits);
            }

            UpdateRestockButton();
        }

        private void UpdateRestockButton()
        {
            int units;
            bool hasRow = dgvLowStock.CurrentRow != null && dgvLowStock.Columns.Contains("MedicineId") &&
                          dgvLowStock.CurrentRow.Cells["MedicineId"].Value != null;
            bool unitsOk = Validator.IsPositiveInt(txtRestockUnits.Text, out units);

            btnRestock.Enabled = hasRow && unitsOk;
            btnOpenEditor.Enabled = hasRow;
        }

        private void btnRestock_Click(object sender, EventArgs e)
        {
            if (dgvLowStock.CurrentRow == null) return;

            int units;
            if (!Validator.IsPositiveInt(txtRestockUnits.Text, out units)) return;

            int medicineId = Convert.ToInt32(dgvLowStock.CurrentRow.Cells["MedicineId"].Value);
            string name = Convert.ToString(dgvLowStock.CurrentRow.Cells["MedicineName"].Value);

            try
            {
                bool added = _medicines.AddStock(medicineId, UserSession.PharmacyId, units);
                LoadEverything();

                if (added)
                {
                    lblStatus.Text = units + " unit(s) added to " + name + ".";
                }
                else
                {
                    lblStatus.Text = "No stock was added.";
                    MessageBox.Show("No stock was added: " + name + " is no longer in your pharmacy's list. " +
                                    "The screen has been refreshed.",
                        "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The stock could not be updated.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenEditor_Click(object sender, EventArgs e)
        {
            if (dgvLowStock.CurrentRow == null) return;
            OpenEditor(Convert.ToInt32(dgvLowStock.CurrentRow.Cells["MedicineId"].Value));
        }

        private void dgvInventory_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            OpenEditor(Convert.ToInt32(dgvInventory.Rows[e.RowIndex].Cells["MedicineId"].Value));
        }

        private void OpenEditor(int medicineId)
        {
            string saved = "";
            using (MedicineEditorForm editor = new MedicineEditorForm(medicineId, true))
            {
                if (editor.ShowDialog(this) == DialogResult.OK) saved = editor.SavedMessage;
            }
            LoadEverything();
            if (saved.Length > 0) lblStatus.Text = saved;
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
