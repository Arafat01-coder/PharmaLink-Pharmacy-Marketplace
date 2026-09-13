using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Registration for both kinds of account (requirements 10 and 19).
    ///
    /// The "Register as" dropdown decides which of the two paths runs. A
    /// customer is created Active and can order immediately; a pharmacy owner
    /// is created Pending together with a Pending Pharmacies row, and neither
    /// becomes usable until the Super Admin has checked the drug licence.
    /// </summary>
    public partial class SignUpForm : Form
    {
        private readonly AuthService _auth = new AuthService();
        private readonly PharmacyService _pharmacies = new PharmacyService();

        /// <summary>Fields the user has typed into; only these show red errors before a submit.</summary>
        private readonly HashSet<Control> _touched = new HashSet<Control>();

        /// <summary>Set by the first click on Create, after which every rule shows its error.</summary>
        private bool _submitAttempted;

        /// <summary>Read by LoginForm so the new user's email is pre-filled after registration.</summary>
        public string RegisteredEmail { get; private set; } = "";

        private bool IsPharmacyOwner => cmbRegisterAs.SelectedIndex == 1;

        public SignUpForm()
        {
            InitializeComponent();
        }

        private void SignUpForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();

            cmbRegisterAs.Items.Add("Customer  (patient buying medicine)");
            cmbRegisterAs.Items.Add("Pharmacy Owner  (shop selling medicine)");
            cmbRegisterAs.SelectedIndex = 0;

            LoadAreaSuggestions();
            ValidateAll();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Create account");
            UiTheme.StyleHeader(panelHeader, lblHeader, lblHeaderSub);

            grpPersonal.Font = UiTheme.FontHeading;
            grpPersonal.ForeColor = UiTheme.Primary;
            grpPersonal.BackColor = UiTheme.CardBack;

            grpPharmacy.Font = UiTheme.FontHeading;
            grpPharmacy.ForeColor = UiTheme.Primary;
            grpPharmacy.BackColor = UiTheme.CardBack;

            foreach (Control group in new Control[] { grpPersonal, grpPharmacy })
            {
                foreach (Control child in group.Controls)
                {
                    child.Font = UiTheme.FontBody;
                    child.ForeColor = UiTheme.TextDark;
                    if (child is Label label && label.Name.EndsWith("Error"))
                    {
                        label.Font = UiTheme.FontSmall;
                        label.ForeColor = UiTheme.Danger;
                    }
                }
            }

            lblPendingNote.Font = UiTheme.FontSmall;
            lblPendingNote.ForeColor = UiTheme.Warning;

            lblFormMessage.Font = UiTheme.FontSmall;
            lblFormMessage.ForeColor = UiTheme.Danger;

            UiTheme.StylePrimary(btnCreate);
            btnCreate.Font = UiTheme.FontButtonStrong;
            UiTheme.StyleSecondary(btnBack);
        }

        /// <summary>
        /// Suggestions for the Area box: the areas pharmacies already use, plus
        /// a plain default list. The list is only a convenience - the owner can
        /// type any area - so if the database cannot be read the defaults alone
        /// are offered instead of an error.
        /// </summary>
        private void LoadAreaSuggestions()
        {
            cmbArea.Items.Clear();
            try
            {
                foreach (string area in _pharmacies.GetAreas(false))
                    cmbArea.Items.Add(area);
            }
            catch (Exception ex)
            {
                cmbArea.Items.Clear();
                System.Diagnostics.Debug.WriteLine("Area suggestions unavailable, using defaults: " + DbHelper.Describe(ex));
            }

            foreach (string area in new[] { "Mitford", "Dhanmondi", "Mirpur", "Uttara", "Banani", "Gulshan", "Mohammadpur" })
            {
                if (!cmbArea.Items.Contains(area)) cmbArea.Items.Add(area);
            }
        }

        private void cmbRegisterAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            grpPharmacy.Enabled = IsPharmacyOwner;
            grpPharmacy.ForeColor = IsPharmacyOwner ? UiTheme.Primary : UiTheme.TextMuted;
            lblAddress.Text = IsPharmacyOwner ? "Your personal address" : "Delivery address";
            btnCreate.Text = IsPharmacyOwner ? "Submit for approval" : "Create account";
            lblFormMessage.Visible = false;
            ValidateAll();
        }

        // ---------------------------------------------------------------------
        //  VALIDATION
        //  Each failure shows a red label directly under the offending field -
        //  but only once the user has typed into that field, or has pressed the
        //  Create button. A fresh form, or one whose role was just switched,
        //  therefore starts clean instead of covered in red. Pressing Create
        //  with a problem left reveals every error and saves nothing.
        //  The email pattern and duplicate email / phone / licence are also
        //  enforced by the database; the phone formats and the password rule
        //  are enforced only here.
        // ---------------------------------------------------------------------

        private void Field_Changed(object sender, EventArgs e)
        {
            if (sender is Control field) _touched.Add(field);
            lblFormMessage.Visible = false;
            ValidateAll();
        }

        private bool ValidateAll()
        {
            bool ok = true;

            ok &= Check(!Validator.IsBlank(txtFullName.Text), lblFullNameError, txtFullName,
                        "Please enter your full name.");

            ok &= Check(Validator.IsEmail(txtEmail.Text), lblEmailError, txtEmail,
                        "Enter a valid email address, for example name@example.com.");

            ok &= Check(Validator.IsMobile(txtPhone.Text), lblPhoneError, txtPhone,
                        "A mobile number is 11 digits and starts with 01.");

            ok &= Check(!Validator.IsBlank(txtAddress.Text), lblAddressError, txtAddress,
                        "Please enter an address.");

            ok &= Check(Validator.IsStrongPassword(txtPassword.Text), lblPasswordError, txtPassword,
                        "Use at least 8 characters, with at least one letter and one digit.");

            ok &= Check(!Validator.IsBlank(txtConfirm.Text) && txtConfirm.Text == txtPassword.Text,
                        lblConfirmError, txtConfirm, "The two passwords do not match.");

            if (IsPharmacyOwner)
            {
                ok &= Check(!Validator.IsBlank(txtShopName.Text), lblShopNameError, txtShopName,
                            "Enter the trading name of your pharmacy.");
                ok &= Check(Validator.IsLicenseNo(txtLicenseNo.Text), lblLicenseError, txtLicenseNo,
                            "Enter your DGDA licence number, for example DGDA-DH-10021.");
                ok &= Check(!Validator.IsBlank(cmbArea.Text), lblAreaError, cmbArea,
                            "Choose or type the area your shop is in.");
                ok &= Check(!Validator.IsBlank(txtShopAddress.Text), lblShopAddressError, txtShopAddress,
                            "Enter the full postal address of the shop.");
                ok &= Check(Validator.IsContactPhone(txtShopPhone.Text), lblShopPhoneError, txtShopPhone,
                            "Enter 11 digits starting with 01 or 02, for example 02955000021.");
            }
            else
            {
                foreach (Control child in grpPharmacy.Controls)
                {
                    if (child is Label label && label.Name.EndsWith("Error")) label.Visible = false;
                    if (child is TextBox || child is ComboBox) child.BackColor = Color.White;
                }
            }

            return ok;
        }

        /// <summary>
        /// Shows or clears one field's error label and returns the rule's result.
        /// A failing rule stays quiet until the field has been touched or the
        /// form has been submitted.
        /// </summary>
        private bool Check(bool rulePassed, Label errorLabel, Control field, string message)
        {
            if (rulePassed || (!_submitAttempted && !_touched.Contains(field)))
                UiTheme.ClearError(errorLabel, field);
            else
                UiTheme.ShowError(errorLabel, field, message);
            return rulePassed;
        }

        /// <summary>Puts the cursor in the first field that is showing an error.</summary>
        private void FocusFirstError()
        {
            Control[] order =
            {
                txtFullName, txtEmail, txtPhone, txtAddress, txtPassword, txtConfirm,
                txtShopName, txtLicenseNo, cmbArea, txtShopAddress, txtShopPhone
            };
            Label[] errors =
            {
                lblFullNameError, lblEmailError, lblPhoneError, lblAddressError, lblPasswordError, lblConfirmError,
                lblShopNameError, lblLicenseError, lblAreaError, lblShopAddressError, lblShopPhoneError
            };

            for (int i = 0; i < order.Length; i++)
            {
                if (errors[i].Visible && order[i].Enabled)
                {
                    order[i].Focus();
                    return;
                }
            }
        }

        private void ShowFormMessage(string message)
        {
            UiTheme.ShowError(lblFormMessage, null, message.Replace("\r\n\r\n", " "));
        }

        /// <summary>
        /// Two people can register the same email, phone or licence at the same
        /// moment: both pass the "already exists" check, and the second INSERT
        /// then hits a UNIQUE constraint. That error names the constraint, so
        /// the message can still go under the right field.
        /// </summary>
        private bool ShowDuplicateError(Exception ex)
        {
            SqlException sql = ex as SqlException ?? ex.InnerException as SqlException;
            if (sql == null || (sql.Number != 2627 && sql.Number != 2601)) return false;

            if (sql.Message.Contains("UQ_Users_Email"))
                UiTheme.ShowError(lblEmailError, txtEmail, "An account with this email already exists.");
            else if (sql.Message.Contains("UQ_Users_Phone"))
                UiTheme.ShowError(lblPhoneError, txtPhone, "This mobile number is already registered.");
            else if (sql.Message.Contains("UQ_Pharmacies_License"))
                UiTheme.ShowError(lblLicenseError, txtLicenseNo, "This licence number is already registered to another pharmacy.");
            else
                return false;

            ShowFormMessage("Someone registered the same details a moment ago. Change the field marked in red and try again.");
            return true;
        }

        // ---------------------------------------------------------------------
        //  SAVE
        // ---------------------------------------------------------------------

        private void btnCreate_Click(object sender, EventArgs e)
        {
            _submitAttempted = true;
            if (!ValidateAll())
            {
                ShowFormMessage("Please correct the fields marked in red.");
                FocusFirstError();
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                // The UNIQUE constraints on Email and Phone are what actually stop
                // a duplicate account; checking first is only so the user gets a
                // friendly message instead of a database exception.
                if (_auth.EmailExists(txtEmail.Text))
                {
                    UiTheme.ShowError(lblEmailError, txtEmail, "An account with this email already exists.");
                    txtEmail.Focus();
                    return;
                }

                if (_auth.PhoneExists(txtPhone.Text))
                {
                    UiTheme.ShowError(lblPhoneError, txtPhone, "This mobile number is already registered.");
                    txtPhone.Focus();
                    return;
                }

                User user = new User
                {
                    FullName = txtFullName.Text,
                    Email = txtEmail.Text,
                    Phone = txtPhone.Text,
                    Address = txtAddress.Text
                };

                if (IsPharmacyOwner)
                {
                    if (_auth.LicenseExists(txtLicenseNo.Text))
                    {
                        UiTheme.ShowError(lblLicenseError, txtLicenseNo,
                            "This licence number is already registered to another pharmacy.");
                        txtLicenseNo.Focus();
                        return;
                    }

                    Pharmacy pharmacy = new Pharmacy
                    {
                        PharmacyName = txtShopName.Text,
                        LicenseNo = txtLicenseNo.Text,
                        Area = cmbArea.Text,
                        Address = txtShopAddress.Text,
                        ContactPhone = txtShopPhone.Text
                    };

                    _auth.RegisterPharmacyOwner(user, pharmacy, txtPassword.Text);

                    MessageBox.Show(
                        "Your pharmacy registration has been submitted.\r\n\r\n" +
                        "Both your account and " + pharmacy.PharmacyName.Trim() + " are held at status Pending. " +
                        "The Super Admin will check licence " + pharmacy.LicenseNo.Trim() + " and approve the shop, " +
                        "after which you will be able to log in and list your medicines.",
                        "Submitted for approval", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _auth.RegisterCustomer(user, txtPassword.Text);
                    RegisteredEmail = user.Email.Trim();

                    MessageBox.Show(
                        "Welcome to PharmaLink, " + user.FullName.Trim() + ".\r\n\r\n" +
                        "Your account is active. You can sign in and start ordering right away.",
                        "Account created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                if (!ShowDuplicateError(ex))
                    ShowFormMessage("The account could not be created. " + DbHelper.Describe(ex));
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
