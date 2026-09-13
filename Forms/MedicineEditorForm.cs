using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// The Add / Edit Medicine modal dialog (requirement 12).
    ///
    /// This is the form the navigation diagram draws with a dashed border. Every
    /// field is validated as it is typed: a failing rule paints the field red,
    /// explains the rule in plain words (the database enforces the same rules
    /// again) and keeps the Save button disabled.
    ///
    /// Editing is careful with data the dialog does not own. Stock is saved as
    /// a change on top of whatever is on the shelf when Save is pressed, so a
    /// sale made while the dialog was open is never undone; the image path is
    /// never touched; a category the Super Admin has deactivated stays selected
    /// instead of silently switching to another; and an already expired
    /// medicine can still be corrected without inventing a new expiry date.
    /// </summary>
    public partial class MedicineEditorForm : Form
    {
        private readonly MedicineService _medicines = new MedicineService();
        private readonly CategoryService _categories = new CategoryService();

        private readonly int _medicineId;      // 0 means "add a new one"
        private readonly bool _focusStock;     // opened from the Restock button
        private bool _loading = true;

        // what the dialog showed when it opened, for the stock delta and the expiry rule
        private int _originalStock;
        private DateTime _originalExpiry = DateTime.MinValue;

        /// <summary>
        /// The duplicate name check is a database query, so it waits until the
        /// owner stops typing for 400 ms instead of running on every keystroke.
        /// </summary>
        private readonly System.Windows.Forms.Timer _duplicateTimer = new System.Windows.Forms.Timer { Interval = 400 };
        private bool _duplicateName;

        /// <summary>A sentence the calling screen can show in its status bar after a successful save.</summary>
        public string SavedMessage { get; private set; } = "";

        public MedicineEditorForm(int medicineId, bool focusStock)
        {
            InitializeComponent();
            _medicineId = medicineId;
            _focusStock = focusStock;
            _duplicateTimer.Tick += DuplicateTimer_Tick;
            FormClosed += (s, e) => _duplicateTimer.Dispose();
        }

        private void MedicineEditorForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();

            try
            {
                if (_medicineId > 0)
                {
                    if (!LoadExisting()) return;   // the dialog is already closing
                }
                else
                {
                    if (!LoadCategories(0)) return;
                    lblTitle.Text = "Add Medicine";
                    dtpExpiry.Value = DateTime.Today.AddYears(2);
                    txtStock.Text = "0";
                    txtMinStock.Text = "10";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The medicine could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _loading = false;
            RunDuplicateCheck();
            ValidateAll();

            if (_focusStock)
            {
                txtStock.Focus();
                txtStock.SelectAll();
            }
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Medicine");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            foreach (Control control in Controls)
            {
                if (control is Label label && label.Name.EndsWith("Error"))
                {
                    label.Font = UiTheme.FontSmall;
                    label.ForeColor = UiTheme.Danger;
                }
            }

            lblConstraintNote.Font = UiTheme.FontSmall;
            lblConstraintNote.ForeColor = UiTheme.TextMuted;

            UiTheme.StylePrimary(btnSave);
            btnSave.Font = UiTheme.FontButtonStrong;
            UiTheme.StyleSecondary(btnCancel);
        }

        /// <summary>
        /// Fills the category list. Returns false (and closes the dialog) when
        /// there is nothing to choose from, because a medicine must have one.
        /// </summary>
        private bool LoadCategories(int currentCategoryId)
        {
            List<Category> list = _categories.GetListForEditor(currentCategoryId);

            if (list.Count == 0)
            {
                MessageBox.Show("There are no active medicine categories yet, so a medicine cannot be added.\r\n\r\n" +
                                "Ask the PharmaLink Super Admin to add categories first.",
                    "No categories", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
                return false;
            }

            foreach (Category category in list)
            {
                if (!category.IsActive) category.CategoryName += "  (inactive)";
            }

            cmbCategory.DataSource = list;
            cmbCategory.DisplayMember = "CategoryName";
            cmbCategory.ValueMember = "CategoryId";
            return true;
        }

        /// <summary>Returns false when the dialog had to close instead.</summary>
        private bool LoadExisting()
        {
            Medicine medicine = _medicines.GetForEdit(_medicineId, UserSession.PharmacyId);

            if (medicine == null)
            {
                // The WHERE clause carried PharmacyId, so this only happens when
                // the row belongs to a different pharmacy.
                MessageBox.Show("That medicine does not belong to your pharmacy.",
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
                return false;
            }

            // The list includes this medicine's category even if it is inactive,
            // so SelectedValue below always finds it.
            if (!LoadCategories(medicine.CategoryId)) return false;

            lblTitle.Text = "Edit Medicine";
            txtName.Text = medicine.MedicineName;
            txtGeneric.Text = medicine.GenericName;
            txtManufacturer.Text = medicine.Manufacturer;
            txtStrength.Text = medicine.Strength;
            txtUnitPrice.Text = medicine.UnitPrice.ToString("0.00");
            txtStock.Text = medicine.Stock.ToString();
            txtMinStock.Text = medicine.MinStock.ToString();
            chkRequiresRx.Checked = medicine.RequiresRx;
            txtDescription.Text = medicine.Description;

            // Show the real date, even when it has already passed.
            _originalStock = medicine.Stock;
            _originalExpiry = medicine.ExpiryDate.Date;
            dtpExpiry.Value = medicine.ExpiryDate.Date;

            cmbCategory.SelectedValue = medicine.CategoryId;
            return true;
        }

        // ---------------------------------------------------------------------
        //  VALIDATION
        // ---------------------------------------------------------------------

        private void Field_Changed(object sender, EventArgs e)
        {
            if (_loading) return;

            if (sender == txtName || sender == txtStrength)
            {
                // The previous answer no longer applies; ask again once typing stops.
                _duplicateName = false;
                _duplicateTimer.Stop();
                _duplicateTimer.Start();
            }
            ValidateAll();
        }

        private void DuplicateTimer_Tick(object sender, EventArgs e)
        {
            _duplicateTimer.Stop();
            RunDuplicateCheck();
            ValidateAll();
        }

        /// <summary>
        /// One brand and strength may be listed once per pharmacy. A failure to
        /// reach the database here is not treated as a duplicate; Save checks
        /// again and the database refuses a real duplicate anyway.
        /// </summary>
        private void RunDuplicateCheck()
        {
            try
            {
                _duplicateName = !Validator.IsBlank(txtName.Text) &&
                    _medicines.NameExistsInPharmacy(UserSession.PharmacyId, txtName.Text, txtStrength.Text, _medicineId);
            }
            catch (Exception)
            {
                _duplicateName = false;
            }
        }

        /// <summary>
        /// A new medicine needs a future expiry date. When editing, the date is
        /// only required to be in the future if the owner changed it, so an
        /// expired medicine's price or description can still be corrected.
        /// </summary>
        private bool ExpiryMustBeFuture()
        {
            return _medicineId == 0 || dtpExpiry.Value.Date != _originalExpiry;
        }

        private bool ValidateAll()
        {
            if (_loading) return false;

            bool ok = true;

            ok &= Check(!Validator.IsBlank(txtName.Text), lblNameError, txtName,
                        "The brand name cannot be empty.");

            if (!Validator.IsBlank(txtName.Text) && _duplicateName)
            {
                UiTheme.ShowError(lblNameError, txtName,
                    "You already list " + (txtName.Text.Trim() + " " + txtStrength.Text.Trim()).Trim() +
                    ". Each brand and strength can be listed only once - edit the existing one instead.");
                ok = false;
            }

            ok &= Check(!Validator.IsBlank(txtGeneric.Text), lblGenericError, txtGeneric,
                        "The generic name is what makes the medicine searchable, so it is required.");

            ok &= Check(!Validator.IsBlank(txtManufacturer.Text), lblManufacturerError, txtManufacturer,
                        "Enter the manufacturer, for example Beximco, Square or Renata.");

            if (ExpiryMustBeFuture())
            {
                ok &= Check(Validator.IsFutureDate(dtpExpiry.Value), lblExpiryError, null,
                            "The expiry date must be in the future - expired stock is never offered to customers.");
            }
            else if (!Validator.IsFutureDate(dtpExpiry.Value))
            {
                // Not an error: the owner may save other changes. Just say what it means.
                UiTheme.ShowError(lblExpiryError, null,
                    "This medicine has expired, so customers cannot buy it. Change the date only for fresh stock.");
                lblExpiryError.ForeColor = UiTheme.Warning;
            }
            else
            {
                UiTheme.ClearError(lblExpiryError, null);
            }

            decimal price;
            ok &= Check(Validator.IsPositiveDecimal(txtUnitPrice.Text, out price), lblUnitPriceError, txtUnitPrice,
                        "The unit price must be a number greater than zero.");

            int stock;
            ok &= Check(Validator.IsNonNegativeInt(txtStock.Text, out stock), lblStockError, txtStock,
                        "Stock must be a whole number of zero or more.");

            int minStock;
            ok &= Check(Validator.IsNonNegativeInt(txtMinStock.Text, out minStock), lblMinStockError, txtMinStock,
                        "Minimum stock must be a whole number of zero or more.");

            ok &= cmbCategory.SelectedItem != null;

            // StylePrimary greys the button itself while it is disabled.
            btnSave.Enabled = ok;
            return ok;
        }

        private bool Check(bool rulePassed, Label errorLabel, Control field, string message)
        {
            if (rulePassed) UiTheme.ClearError(errorLabel, field);
            else UiTheme.ShowError(errorLabel, field, message);
            return rulePassed;
        }

        // ---------------------------------------------------------------------

        private void btnSave_Click(object sender, EventArgs e)
        {
            _duplicateTimer.Stop();
            RunDuplicateCheck();
            if (!ValidateAll()) return;

            try
            {
                Medicine medicine = new Medicine
                {
                    MedicineId = _medicineId,
                    CategoryId = Convert.ToInt32(cmbCategory.SelectedValue),
                    MedicineName = txtName.Text,
                    GenericName = txtGeneric.Text,
                    Manufacturer = txtManufacturer.Text,
                    Strength = txtStrength.Text,
                    UnitPrice = decimal.Parse(txtUnitPrice.Text),
                    Stock = int.Parse(txtStock.Text),
                    MinStock = int.Parse(txtMinStock.Text),
                    RequiresRx = chkRequiresRx.Checked,
                    ExpiryDate = dtpExpiry.Value.Date,
                    Description = txtDescription.Text
                    // ImagePath deliberately left empty: Update keeps the stored one.
                };

                if (_medicineId == 0)
                {
                    // PharmacyId comes from the session, never from the form, so
                    // an owner cannot create a medicine under someone else's shop.
                    _medicines.Insert(medicine, UserSession.PharmacyId);
                    SavedMessage = "Medicine added and immediately visible to customers.";
                }
                else
                {
                    string message;
                    if (!_medicines.Update(medicine, UserSession.PharmacyId, _originalStock, out message))
                    {
                        MessageBox.Show(message, "Not saved", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    SavedMessage = message;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The medicine could not be saved.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
