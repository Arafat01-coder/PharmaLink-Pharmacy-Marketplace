using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirements 17, 27 and 30. The same form serves all three roles,
    /// because "edit my own details and change my own password" is the same job
    /// whoever is doing it.
    ///
    /// Email is read only: it is the login identifier. The password change
    /// (AuthService.ChangePassword) reads the stored salt and hash, verifies
    /// the current password against them in memory, and only then writes the
    /// new hash. The UPDATE also requires the stored hash to be unchanged, so
    /// a password changed elsewhere at the same moment is not overwritten; in
    /// either case ChangePassword returns false and nothing is changed.
    ///
    /// Messages on this screen are for the account holder, so they talk about
    /// fields ("mobile number") and never about tables or constraint names.
    /// Text boxes carry MaxLength values that match the column sizes.
    /// </summary>
    public partial class MyProfileForm : Form
    {
        private readonly AuthService _auth = new AuthService();
        private bool _loading = true;

        private const string PhoneTakenMessage =
            "This mobile number is already used by another account. Please enter a different mobile number.";

        public MyProfileForm()
        {
            InitializeComponent();
        }

        private void MyProfileForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            LoadProfile();
            _loading = false;
            ValidateProfile();
            ValidatePassword();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "My Account");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            foreach (GroupBox group in new[] { grpProfile, grpPassword })
            {
                group.Font = UiTheme.FontHeading;
                group.ForeColor = UiTheme.Primary;
                group.BackColor = UiTheme.CardBack;

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

            txtEmail.BackColor = UiTheme.ReadOnlyBack;
            lblEmailNote.Font = UiTheme.FontSmall;
            lblEmailNote.ForeColor = UiTheme.TextMuted;
            lblHashNote.Font = UiTheme.FontSmall;
            lblHashNote.ForeColor = UiTheme.TextMuted;
            lblMemberSince.Font = UiTheme.FontSmall;
            lblMemberSince.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StylePrimary(btnSaveProfile);
            UiTheme.StyleAccent(btnChangePassword);
            btnSaveProfile.Font = UiTheme.FontButtonStrong;
            btnChangePassword.Font = UiTheme.FontButtonStrong;
        }

        private void LoadProfile()
        {
            User user;
            try
            {
                user = _auth.GetUser(UserSession.UserId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your details could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (user == null) return;

            txtFullName.Text = user.FullName;
            txtEmail.Text = user.Email;
            txtPhone.Text = user.Phone;
            txtAddress.Text = user.Address;

            lblMemberSince.Text = "Signed in as " + user.UserType + "   |   Member since " +
                                  user.CreatedAt.ToString("dd MMM yyyy");

            lblAddress.Text = user.UserType == "Customer"
                ? "Delivery address (pre-filled at checkout)"
                : "Your personal address";
        }

        // ---------------------------------------------------------------------
        //  PROFILE
        // ---------------------------------------------------------------------

        private void Profile_Changed(object sender, EventArgs e)
        {
            if (_loading) return;
            ValidateProfile();
        }

        private bool ValidateProfile()
        {
            bool ok = true;

            ok &= Check(!Validator.IsBlank(txtFullName.Text), lblFullNameError, txtFullName,
                        "Your name cannot be empty.");

            ok &= Check(Validator.IsMobile(txtPhone.Text), lblPhoneError, txtPhone,
                        "A mobile number is 11 digits and starts with 01.");

            ok &= Check(!Validator.IsBlank(txtAddress.Text), lblAddressError, txtAddress,
                        "Please enter an address.");

            btnSaveProfile.Enabled = ok;
            return ok;
        }

        private void btnSaveProfile_Click(object sender, EventArgs e)
        {
            if (!ValidateProfile()) return;

            try
            {
                // The database keeps mobile numbers unique, so this check only
                // exists to point at the right field before the save is tried.
                if (PhoneTakenBySomeoneElse(txtPhone.Text.Trim()))
                {
                    UiTheme.ShowError(lblPhoneError, txtPhone, PhoneTakenMessage);
                    return;
                }

                if (_auth.UpdateProfile(UserSession.UserId, txtFullName.Text, txtPhone.Text, txtAddress.Text))
                {
                    UserSession.FullName = txtFullName.Text.Trim();
                    lblStatus.Text = "Your details have been saved.";
                    MessageBox.Show("Your details have been saved.", "PharmaLink",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.Text = "Nothing was saved because your account could not be found. Please sign in again.";
                }
            }
            catch (Exception ex)
            {
                // Someone may have taken the number between the check and the
                // save; if so, say so against the mobile number field.
                bool phoneClash = false;
                try { phoneClash = PhoneTakenBySomeoneElse(txtPhone.Text.Trim()); } catch { }

                if (phoneClash)
                    UiTheme.ShowError(lblPhoneError, txtPhone, PhoneTakenMessage);
                else
                    MessageBox.Show("Your details could not be saved.\r\n\r\n" + DbHelper.Describe(ex),
                        "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool PhoneTakenBySomeoneElse(string phone)
        {
            User current = _auth.GetUser(UserSession.UserId);
            if (current != null && current.Phone == phone) return false;
            return _auth.PhoneExists(phone);
        }

        // ---------------------------------------------------------------------
        //  PASSWORD
        // ---------------------------------------------------------------------

        private void Password_Changed(object sender, EventArgs e)
        {
            if (_loading) return;
            ValidatePassword();
        }

        private bool ValidatePassword()
        {
            bool ok = true;

            if (Validator.IsBlank(txtCurrent.Text))
            {
                UiTheme.ClearError(lblCurrentError, txtCurrent);
                ok = false;
            }
            else
            {
                UiTheme.ClearError(lblCurrentError, txtCurrent);
            }

            if (Validator.IsBlank(txtNew.Text))
            {
                UiTheme.ClearError(lblNewError, txtNew);
                ok = false;
            }
            else
            {
                ok &= Check(Validator.IsStrongPassword(txtNew.Text), lblNewError, txtNew,
                            "The new password needs at least 8 characters, with at least one letter and one digit.");
            }

            if (Validator.IsBlank(txtConfirm.Text))
            {
                UiTheme.ClearError(lblConfirmError, txtConfirm);
                ok = false;
            }
            else
            {
                ok &= Check(txtConfirm.Text == txtNew.Text, lblConfirmError, txtConfirm,
                            "The two new passwords do not match.");
            }

            if (!Validator.IsBlank(txtNew.Text) && txtNew.Text == txtCurrent.Text)
            {
                UiTheme.ShowError(lblNewError, txtNew, "The new password must be different from the current one.");
                ok = false;
            }

            btnChangePassword.Enabled = ok;
            return ok;
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            if (!ValidatePassword()) return;

            try
            {
                if (_auth.ChangePassword(UserSession.UserId, txtCurrent.Text, txtNew.Text))
                {
                    txtCurrent.Clear();
                    txtNew.Clear();
                    txtConfirm.Clear();
                    ValidatePassword();

                    lblStatus.Text = "Your password has been updated.";
                    MessageBox.Show(
                        "Your password has been updated.\r\n\r\nUse the new password the next time you sign in.",
                        "Password changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // ChangePassword returns false when the current password does
                    // not verify, or when the password was changed elsewhere at
                    // the same moment; either way nothing was written.
                    UiTheme.ShowError(lblCurrentError, txtCurrent,
                        "That is not your current password, so nothing was changed.");
                    txtCurrent.SelectAll();
                    txtCurrent.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The password could not be changed.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chkShowPasswords_CheckedChanged(object sender, EventArgs e)
        {
            char mask = chkShowPasswords.Checked ? '\0' : '*';
            txtCurrent.PasswordChar = mask;
            txtNew.PasswordChar = mask;
            txtConfirm.PasswordChar = mask;
        }

        // ---------------------------------------------------------------------

        private bool Check(bool rulePassed, Label errorLabel, Control field, string message)
        {
            if (rulePassed) UiTheme.ClearError(errorLabel, field);
            else UiTheme.ShowError(errorLabel, field, message);
            return rulePassed;
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
