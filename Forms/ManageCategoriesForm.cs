using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 7. The master category list, with Add, Save, Deactivate and
    /// Reactivate.
    ///
    /// A category that a medicine already references is never deleted, only
    /// deactivated, which is what keeps every foreign key from Medicines valid.
    ///
    /// The editor only shows a category the Super Admin clicked on. The grid
    /// normally selects its first row by itself whenever it is bound, which
    /// used to load that row into the editor after Clear or after a save, so
    /// the next "Add new" silently started from somebody else's category.
    /// Those automatic selections are suppressed.
    /// </summary>
    public partial class ManageCategoriesForm : Form
    {
        private readonly CategoryService _categories = new CategoryService();
        private int _selectedId;
        private bool _suppressSelection;

        public ManageCategoriesForm()
        {
            InitializeComponent();
        }

        private void ManageCategoriesForm_Load(object sender, EventArgs e)
        {
            UiTheme.MakeResizable(this, Size);
            ApplyTheme();
            LoadGrid();
            Shown += (s, args) => { if (_selectedId == 0) DeselectGrid(); };
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Medicine Categories");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            grpEditor.Font = UiTheme.FontHeading;
            grpEditor.ForeColor = UiTheme.Primary;
            grpEditor.BackColor = UiTheme.CardBack;
            foreach (Control child in grpEditor.Controls)
            {
                child.Font = UiTheme.FontBody;
                child.ForeColor = UiTheme.TextDark;
            }

            lblNameError.Font = UiTheme.FontSmall;
            lblNameError.ForeColor = UiTheme.Danger;
            lblEditorNote.Font = UiTheme.FontSmall;
            lblEditorNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSuccess(btnAdd);
            UiTheme.StylePrimary(btnUpdate);
            UiTheme.StyleDanger(btnDeactivate);
            UiTheme.StyleAccent(btnActivate);
            UiTheme.StyleSecondary(btnNew);
            UiTheme.StyleGrid(dgvCategories);
            UiTheme.EnableEmptyMessage(dgvCategories, "No categories yet. Add the first one on the right.");
            dgvCategories.DataBindingComplete += dgvCategories_DataBindingComplete;
        }

        /// <summary>
        /// Reloads the grid and then puts the selection back on the category
        /// being edited, or on nothing, without touching the editor.
        /// </summary>
        private void LoadGrid()
        {
            try
            {
                DataTable table = _categories.GetTable(!chkShowInactive.Checked);

                _suppressSelection = true;
                try
                {
                    dgvCategories.DataSource = table;
                }
                finally
                {
                    _suppressSelection = false;
                }

                if (dgvCategories.Columns.Count > 0)
                {
                    dgvCategories.Columns["CategoryId"].HeaderText = "ID";
                    dgvCategories.Columns["CategoryName"].HeaderText = "Category";
                    dgvCategories.Columns["Description"].HeaderText = "Description";
                    dgvCategories.Columns["IsActive"].HeaderText = "Active";
                    dgvCategories.Columns["MedicineCount"].HeaderText = "Medicines";

                    UiTheme.SizeColumn(dgvCategories, "CategoryId", 25, 40);
                    UiTheme.SizeColumn(dgvCategories, "CategoryName", 70, 110);
                    UiTheme.SizeColumn(dgvCategories, "Description", 130, 120);
                    UiTheme.SizeColumn(dgvCategories, "IsActive", 35, 55);
                    UiTheme.SizeColumn(dgvCategories, "MedicineCount", 45, 80);
                }

                if (_selectedId > 0 && !SelectRowById(_selectedId))
                {
                    // The category being edited is no longer listed (for example it
                    // was deactivated while inactive ones are hidden).
                    ClearEditor();
                }
                else if (_selectedId == 0)
                {
                    DeselectGrid();
                }

                lblStatus.Text = table.Rows.Count + " categor" + (table.Rows.Count == 1 ? "y" : "ies") + " listed.";
                UpdateButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The categories could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvCategories_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvCategories.Columns.Contains("IsActive")) return;

            foreach (DataGridViewRow row in dgvCategories.Rows)
            {
                object value = row.Cells["IsActive"].Value;
                bool active = value == null || value == DBNull.Value || Convert.ToBoolean(value);
                row.DefaultCellStyle.ForeColor = active ? UiTheme.TextDark : UiTheme.TextMuted;
                row.DefaultCellStyle.BackColor = active ? Color.Empty : UiTheme.InactiveBack;
            }
        }

        private void DeselectGrid()
        {
            _suppressSelection = true;
            try
            {
                dgvCategories.ClearSelection();
                dgvCategories.CurrentCell = null;
            }
            finally
            {
                _suppressSelection = false;
            }
        }

        private bool SelectRowById(int categoryId)
        {
            foreach (DataGridViewRow row in dgvCategories.Rows)
            {
                if (Convert.ToInt32(row.Cells["CategoryId"].Value) != categoryId) continue;

                _suppressSelection = true;
                try
                {
                    dgvCategories.ClearSelection();
                    dgvCategories.CurrentCell = row.Cells["CategoryName"];
                    row.Selected = true;
                }
                finally
                {
                    _suppressSelection = false;
                }
                return true;
            }
            return false;
        }

        private DataGridViewRow SelectedRow()
        {
            if (dgvCategories.SelectedRows.Count == 0 || !dgvCategories.Columns.Contains("CategoryId")) return null;
            return dgvCategories.SelectedRows[0];
        }

        private void dgvCategories_SelectionChanged(object sender, EventArgs e)
        {
            if (_suppressSelection) return;

            DataGridViewRow row = SelectedRow();
            if (row == null || row.Cells["CategoryId"].Value == null) return;

            _selectedId = Convert.ToInt32(row.Cells["CategoryId"].Value);
            txtName.Text = Convert.ToString(row.Cells["CategoryName"].Value);
            txtDescription.Text = row.Cells["Description"].Value == DBNull.Value
                ? "" : Convert.ToString(row.Cells["Description"].Value);

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool hasSelection = _selectedId > 0;
            bool nameOk = !Validator.IsBlank(txtName.Text);

            btnAdd.Enabled = nameOk;
            btnUpdate.Enabled = hasSelection && nameOk;

            DataGridViewRow row = SelectedRow();
            if (hasSelection && row != null && row.Cells["IsActive"].Value != DBNull.Value)
            {
                bool active = Convert.ToBoolean(row.Cells["IsActive"].Value);
                btnDeactivate.Enabled = active;
                btnActivate.Enabled = !active;
            }
            else
            {
                btnDeactivate.Enabled = false;
                btnActivate.Enabled = false;
            }
        }

        // ---------------------------------------------------------------------
        //  VALIDATION
        //  The name is checked for emptiness and for duplicates before anything
        //  is sent, so a duplicate produces a red label rather than an error
        //  dialog. The database still refuses a duplicate that slips past (two
        //  admins at once), and that case is shown next to the name as well.
        // ---------------------------------------------------------------------

        private const string DuplicateMessageFormat = "A category called '{0}' already exists.";

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Validator.IsBlank(txtName.Text))
                    UiTheme.ClearError(lblNameError, txtName);
                else if (_categories.NameExists(txtName.Text, _selectedId))
                    UiTheme.ShowError(lblNameError, txtName, string.Format(DuplicateMessageFormat, txtName.Text.Trim()));
                else
                    UiTheme.ClearError(lblNameError, txtName);
            }
            catch (Exception)
            {
                // A lookup failure while typing is not the user's mistake; Save reports it.
                UiTheme.ClearError(lblNameError, txtName);
            }

            UpdateButtons();
        }

        private bool NameIsUsable(int ignoreId)
        {
            if (Validator.IsBlank(txtName.Text))
            {
                UiTheme.ShowError(lblNameError, txtName, "The category name cannot be empty.");
                return false;
            }

            if (_categories.NameExists(txtName.Text, ignoreId))
            {
                UiTheme.ShowError(lblNameError, txtName, string.Format(DuplicateMessageFormat, txtName.Text.Trim()));
                return false;
            }

            UiTheme.ClearError(lblNameError, txtName);
            return true;
        }

        /// <summary>A duplicate refused by the database goes next to the name; anything else is a dialog.</summary>
        private void ShowSaveError(Exception ex, string action)
        {
            SqlException sql = ex as SqlException ?? ex.InnerException as SqlException;
            if (sql != null && (sql.Number == 2627 || sql.Number == 2601))
            {
                UiTheme.ShowError(lblNameError, txtName, string.Format(DuplicateMessageFormat, txtName.Text.Trim()));
                return;
            }

            MessageBox.Show("The category could not be " + action + ".\r\n\r\n" + DbHelper.Describe(ex),
                "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // ---------------------------------------------------------------------

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();

            try
            {
                if (!NameIsUsable(0)) return;

                _categories.Add(txtName.Text, txtDescription.Text);
                ClearEditor();
                LoadGrid();
                lblStatus.Text = "Category '" + name + "' added.";
            }
            catch (Exception ex)
            {
                ShowSaveError(ex, "added");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedId == 0) return;

            try
            {
                if (!NameIsUsable(_selectedId)) return;

                bool saved = _categories.Update(_selectedId, txtName.Text, txtDescription.Text);
                if (!saved) ClearEditor();
                LoadGrid();
                lblStatus.Text = saved
                    ? "Category saved. Every medicine in it now shows the new name."
                    : "Nothing was saved - that category no longer exists.";
            }
            catch (Exception ex)
            {
                ShowSaveError(ex, "saved");
            }
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            if (_selectedId == 0) return;

            try
            {
                bool referenced = _categories.IsReferenced(_selectedId);
                string extra = referenced
                    ? "\r\n\r\nMedicines already use this category, so it can only be deactivated, never deleted."
                    : "";

                DialogResult answer = MessageBox.Show(
                    "Deactivate '" + txtName.Text.Trim() + "'?\r\n\r\n" +
                    "It disappears from every category dropdown but existing medicines keep working." + extra,
                    "Deactivate category", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (answer != DialogResult.Yes) return;

                bool changed = _categories.SetActive(_selectedId, false);
                LoadGrid();
                lblStatus.Text = changed
                    ? "Category deactivated. It is kept, so it can be reactivated at any time."
                    : "Nothing changed - that category no longer exists.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The category could not be deactivated.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            if (_selectedId == 0) return;

            try
            {
                bool changed = _categories.SetActive(_selectedId, true);
                LoadGrid();
                lblStatus.Text = changed
                    ? "Category reactivated."
                    : "Nothing changed - that category no longer exists.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The category could not be reactivated.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnNew_Click(object sender, EventArgs e) => ClearEditor();

        private void ClearEditor()
        {
            _selectedId = 0;
            txtName.Clear();
            txtDescription.Clear();
            UiTheme.ClearError(lblNameError, txtName);
            DeselectGrid();
            UpdateButtons();
        }

        private void chkShowInactive_CheckedChanged(object sender, EventArgs e) => LoadGrid();
        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
