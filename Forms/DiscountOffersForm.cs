using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 14. The pharmacy owner's time limited percentage discounts.
    ///
    /// The percentage must be greater than 0 and no more than 70, and the end
    /// date cannot be earlier than the start date. Both rules are enforced here
    /// and again by the database. The discounted price itself is always
    /// computed inside the SQL query, so this screen, the customer's Offers
    /// screen, the cart and the invoice can never disagree.
    ///
    /// The editor always opens empty, with no offer selected: selecting a row
    /// loads that offer (and its medicine) for editing, and Clear really does
    /// clear, because the grid's automatic "select the first row" is suppressed.
    /// </summary>
    public partial class DiscountOffersForm : Form
    {
        private readonly OfferService _offers = new OfferService();
        private readonly MedicineService _medicines = new MedicineService();

        private readonly int _preselectMedicineId;
        // index 0 of the ComboBox is the prompt; index i is _medicineList[i - 1]
        private List<Medicine> _medicineList = new List<Medicine>();
        private int _selectedOfferId;
        private bool _loading = true;
        private bool _suppressSelection;

        public DiscountOffersForm() : this(0) { }

        /// <summary>preselectMedicineId chooses that medicine in an otherwise empty editor ("Create offer").</summary>
        public DiscountOffersForm(int preselectMedicineId)
        {
            InitializeComponent();
            _preselectMedicineId = preselectMedicineId;
        }

        private void DiscountOffersForm_Load(object sender, EventArgs e)
        {
            UiTheme.MakeResizable(this, Size);
            ApplyTheme();

            try
            {
                LoadMedicines();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your medicines could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            _loading = false;
            LoadGrid();
            ClearEditor();
            if (_preselectMedicineId > 0) SelectMedicine(_preselectMedicineId);
            ValidateAll();

            // The grid selects its first row when it is first shown; undo that.
            Shown += (s, args) => DeselectGrid();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Discount Offers");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            lblGridTitle.Font = UiTheme.FontHeading;
            lblGridTitle.ForeColor = UiTheme.TextDark;

            grpEditor.Font = UiTheme.FontHeading;
            grpEditor.ForeColor = UiTheme.Primary;
            grpEditor.BackColor = UiTheme.CardBack;
            foreach (Control child in grpEditor.Controls)
            {
                child.Font = UiTheme.FontBody;
                child.ForeColor = UiTheme.TextDark;
                if (child is Label label && label.Name.EndsWith("Error"))
                {
                    label.Font = UiTheme.FontSmall;
                    label.ForeColor = UiTheme.Danger;
                }
            }

            lblPreview.Font = UiTheme.FontSmall;
            lblPreview.ForeColor = UiTheme.Success;
            lblNote.Font = UiTheme.FontSmall;
            lblNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSuccess(btnCreate);
            UiTheme.StylePrimary(btnUpdate);
            UiTheme.StyleSecondary(btnClearEditor);
            UiTheme.StyleDanger(btnPause);
            UiTheme.StyleAccent(btnResume);
            UiTheme.StyleDanger(btnDelete);
            UiTheme.StyleGrid(dgvOffers);
            UiTheme.EnableEmptyMessage(dgvOffers, "You have no offers yet. Fill in the form on the right to create one.");
            dgvOffers.DataBindingComplete += dgvOffers_DataBindingComplete;
        }

        /// <summary>
        /// Delisted medicines are included and marked so an existing offer on
        /// one can still be opened and edited; a new offer on one is refused.
        /// </summary>
        private void LoadMedicines()
        {
            _medicineList = _medicines.GetSimpleListForPharmacy(UserSession.PharmacyId, true);

            cmbMedicine.Items.Clear();
            cmbMedicine.Items.Add("- choose one of your medicines -");
            foreach (Medicine medicine in _medicineList)
            {
                cmbMedicine.Items.Add((medicine.MedicineName + " " + medicine.Strength).Trim() +
                                      "  (Tk " + medicine.UnitPrice.ToString("N2") + ")" +
                                      (medicine.IsActive ? "" : "  (delisted)"));
            }
            cmbMedicine.SelectedIndex = 0;
        }

        private void SelectMedicine(int medicineId)
        {
            int index = _medicineList.FindIndex(m => m.MedicineId == medicineId);
            cmbMedicine.SelectedIndex = index < 0 ? 0 : index + 1;
        }

        private Medicine SelectedMedicine()
        {
            int index = cmbMedicine.SelectedIndex;
            if (index <= 0 || index > _medicineList.Count) return null;
            return _medicineList[index - 1];
        }

        private int SelectedMedicineId()
        {
            Medicine medicine = SelectedMedicine();
            return medicine == null ? 0 : medicine.MedicineId;
        }

        // ---------------------------------------------------------------------

        private void LoadGrid()
        {
            if (_loading) return;

            try
            {
                DataTable table = _offers.GetForPharmacy(UserSession.PharmacyId);
                dgvOffers.DataSource = table;

                if (dgvOffers.Columns.Count > 0)
                {
                    dgvOffers.Columns["OfferId"].HeaderText = "ID";
                    dgvOffers.Columns["MedicineId"].Visible = false;
                    dgvOffers.Columns["OfferTitle"].HeaderText = "Offer";
                    dgvOffers.Columns["MedicineName"].HeaderText = "Medicine";
                    dgvOffers.Columns["Strength"].HeaderText = "Strength";
                    dgvOffers.Columns["OriginalPrice"].HeaderText = "Was (Tk)";
                    dgvOffers.Columns["DiscountPercent"].HeaderText = "Off %";
                    dgvOffers.Columns["DiscountedPrice"].HeaderText = "Now (Tk)";
                    dgvOffers.Columns["StartDate"].HeaderText = "From";
                    dgvOffers.Columns["StartDate"].DefaultCellStyle.Format = "dd MMM yy";
                    dgvOffers.Columns["EndDate"].HeaderText = "Until";
                    dgvOffers.Columns["EndDate"].DefaultCellStyle.Format = "dd MMM yy";
                    dgvOffers.Columns["IsActive"].Visible = false;
                    dgvOffers.Columns["OfferState"].HeaderText = "State";

                    UiTheme.SizeColumn(dgvOffers, "OfferId", 28, 40);
                    UiTheme.SizeColumn(dgvOffers, "OfferTitle", 120, 120);
                    UiTheme.SizeColumn(dgvOffers, "Strength", 45, 60);
                    UiTheme.SizeColumn(dgvOffers, "DiscountPercent", 38, 50);
                    UiTheme.SizeColumn(dgvOffers, "StartDate", 55, 75);
                    UiTheme.SizeColumn(dgvOffers, "EndDate", 55, 75);
                    UiTheme.SizeColumn(dgvOffers, "OfferState", 52, 75);
                }

                DeselectGrid();

                lblGridTitle.Text = "My offers  (" + table.Rows.Count + ")   -   " +
                                    _offers.CountRunningForPharmacy(UserSession.PharmacyId) + " running today";

                UpdateGridButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your offers could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvOffers_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvOffers.Columns.Contains("OfferState")) return;

            foreach (DataGridViewRow row in dgvOffers.Rows)
            {
                switch (Convert.ToString(row.Cells["OfferState"].Value))
                {
                    case "Running": row.DefaultCellStyle.BackColor = UiTheme.DeliveredBack; break;
                    case "Scheduled": row.DefaultCellStyle.BackColor = UiTheme.PendingBack; break;
                    default: row.DefaultCellStyle.BackColor = UiTheme.InactiveBack; break;
                }
            }
        }

        /// <summary>Removes the grid's selection without loading anything into the editor.</summary>
        private void DeselectGrid()
        {
            _suppressSelection = true;
            try
            {
                dgvOffers.ClearSelection();
                dgvOffers.CurrentCell = null;
            }
            finally
            {
                _suppressSelection = false;
            }
            UpdateGridButtons();
        }

        private DataGridViewRow SelectedOfferRow()
        {
            if (dgvOffers.SelectedRows.Count == 0 || !dgvOffers.Columns.Contains("OfferId")) return null;
            DataGridViewRow row = dgvOffers.SelectedRows[0];
            return row.Cells["OfferId"].Value == null ? null : row;
        }

        private void dgvOffers_SelectionChanged(object sender, EventArgs e)
        {
            if (_suppressSelection) return;

            DataGridViewRow row = SelectedOfferRow();
            if (row == null)
            {
                UpdateGridButtons();
                return;
            }

            _loading = true;
            _selectedOfferId = Convert.ToInt32(row.Cells["OfferId"].Value);
            SelectMedicine(Convert.ToInt32(row.Cells["MedicineId"].Value));
            txtOfferTitle.Text = Convert.ToString(row.Cells["OfferTitle"].Value);
            txtPercent.Text = Convert.ToDecimal(row.Cells["DiscountPercent"].Value).ToString("0.##");
            dtpStart.Value = Convert.ToDateTime(row.Cells["StartDate"].Value);
            dtpEnd.Value = Convert.ToDateTime(row.Cells["EndDate"].Value);
            _loading = false;

            UpdateGridButtons();
            ValidateAll();
        }

        private void UpdateGridButtons()
        {
            DataGridViewRow row = SelectedOfferRow();
            bool hasRow = row != null && _selectedOfferId > 0;

            bool active = hasRow && row.Cells["IsActive"].Value != DBNull.Value &&
                          Convert.ToBoolean(row.Cells["IsActive"].Value);

            btnPause.Enabled = hasRow && active;
            btnResume.Enabled = hasRow && !active;
            btnDelete.Enabled = hasRow;
        }

        // ---------------------------------------------------------------------
        //  VALIDATION
        // ---------------------------------------------------------------------

        private void Field_Changed(object sender, EventArgs e)
        {
            if (_loading) return;
            ValidateAll();
        }

        private bool ValidateAll()
        {
            bool ok = true;

            // A medicine is always required, including when editing: selecting
            // an offer selects its medicine, so there is nothing to bypass.
            Medicine medicine = SelectedMedicine();
            ok &= Check(medicine != null, lblMedicineError, cmbMedicine,
                        "Choose which of your medicines the discount applies to.");

            bool medicineOnSale = medicine != null && medicine.IsActive;
            if (medicine != null && !medicine.IsActive && _selectedOfferId == 0)
            {
                UiTheme.ShowError(lblMedicineError, cmbMedicine,
                    "This medicine is delisted. Put it back on sale before creating an offer.");
            }

            ok &= Check(!Validator.IsBlank(txtOfferTitle.Text), lblOfferTitleError, txtOfferTitle,
                        "Give the offer a title, for example 'Fever Season Pack - 12% off'.");

            decimal percent;
            bool percentOk = Validator.IsDiscountPercent(txtPercent.Text, out percent);
            ok &= Check(percentOk, lblPercentError, txtPercent,
                        "The discount must be more than 0 and no more than 70 percent.");

            bool datesOk = dtpEnd.Value.Date >= dtpStart.Value.Date;
            bool endNotPast = dtpEnd.Value.Date >= DateTime.Today;
            if (!datesOk)
                UiTheme.ShowError(lblDateError, null, "The end date cannot be earlier than the start date.");
            else if (!endNotPast && _selectedOfferId == 0)
                UiTheme.ShowError(lblDateError, null, "A new offer cannot end in the past. Choose an end date from today on.");
            else
                UiTheme.ClearError(lblDateError, null);
            ok &= datesOk;

            // Live preview of what the customer will actually pay.
            if (percentOk && medicine != null)
            {
                decimal newPrice = decimal.Round(medicine.UnitPrice * (1 - percent / 100m), 2);
                lblPreview.Text = (medicine.MedicineName + " " + medicine.Strength).Trim() +
                                  ":  Tk " + medicine.UnitPrice.ToString("N2") +
                                  "  ->  Tk " + newPrice.ToString("N2") +
                                  "   (customer saves Tk " + (medicine.UnitPrice - newPrice).ToString("N2") + " per unit)";
            }
            else
            {
                lblPreview.Text = "";
            }

            btnCreate.Enabled = ok && medicineOnSale && endNotPast;
            btnUpdate.Enabled = ok && _selectedOfferId > 0;
            return ok;
        }

        private bool Check(bool rulePassed, Label errorLabel, Control field, string message)
        {
            if (rulePassed) UiTheme.ClearError(errorLabel, field);
            else UiTheme.ShowError(errorLabel, field, message);
            return rulePassed;
        }

        /// <summary>
        /// Two offers on the same medicine may overlap, but customers only ever
        /// get the higher discount. That surprises owners, so say it before
        /// saving and let the owner decide. Returns false when the owner backs out.
        /// </summary>
        private bool ConfirmOverlap(int medicineId, int ignoreOfferId, decimal percent)
        {
            decimal highest;
            int count = _offers.CountOverlapping(UserSession.PharmacyId, medicineId, dtpStart.Value, dtpEnd.Value,
                                                 ignoreOfferId, out highest);
            if (count == 0) return true;

            string applies = highest > percent
                ? "the other offer's " + highest.ToString("0.##") + "% applies on those days, not this " + percent.ToString("0.##") + "%"
                : highest == percent
                    ? "customers get " + percent.ToString("0.##") + "% either way"
                    : "this offer's " + percent.ToString("0.##") + "% applies on those days";

            DialogResult answer = MessageBox.Show(
                "This medicine already has " + count + " other active offer(s) running on some of these dates.\r\n\r\n" +
                "Customers never get two discounts at once - the higher discount applies, so " + applies + ".\r\n\r\n" +
                "Save this offer anyway?",
                "Overlapping offers", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            return answer == DialogResult.Yes;
        }

        // ---------------------------------------------------------------------

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (!ValidateAll()) return;

            int medicineId = SelectedMedicineId();
            if (medicineId == 0) return;

            decimal percent = decimal.Parse(txtPercent.Text);
            DateTime start = dtpStart.Value, end = dtpEnd.Value;

            try
            {
                if (!ConfirmOverlap(medicineId, 0, percent)) return;

                // The INSERT ... SELECT is scoped to this pharmacy's medicines, so
                // an offer can never be created on another pharmacy's medicine.
                if (_offers.Create(medicineId, UserSession.PharmacyId, txtOfferTitle.Text, percent, start, end))
                {
                    ClearEditor();
                    LoadGrid();
                    lblStatus.Text = "Offer created. It appears on the customer's Offers screen from " +
                                     start.ToString("dd MMM") + " to " + end.ToString("dd MMM yyyy") + ".";
                }
                else
                {
                    LoadGrid();
                    MessageBox.Show("The offer was not created because that medicine is no longer in your pharmacy's list.",
                        "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The offer could not be created.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedOfferId == 0 || !ValidateAll()) return;

            int offerId = _selectedOfferId;
            int medicineId = SelectedMedicineId();
            decimal percent = decimal.Parse(txtPercent.Text);

            try
            {
                if (!ConfirmOverlap(medicineId, offerId, percent)) return;

                bool saved = _offers.Update(offerId, UserSession.PharmacyId, medicineId, txtOfferTitle.Text,
                                            percent, dtpStart.Value, dtpEnd.Value);
                ClearEditor();
                LoadGrid();

                if (saved)
                {
                    lblStatus.Text = "Offer " + offerId + " saved.";
                }
                else
                {
                    lblStatus.Text = "Offer " + offerId + " was not saved.";
                    MessageBox.Show("The offer was not saved because it, or the medicine chosen for it, is no longer " +
                                    "in your pharmacy's list. The list has been refreshed.",
                        "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The offer could not be saved.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPause_Click(object sender, EventArgs e) => SetOfferActive(false);

        private void btnResume_Click(object sender, EventArgs e) => SetOfferActive(true);

        private void SetOfferActive(bool active)
        {
            if (_selectedOfferId == 0) return;
            int offerId = _selectedOfferId;

            try
            {
                bool changed = _offers.SetActive(offerId, UserSession.PharmacyId, active);
                ClearEditor();
                LoadGrid();

                if (!changed)
                    lblStatus.Text = "Offer " + offerId + " was not changed - it is no longer in your list.";
                else if (active)
                    lblStatus.Text = "Offer " + offerId + " is running again.";
                else
                    lblStatus.Text = "Offer " + offerId + " paused. It is kept, so it can be switched back on at any time.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The offer could not be updated.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedOfferId == 0) return;
            int offerId = _selectedOfferId;

            DialogResult answer = MessageBox.Show(
                "Delete this offer permanently?\r\n\r\n" +
                "Orders already placed keep the price they were sold at.",
                "Delete offer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes) return;

            try
            {
                bool deleted = _offers.Delete(offerId, UserSession.PharmacyId);
                ClearEditor();
                LoadGrid();
                lblStatus.Text = deleted
                    ? "Offer " + offerId + " deleted."
                    : "Offer " + offerId + " was not deleted - it is no longer in your list.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("The offer could not be deleted.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearEditor_Click(object sender, EventArgs e)
        {
            ClearEditor();
            lblStatus.Text = "";
        }

        private void ClearEditor()
        {
            _loading = true;
            _selectedOfferId = 0;
            cmbMedicine.SelectedIndex = cmbMedicine.Items.Count > 0 ? 0 : -1;
            txtOfferTitle.Clear();
            txtPercent.Clear();
            dtpStart.Value = DateTime.Today;
            dtpEnd.Value = DateTime.Today.AddDays(14);
            _loading = false;

            DeselectGrid();
            ValidateAll();
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
