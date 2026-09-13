using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// "Forgot password?" from the login screen.
    ///
    /// PharmaLink has no email or SMS service, so this cannot send a reset link.
    /// It records a request that the Super Admin sees on Manage Users; the Super
    /// Admin phones the registered mobile number and reads out a temporary
    /// password, and the next login with it must choose a new password.
    ///
    /// Asking for the email AND the registered mobile number means a stranger
    /// who only knows someone's email cannot fill the Super Admin's queue with
    /// requests for them. Whatever is typed, the reply is the same sentence
    /// (AuthService.PasswordResetRequestedMessage), so this form cannot be used
    /// to discover which email and mobile pairs belong to real accounts.
    ///
    /// A field's error appears only once the user has left that field or
    /// pressed Send request, so the form does not open covered in red.
    /// </summary>
    public partial class ForgotPasswordForm : Form
    {
        private readonly AuthService _auth = new AuthService();
        private readonly string _initialEmail = "";

        private bool _emailTouched;
        private bool _mobileTouched;
        private bool _submitted;

        public ForgotPasswordForm()
        {
            InitializeComponent();
        }

        /// <summary>Pre-fills the email the user had already typed on the login screen.</summary>
        public ForgotPasswordForm(string email) : this()
        {
            _initialEmail = email ?? "";
        }

        private void ForgotPasswordForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();

            if (Validator.IsEmail(_initialEmail))
            {
                txtResetEmail.Text = _initialEmail.Trim();
                ActiveControl = txtResetMobile;
            }
            else
            {
                ActiveControl = txtResetEmail;
            }
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Forgot password");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            lblIntro.Font = UiTheme.FontSmall;
            lblIntro.ForeColor = UiTheme.TextMuted;
            lblEmail.Font = UiTheme.FontBody;
            lblMobile.Font = UiTheme.FontBody;
            lblEmailError.Font = UiTheme.FontSmall;
            lblEmailError.ForeColor = UiTheme.Danger;
            lblMobileError.Font = UiTheme.FontSmall;
            lblMobileError.ForeColor = UiTheme.Danger;
            lblResult.Font = UiTheme.FontBody;

            UiTheme.StylePrimary(btnSubmitReset);
            btnSubmitReset.Font = UiTheme.FontButtonStrong;
            UiTheme.StyleSecondary(btnCancelReset);

            AcceptButton = btnSubmitReset;
            CancelButton = btnCancelReset;
        }

        // ---------------------------------------------------------------------
        //  VALIDATION
        // ---------------------------------------------------------------------

        private void txtResetEmail_Leave(object sender, EventArgs e)
        {
            if (_submitted) return;
            // Tabbing past an empty box is not "touching" it; typing something is.
            if (txtResetEmail.Text.Length > 0) _emailTouched = true;
            ValidateFields();
        }

        private void txtResetMobile_Leave(object sender, EventArgs e)
        {
            if (_submitted) return;
            if (txtResetMobile.Text.Length > 0) _mobileTouched = true;
            ValidateFields();
        }

        private void Field_Changed(object sender, EventArgs e)
        {
            if (_submitted) return;
            ValidateFields();
        }

        /// <summary>
        /// Checks both fields; an error is only shown for a field that has been
        /// touched. Returns whether both are valid, touched or not.
        /// </summary>
        private bool ValidateFields()
        {
            bool emailOk = Validator.IsEmail(txtResetEmail.Text);
            bool mobileOk = Validator.IsMobile(txtResetMobile.Text);

            if (_emailTouched && !emailOk)
                UiTheme.ShowError(lblEmailError, txtResetEmail, Validator.IsBlank(txtResetEmail.Text)
                    ? "Enter the email address you sign in with."
                    : "That does not look like a valid email address.");
            else
                UiTheme.ClearError(lblEmailError, txtResetEmail);

            if (_mobileTouched && !mobileOk)
                UiTheme.ShowError(lblMobileError, txtResetMobile, Validator.IsBlank(txtResetMobile.Text)
                    ? "Enter the mobile number registered on your account."
                    : "A mobile number is 11 digits and starts with 01.");
            else
                UiTheme.ClearError(lblMobileError, txtResetMobile);

            return emailOk && mobileOk;
        }

        // ---------------------------------------------------------------------
        //  SUBMIT
        // ---------------------------------------------------------------------

        private void btnSubmitReset_Click(object sender, EventArgs e)
        {
            _emailTouched = true;
            _mobileTouched = true;
            if (!ValidateFields())
            {
                if (!Validator.IsEmail(txtResetEmail.Text)) txtResetEmail.Focus();
                else txtResetMobile.Focus();
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                string message = _auth.RequestPasswordReset(txtResetEmail.Text.Trim(), txtResetMobile.Text.Trim());
                ShowSent(message);
            }
            catch (Exception ex)
            {
                UiTheme.ShowError(lblResult, null,
                    "Your request could not be sent. " + DbHelper.Describe(ex).Replace("\r\n\r\n", " "));
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// The same confirmation for every request. The fields are locked so the
        /// form cannot be used to try pair after pair, and Close is the only
        /// button left.
        /// </summary>
        private void ShowSent(string message)
        {
            _submitted = true;

            lblResult.Text = message;
            lblResult.ForeColor = UiTheme.Success;
            lblResult.Visible = true;

            foreach (TextBox box in new[] { txtResetEmail, txtResetMobile })
            {
                box.ReadOnly = true;
                box.BackColor = UiTheme.ReadOnlyBack;
            }

            btnSubmitReset.Enabled = false;
            btnCancelReset.Text = "Close";
            AcceptButton = btnCancelReset;
            btnCancelReset.Focus();
        }

        private void btnCancelReset_Click(object sender, EventArgs e)
        {
            DialogResult = _submitted ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }
    }
}
