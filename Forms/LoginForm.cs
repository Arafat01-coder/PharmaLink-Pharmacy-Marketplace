using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// The single entry point of the application.
    ///
    /// There is no separate administrator login. All three roles type their
    /// email and password into this one form; the login query returns the
    /// UserType and that single value decides which of the three dashboards
    /// opens. No arrow in the navigation diagram ever crosses from one role
    /// branch into another.
    ///
    /// Two gates sit between a correct password and the dashboard:
    ///  - an account signed in with a temporary password the Super Admin
    ///    issued (User.MustChangePassword) must choose a new one first, in
    ///    ChangePasswordRequiredForm; cancelling it abandons the login;
    ///  - "Remember my email" is saved or forgotten (LoginPreferences) only once
    ///    the dashboard is actually about to open.
    /// </summary>
    public partial class LoginForm : Form
    {
        private readonly AuthService _auth = new AuthService();

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            UserSession.Clear();

            // A remembered email is pre-filled and the cursor waits in the
            // password box, so a returning user only types the password.
            string rememberedEmail = LoginPreferences.Load();
            if (rememberedEmail.Length > 0)
            {
                txtEmail.Text = rememberedEmail;
                chkRememberMe.Checked = true;
                ActiveControl = txtPassword;
            }
            else
            {
                ActiveControl = txtEmail;
            }

            ValidateFields();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Sign in");

            // ---- the branding panel on the left ----
            panelBrand.BackColor = UiTheme.Primary;

            lblBrandMark.Font = UiTheme.FontLoginMark;
            lblBrandMark.ForeColor = UiTheme.BrandMint;

            lblBrandName.Font = UiTheme.FontLoginBrand;
            lblBrandName.ForeColor = Color.White;

            lblTagline.Font = UiTheme.FontTagline;
            lblTagline.ForeColor = UiTheme.BrandMintSoft;

            lblBrandBlurb.Font = UiTheme.FontBody;
            lblBrandBlurb.ForeColor = UiTheme.BrandMintPale;

            ShowDemoAccounts();

            // ---- the login card on the right ----
            panelCard.BackColor = UiTheme.CardBack;
            panelCard.BorderStyle = BorderStyle.FixedSingle;

            lblTitle.Font = UiTheme.FontTitle;
            lblTitle.ForeColor = UiTheme.TextDark;
            lblSubtitle.Font = UiTheme.FontSmall;
            lblSubtitle.ForeColor = UiTheme.TextMuted;

            lblEmail.Font = UiTheme.FontBody;
            lblPassword.Font = UiTheme.FontBody;
            lblEmailError.Font = UiTheme.FontSmall;
            lblEmailError.ForeColor = UiTheme.Danger;
            lblPasswordError.Font = UiTheme.FontSmall;
            lblPasswordError.ForeColor = UiTheme.Danger;
            lblFormError.Font = UiTheme.FontSmall;
            lblFormError.ForeColor = UiTheme.Danger;

            lblNoAccount.Font = UiTheme.FontSmall;
            lblNoAccount.ForeColor = UiTheme.TextMuted;

            StyleAsLink(btnForgotPassword);

            UiTheme.StylePrimary(btnLogin);
            btnLogin.Font = UiTheme.FontButtonLarge;
            UiTheme.StyleSecondary(btnGoSignUp);

            AcceptButton = btnLogin;
        }

        /// <summary>
        /// "Forgot password?" is a secondary action, so it reads as a link in the
        /// accent colour rather than competing with the Log in button. It stays
        /// a Button (keyboard focus, Enter/Space, UI Automation) and is only
        /// dressed with theme colours: flat, no border, and a hover colour equal
        /// to the card so it never flashes a grey box.
        /// </summary>
        private static void StyleAsLink(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = UiTheme.CardBack;
            button.FlatAppearance.MouseDownBackColor = UiTheme.CardBack;
            button.BackColor = UiTheme.CardBack;
            button.ForeColor = UiTheme.Accent;
            button.Font = UiTheme.FontSmall;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        /// <summary>
        /// The demonstration accounts and their passwords are a convenience for
        /// the project demo. They are compiled only into DEBUG builds, so a
        /// Release build never prints working credentials on its login screen.
        /// The label auto-sizes, so no line is ever clipped.
        /// </summary>
        private void ShowDemoAccounts()
        {
#if DEBUG
            lblDemoTitle.Font = UiTheme.FontCaption;
            lblDemoTitle.ForeColor = UiTheme.BrandMint;

            lblDemoAccounts.Font = UiTheme.FontMono;
            lblDemoAccounts.ForeColor = UiTheme.BrandMintFaint;
            lblDemoAccounts.Text =
                "Super Admin" + Environment.NewLine +
                "  admin@pharmalink.com.bd      Admin@123" + Environment.NewLine +
                "Pharmacy owners (Pharma@123)" + Environment.NewLine +
                "  kamrul@mitfordpharma.com" + Environment.NewLine +
                "  shirin@dhanmondimedico.com" + Environment.NewLine +
                "  tanvir@lazzcare.com" + Environment.NewLine +
                "Customers (Cust@123)" + Environment.NewLine +
                "  rahim@gmail.com" + Environment.NewLine +
                "  nusrat@gmail.com" + Environment.NewLine +
                Environment.NewLine +
                "imran@newlifepharmacy.com is still Pending" + Environment.NewLine +
                "and is refused until it is approved.";

            lblDemoTitle.Visible = true;
            lblDemoAccounts.Visible = true;
#else
            lblDemoTitle.Visible = false;
            lblDemoAccounts.Visible = false;
#endif
        }

        // ---------------------------------------------------------------------
        //  VALIDATION
        //  Checked as the user types. The Login button stays disabled until the
        //  email looks like an email and a password has been typed, so an
        //  obviously wrong attempt never even reaches the database. There is no
        //  length rule here: accounts created under an older, shorter password
        //  rule must still be able to sign in.
        // ---------------------------------------------------------------------

        private void Field_Changed(object sender, EventArgs e)
        {
            lblFormError.Visible = false;
            ValidateFields();
        }

        private bool ValidateFields()
        {
            bool ok = true;

            if (Validator.IsBlank(txtEmail.Text))
            {
                UiTheme.ClearError(lblEmailError, txtEmail);
                ok = false;
            }
            else if (!Validator.IsEmail(txtEmail.Text))
            {
                UiTheme.ShowError(lblEmailError, txtEmail, "That does not look like a valid email address.");
                ok = false;
            }
            else
            {
                UiTheme.ClearError(lblEmailError, txtEmail);
            }

            UiTheme.ClearError(lblPasswordError, txtPassword);
            if (string.IsNullOrEmpty(txtPassword.Text)) ok = false;

            // A styled button greys itself out while disabled.
            btnLogin.Enabled = ok;
            return ok;
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '*';
        }

        // ---------------------------------------------------------------------
        //  LOGIN AND ROLE ROUTING
        // ---------------------------------------------------------------------

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                string reason;
                User user = _auth.Login(txtEmail.Text.Trim(), txtPassword.Text, out reason);

                if (user == null)
                {
                    UiTheme.ShowError(lblFormError, null, reason);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    return;
                }

                // The session is what every later query filters on.
                UserSession.UserId = user.UserId;
                UserSession.FullName = user.FullName;
                UserSession.Email = user.Email;
                UserSession.UserType = user.UserType;
                UserSession.PharmacyId = user.PharmacyId;
                UserSession.PharmacyName = user.PharmacyName;

                // A temporary password gets the user this far and no further.
                if (user.MustChangePassword && !CompleteRequiredPasswordChange(user))
                {
                    UserSession.Clear();
                    txtPassword.Clear();
                    // Two lines at most: the label under Log in is 34 pixels tall.
                    UiTheme.ShowError(lblFormError, null,
                        "Not signed in. Log in again with your temporary password to choose a new one.");
                    txtPassword.Focus();
                    return;
                }

                RememberEmail(user.Email);
                OpenDashboardFor(user.UserType);
            }
            catch (Exception ex)
            {
                // Describe already says "could not reach the database" when that
                // is the actual problem, and something else when it is not.
                UserSession.Clear();
                UiTheme.ShowError(lblFormError, null, DbHelper.Describe(ex).Replace("\r\n\r\n", " "));
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Shows the forced password change and returns true only when the new
        /// password was saved. The temporary password is handed over from the
        /// password box it was just verified from, then cleared from that box.
        /// </summary>
        private bool CompleteRequiredPasswordChange(User user)
        {
            Cursor = Cursors.Default;
            using (ChangePasswordRequiredForm dialog =
                   new ChangePasswordRequiredForm(user.UserId, user.FullName, txtPassword.Text))
            {
                // The dashboard opening straight away is the confirmation; a
                // message box on top would only be one more click.
                bool changed = dialog.ShowDialog(this) == DialogResult.OK;
                if (changed) txtPassword.Clear();
                return changed;
            }
        }

        /// <summary>
        /// Saves the email when "Remember my email" is ticked and forgets it
        /// when it is not, so unticking the box and signing in once is how a
        /// user removes a remembered email. Never a password.
        /// </summary>
        private void RememberEmail(string email)
        {
            if (chkRememberMe.Checked) LoginPreferences.Save(email);
            else LoginPreferences.Clear();
        }

        /// <summary>
        /// The single decision node of the whole navigation diagram.
        /// One value, three destinations, and nothing else in the application
        /// ever has to ask again who the user is.
        /// </summary>
        private void OpenDashboardFor(string userType)
        {
            Form dashboard;

            switch (userType)
            {
                case "SuperAdmin":
                    dashboard = new SuperAdminDashboard();
                    break;
                case "Admin":
                    dashboard = new AdminDashboard();
                    break;
                case "Customer":
                    dashboard = new CustomerHomeForm();
                    break;
                default:
                    UserSession.Clear();
                    UiTheme.ShowError(lblFormError, null, "This account has an unknown user type.");
                    return;
            }

            Hide();
            dashboard.FormClosed += Dashboard_FormClosed;
            dashboard.Show();
        }

        /// <summary>Logging out closes the dashboard and brings this form back, cleared.</summary>
        private void Dashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            UserSession.Clear();
            txtPassword.Clear();
            lblFormError.Visible = false;
            Show();
            ValidateFields();

            if (chkRememberMe.Checked && Validator.IsEmail(txtEmail.Text)) txtPassword.Focus();
            else txtEmail.Focus();
        }

        /// <summary>
        /// Opens the help-desk reset request, passing on whatever email is
        /// already typed. The login card itself is left exactly as it was.
        /// </summary>
        private void btnForgotPassword_Click(object sender, EventArgs e)
        {
            using (ForgotPasswordForm forgot = new ForgotPasswordForm(txtEmail.Text))
            {
                forgot.ShowDialog(this);
            }
            txtPassword.Focus();
        }

        private void btnGoSignUp_Click(object sender, EventArgs e)
        {
            using (SignUpForm signUp = new SignUpForm())
            {
                Hide();
                signUp.ShowDialog();
                Show();

                // A brand new customer lands straight back here with the email
                // already typed in, so the first login is one click away. This
                // deliberately replaces a remembered email: the account that was
                // just created is the one the user means to sign in to now.
                if (!string.IsNullOrEmpty(signUp.RegisteredEmail))
                {
                    txtEmail.Text = signUp.RegisteredEmail;
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
                ValidateFields();
            }
        }
    }
}
