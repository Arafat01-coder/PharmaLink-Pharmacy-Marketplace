using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Shown by LoginForm straight after a successful login whose account has
    /// Users.MustChangePassword = 1, i.e. it was signed in with a temporary
    /// password the Super Admin issued. No dashboard opens until this closes
    /// with OK; Cancel (or the window's close box) abandons the login.
    ///
    /// The temporary password is not typed again: LoginForm passes the one it
    /// just verified, and AuthService.CompleteRequiredPasswordChange checks it
    /// once more inside the guarded UPDATE, so a second temporary password
    /// issued meanwhile makes the change fail instead of being overwritten.
    ///
    /// The rules are the registration rules (Validator.IsStrongPassword), plus
    /// "not the temporary password", which would defeat the point.
    /// </summary>
    public partial class ChangePasswordRequiredForm : Form
    {
        private readonly AuthService _auth = new AuthService();
        private readonly int _userId;
        private readonly string _fullName = "";
        private string _temporaryPassword = "";

        public ChangePasswordRequiredForm()
        {
            InitializeComponent();
        }

        public ChangePasswordRequiredForm(int userId, string fullName, string temporaryPassword) : this()
        {
            _userId = userId;
            _fullName = fullName ?? "";
            _temporaryPassword = temporaryPassword ?? "";
        }

        private void ChangePasswordRequiredForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();

            if (_fullName.Length > 0)
                lblSubtitle.Text = "Hello " + _fullName + ". You signed in with a temporary password.";

            // Drop this form's copy of the temporary password however it closes.
            FormClosed += (s, args) => _temporaryPassword = "";

            ActiveControl = txtNewPassword;
            ValidateFields();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Choose a new password");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            lblIntro.Font = UiTheme.FontSmall;
            lblIntro.ForeColor = UiTheme.TextMuted;
            lblNew.Font = UiTheme.FontBody;
            lblConfirm.Font = UiTheme.FontBody;
            lblRules.Font = UiTheme.FontSmall;
            lblRules.ForeColor = UiTheme.TextMuted;

            foreach (Label error in new[] { lblNewError, lblConfirmError, lblFormError })
            {
                error.Font = UiTheme.FontSmall;
                error.ForeColor = UiTheme.Danger;
            }

            UiTheme.StylePrimary(btnSavePassword);
            btnSavePassword.Font = UiTheme.FontButtonStrong;
            UiTheme.StyleSecondary(btnCancelChange);

            AcceptButton = btnSavePassword;
            CancelButton = btnCancelChange;
        }

        // ---------------------------------------------------------------------
        //  VALIDATION
        //  As on My Account: a field shows its error once something is typed
        //  in it, and the Save button stays disabled until both are right.
        // ---------------------------------------------------------------------

        private void Field_Changed(object sender, EventArgs e)
        {
            lblFormError.Visible = false;
            ValidateFields();
        }

        private bool ValidateFields()
        {
            bool ok = true;
            string newPassword = txtNewPassword.Text;

            if (newPassword.Length == 0)
            {
                UiTheme.ClearError(lblNewError, txtNewPassword);
                ok = false;
            }
            else if (!Validator.IsStrongPassword(newPassword))
            {
                UiTheme.ShowError(lblNewError, txtNewPassword,
                    "Use at least 8 characters, with at least one letter and one digit.");
                ok = false;
            }
            else if (newPassword == _temporaryPassword)
            {
                UiTheme.ShowError(lblNewError, txtNewPassword,
                    "Choose a password that is different from the temporary one.");
                ok = false;
            }
            else
            {
                UiTheme.ClearError(lblNewError, txtNewPassword);
            }

            if (txtConfirmPassword.Text.Length == 0)
            {
                UiTheme.ClearError(lblConfirmError, txtConfirmPassword);
                ok = false;
            }
            else if (txtConfirmPassword.Text != newPassword)
            {
                UiTheme.ShowError(lblConfirmError, txtConfirmPassword, "The two passwords do not match.");
                ok = false;
            }
            else
            {
                UiTheme.ClearError(lblConfirmError, txtConfirmPassword);
            }

            btnSavePassword.Enabled = ok;
            return ok;
        }

        private void chkShowPasswords_CheckedChanged(object sender, EventArgs e)
        {
            char mask = chkShowPasswords.Checked ? '\0' : '*';
            txtNewPassword.PasswordChar = mask;
            txtConfirmPassword.PasswordChar = mask;
        }

        // ---------------------------------------------------------------------
        //  SAVE / CANCEL
        // ---------------------------------------------------------------------

        private void btnSavePassword_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                if (_auth.CompleteRequiredPasswordChange(_userId, _temporaryPassword, txtNewPassword.Text))
                {
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                // The rules were already checked above, so false means the
                // temporary password stopped matching (a newer one was issued)
                // or the account is gone.
                UiTheme.ShowError(lblFormError, null,
                    "Your password could not be changed because the temporary password is no longer valid. " +
                    "Cancel, then sign in again with the latest password the administrator gave you.");
            }
            catch (Exception ex)
            {
                UiTheme.ShowError(lblFormError, null,
                    "Your password could not be changed. " + DbHelper.Describe(ex).Replace("\r\n\r\n", " "));
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCancelChange_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
