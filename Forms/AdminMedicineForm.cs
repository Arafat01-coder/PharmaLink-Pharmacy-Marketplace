using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 11: the full CRUD on the owner's own medicines.
    ///
    /// Create and Update open the modal MedicineEditorForm. Delete is a soft
    /// delete - IsActive is set to 0 - which keeps the foreign keys from
    /// OrderItems intact so every old invoice still resolves, while the item
    /// disappears from the customer catalogue.
    /// </summary>
    public partial class AdminMedicineForm : Form
    {
        private readonly MedicineService _medicines = new MedicineService();
        private bool _loading = true;

        /// <summary>
        /// Typing in the search box restarts this timer, and the grid reloads
        /// only once the owner pauses for 300 ms. Without it every keystroke ran
        /// a database query, so typing "paracetamol" ran eleven of them.
        /// </summary>
        private readonly System.Windows.Forms.Timer _searchTimer = new System.Windows.Forms.Timer { Interval = 300 };

        public AdminMedicineForm()
        {
            InitializeComponent();
            _searchTimer.Tick += SearchTimer_Tick;
            FormClosed += (s, e) => _searchTimer.Dispose();
        }

        private void AdminMedicineForm_Load(object sender, EventArgs e)
        {
            UiTheme.MakeResizable(this, Size);
            ApplyTheme();
            _loading = false;
            LoadGrid();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "My Medicines");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSecondary(btnRefresh);
            UiTheme.StyleSuccess(btnAdd);
            UiTheme.StylePrimary(btnEdit);
            UiTheme.StyleDanger(btnDelist);
            UiTheme.StyleAccent(btnRelist);
            UiTheme.StyleAccent(btnCreateOffer);
            UiTheme.StyleGrid(dgvMedicines);
            UiTheme.EnableEmptyMessage(dgvMedicines, "No medicines match this search.");
            dgvMedicines.DataBindingComplete += dgvMedicines_DataBindingComplete;

            lblIsolationHint.Font = UiTheme.FontSmall;
            lblIsolationHint.ForeColor = UiTheme.TextMuted;
            lblIsolationHint.Text =
                "Delisting takes a medicine off the customer catalogue but keeps it on past orders and invoices, " +
                "and you can put it back on sale at any time." + Environment.NewLine +
                "Tick \"Show delisted medicines too\" to see them. Only your own pharmacy's medicines are listed here.";

            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;
        }

        private void LoadGrid()
        {
            if (_loading) return;

            try
            {
                DataTable table = _medicines.GetForPharmacy(
                    UserSession.PharmacyId, txtSearch.Text.Trim(), chkShowDelisted.Checked);

                dgvMedicines.DataSource = table;

                if (dgvMedicines.Columns.Count > 0)
                {
                    dgvMedicines.Columns["MedicineId"].HeaderText = "ID";
                    dgvMedicines.Columns["MedicineName"].HeaderText = "Brand";
                    dgvMedicines.Columns["GenericName"].HeaderText = "Generic";
                    dgvMedicines.Columns["CategoryName"].HeaderText = "Category";
                    dgvMedicines.Columns["Manufacturer"].HeaderText = "Manufacturer";
                    dgvMedicines.Columns["Strength"].HeaderText = "Strength";
                    dgvMedicines.Columns["UnitPrice"].HeaderText = "Price (Tk)";
                    dgvMedicines.Columns["Stock"].HeaderText = "Stock";
                    dgvMedicines.Columns["MinStock"].HeaderText = "Min";
                    dgvMedicines.Columns["RequiresRx"].HeaderText = "Rx";
                    dgvMedicines.Columns["ExpiryDate"].HeaderText = "Expires";
                    dgvMedicines.Columns["ExpiryDate"].DefaultCellStyle.Format = "dd MMM yyyy";
                    dgvMedicines.Columns["IsActive"].HeaderText = "On sale";
                    dgvMedicines.Columns["StockStatus"].HeaderText = "Stock state";

                    UiTheme.SizeColumn(dgvMedicines, "MedicineId", 28, 45);
                    UiTheme.SizeColumn(dgvMedicines, "Strength", 45, 70);
                    UiTheme.SizeColumn(dgvMedicines, "UnitPrice", 45, 75);
                    UiTheme.SizeColumn(dgvMedicines, "Stock", 35, 55);
                    UiTheme.SizeColumn(dgvMedicines, "MinStock", 30, 45);
                    UiTheme.SizeColumn(dgvMedicines, "RequiresRx", 26, 40);
                    UiTheme.SizeColumn(dgvMedicines, "ExpiryDate", 60, 95);
                    UiTheme.SizeColumn(dgvMedicines, "IsActive", 40, 65);
                    UiTheme.SizeColumn(dgvMedicines, "StockStatus", 55, 90);
                }

                lblStatus.Text = table.Rows.Count + " medicine(s) listed for " + UserSession.PharmacyName + ".";
                UpdateButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your medicines could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvMedicines_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvMedicines.Columns.Contains("IsActive") || !dgvMedicines.Columns.Contains("StockStatus")) return;

            foreach (DataGridViewRow row in dgvMedicines.Rows)
            {
                object active = row.Cells["IsActive"].Value;
                string stockState = Convert.ToString(row.Cells["StockStatus"].Value);

                if (active != DBNull.Value && active != null && !Convert.ToBoolean(active))
                {
                    row.DefaultCellStyle.BackColor = UiTheme.InactiveBack;
                    row.DefaultCellStyle.ForeColor = UiTheme.TextMuted;
                }
                else if (stockState == "Low Stock")
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

        private void dgvMedicines_SelectionChanged(object sender, EventArgs e) => UpdateButtons();

        private void UpdateButtons()
        {
            DataGridViewRow row = dgvMedicines.CurrentRow;
            bool hasRow = row != null && dgvMedicines.Columns.Contains("MedicineId") &&
                          row.Cells["MedicineId"].Value != null;

            bool active = hasRow && row.Cells["IsActive"].Value != DBNull.Value &&
                          Convert.ToBoolean(row.Cells["IsActive"].Value);

            btnEdit.Enabled = hasRow;
            btnDelist.Enabled = hasRow && active;
            btnRelist.Enabled = hasRow && !active;
            btnCreateOffer.Enabled = hasRow && active;
        }

        private int SelectedId()
        {
            if (dgvMedicines.CurrentRow == null || !dgvMedicines.Columns.Contains("MedicineId")) return 0;
            return Convert.ToInt32(dgvMedicines.CurrentRow.Cells["MedicineId"].Value);
        }

        private string SelectedName()
        {
            if (dgvMedicines.CurrentRow == null || !dgvMedicines.Columns.Contains("MedicineName")) return "";
            return Convert.ToString(dgvMedicines.CurrentRow.Cells["MedicineName"].Value);
        }

        // ---------------------------------------------------------------------

        private void btnAdd_Click(object sender, EventArgs e)
        {
            bool added;
            using (MedicineEditorForm editor = new MedicineEditorForm(0, false))
            {
                added = editor.ShowDialog(this) == DialogResult.OK;
            }
            LoadGrid();
            if (added) lblStatus.Text = "Medicine added and immediately visible to customers.";
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            int id = SelectedId();
            if (id == 0) return;

            string saved = "";
            using (MedicineEditorForm editor = new MedicineEditorForm(id, false))
            {
                if (editor.ShowDialog(this) == DialogResult.OK) saved = editor.SavedMessage;
            }
            LoadGrid();
            if (saved.Length > 0) lblStatus.Text = saved;
        }

        private void dgvMedicines_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnEdit_Click(sender, EventArgs.Empty);
        }

        private void btnDelist_Click(object sender, EventArgs e)
        {
            int id = SelectedId();
            if (id == 0) return;
            string name = SelectedName();

            DialogResult answer = MessageBox.Show(
                "Take " + name + " off sale?\r\n\r\n" +
                "Customers stop seeing it straight away. It stays on past orders and invoices, " +
                "and you can put it back on sale at any time.",
                "Delist medicine", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes) return;

            try
            {
                bool changed = _medicines.Delist(id, UserSession.PharmacyId);
                LoadGrid();
                lblStatus.Text = changed
                    ? name + " has been taken off sale."
                    : name + " was not changed - it is no longer in your list. The list has been refreshed.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The medicine could not be delisted.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRelist_Click(object sender, EventArgs e)
        {
            int id = SelectedId();
            if (id == 0) return;
            string name = SelectedName();

            try
            {
                bool changed = _medicines.Relist(id, UserSession.PharmacyId);
                LoadGrid();
                lblStatus.Text = changed
                    ? name + " is on sale again."
                    : name + " was not changed - it is no longer in your list. The list has been refreshed.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The medicine could not be put back on sale.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCreateOffer_Click(object sender, EventArgs e)
        {
            int id = SelectedId();
            if (id == 0) return;

            using (DiscountOffersForm offers = new DiscountOffersForm(id))
            {
                offers.ShowDialog(this);
            }
            LoadGrid();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            LoadGrid();
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            LoadGrid();
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
