namespace PharmaLinkApp.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelBrand = new Panel();
            lblBrandName = new Label();
            lblBrandMark = new Label();
            lblTagline = new Label();
            lblBrandBlurb = new Label();
            lblDemoTitle = new Label();
            lblDemoAccounts = new Label();

            panelCard = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblEmailError = new Label();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblPasswordError = new Label();
            chkShowPassword = new CheckBox();
            btnLogin = new Button();
            lblFormError = new Label();
            lblNoAccount = new Label();
            btnGoSignUp = new Button();

            panelBrand.SuspendLayout();
            panelCard.SuspendLayout();
            SuspendLayout();
            //
            // panelBrand
            //
            panelBrand.Controls.Add(lblBrandMark);
            panelBrand.Controls.Add(lblBrandName);
            panelBrand.Controls.Add(lblTagline);
            panelBrand.Controls.Add(lblBrandBlurb);
            panelBrand.Controls.Add(lblDemoTitle);
            panelBrand.Controls.Add(lblDemoAccounts);
            panelBrand.Dock = DockStyle.Left;
            panelBrand.Location = new Point(0, 0);
            panelBrand.Name = "panelBrand";
            panelBrand.Size = new Size(420, 560);
            panelBrand.TabIndex = 0;
            //
            // lblBrandMark
            //
            lblBrandMark.AutoSize = true;
            lblBrandMark.Location = new Point(40, 48);
            lblBrandMark.Name = "lblBrandMark";
            lblBrandMark.Size = new Size(40, 40);
            lblBrandMark.TabIndex = 0;
            lblBrandMark.Text = "+";
            //
            // lblBrandName
            //
            lblBrandName.AutoSize = true;
            lblBrandName.Location = new Point(40, 96);
            lblBrandName.Name = "lblBrandName";
            lblBrandName.Size = new Size(160, 40);
            lblBrandName.TabIndex = 1;
            lblBrandName.Text = "PharmaLink";
            //
            // lblTagline
            //
            lblTagline.AutoSize = true;
            lblTagline.Location = new Point(43, 142);
            lblTagline.Name = "lblTagline";
            lblTagline.Size = new Size(300, 20);
            lblTagline.TabIndex = 2;
            lblTagline.Text = "Pharmacy Marketplace";
            //
            // lblBrandBlurb
            //
            lblBrandBlurb.AutoSize = false;
            lblBrandBlurb.Location = new Point(43, 186);
            lblBrandBlurb.Name = "lblBrandBlurb";
            lblBrandBlurb.Size = new Size(330, 76);
            lblBrandBlurb.TabIndex = 3;
            lblBrandBlurb.Text = "An online marketplace that connects patients with licensed neighbourhood pharmacies. One account, three roles, one entry point.";
            //
            // lblDemoTitle
            //
            lblDemoTitle.AutoSize = true;
            lblDemoTitle.Location = new Point(43, 280);
            lblDemoTitle.Name = "lblDemoTitle";
            lblDemoTitle.Size = new Size(200, 20);
            lblDemoTitle.TabIndex = 4;
            lblDemoTitle.Text = "DEMONSTRATION ACCOUNTS";
            lblDemoTitle.Visible = false;
            //
            // lblDemoAccounts
            //
            lblDemoAccounts.AutoSize = true;
            lblDemoAccounts.Location = new Point(43, 304);
            lblDemoAccounts.Name = "lblDemoAccounts";
            lblDemoAccounts.Size = new Size(340, 180);
            lblDemoAccounts.TabIndex = 5;
            lblDemoAccounts.Text = "";
            lblDemoAccounts.Visible = false;
            //
            // panelCard
            //
            panelCard.Controls.Add(lblTitle);
            panelCard.Controls.Add(lblSubtitle);
            panelCard.Controls.Add(lblEmail);
            panelCard.Controls.Add(txtEmail);
            panelCard.Controls.Add(lblEmailError);
            panelCard.Controls.Add(lblPassword);
            panelCard.Controls.Add(txtPassword);
            panelCard.Controls.Add(lblPasswordError);
            panelCard.Controls.Add(chkShowPassword);
            panelCard.Controls.Add(btnLogin);
            panelCard.Controls.Add(lblFormError);
            panelCard.Controls.Add(lblNoAccount);
            panelCard.Controls.Add(btnGoSignUp);
            panelCard.Location = new Point(468, 74);
            panelCard.Name = "panelCard";
            panelCard.Size = new Size(400, 412);
            panelCard.TabIndex = 1;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(26, 24);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(120, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Sign in";
            //
            // lblSubtitle
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(28, 56);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(320, 18);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Super Admin, pharmacy owner and customer, one form.";
            //
            // lblEmail
            //
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(26, 96);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(90, 18);
            lblEmail.TabIndex = 2;
            lblEmail.Text = "Email address";
            //
            // txtEmail
            //
            txtEmail.Location = new Point(26, 118);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(346, 27);
            txtEmail.TabIndex = 3;
            txtEmail.TextChanged += Field_Changed;
            //
            // lblEmailError
            //
            lblEmailError.AutoSize = false;
            lblEmailError.Location = new Point(26, 146);
            lblEmailError.Name = "lblEmailError";
            lblEmailError.Size = new Size(346, 18);
            lblEmailError.TabIndex = 4;
            lblEmailError.Visible = false;
            //
            // lblPassword
            //
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(26, 172);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(70, 18);
            lblPassword.TabIndex = 5;
            lblPassword.Text = "Password";
            //
            // txtPassword
            //
            txtPassword.Location = new Point(26, 194);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(346, 27);
            txtPassword.TabIndex = 6;
            txtPassword.TextChanged += Field_Changed;
            //
            // lblPasswordError
            //
            lblPasswordError.AutoSize = false;
            lblPasswordError.Location = new Point(26, 222);
            lblPasswordError.Name = "lblPasswordError";
            lblPasswordError.Size = new Size(346, 18);
            lblPasswordError.TabIndex = 7;
            lblPasswordError.Visible = false;
            //
            // chkShowPassword
            //
            chkShowPassword.AutoSize = true;
            chkShowPassword.Location = new Point(26, 244);
            chkShowPassword.Name = "chkShowPassword";
            chkShowPassword.Size = new Size(140, 22);
            chkShowPassword.TabIndex = 8;
            chkShowPassword.Text = "Show password";
            chkShowPassword.CheckedChanged += chkShowPassword_CheckedChanged;
            //
            // btnLogin
            //
            btnLogin.Location = new Point(26, 280);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(346, 42);
            btnLogin.TabIndex = 9;
            btnLogin.Text = "Log in";
            btnLogin.Click += btnLogin_Click;
            //
            // lblFormError
            //
            lblFormError.AutoSize = false;
            lblFormError.Location = new Point(26, 326);
            lblFormError.Name = "lblFormError";
            lblFormError.Size = new Size(346, 34);
            lblFormError.TabIndex = 10;
            lblFormError.Visible = false;
            //
            // lblNoAccount
            //
            lblNoAccount.AutoSize = true;
            lblNoAccount.Location = new Point(26, 372);
            lblNoAccount.Name = "lblNoAccount";
            lblNoAccount.Size = new Size(140, 18);
            lblNoAccount.TabIndex = 11;
            lblNoAccount.Text = "New to PharmaLink?";
            //
            // btnGoSignUp
            //
            btnGoSignUp.Location = new Point(190, 366);
            btnGoSignUp.Name = "btnGoSignUp";
            btnGoSignUp.Size = new Size(182, 32);
            btnGoSignUp.TabIndex = 12;
            btnGoSignUp.Text = "Create an account";
            btnGoSignUp.Click += btnGoSignUp_Click;
            //
            // LoginForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(920, 560);
            Controls.Add(panelCard);
            Controls.Add(panelBrand);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "LoginForm";
            Text = "PharmaLink - Sign in";
            Load += LoginForm_Load;
            panelBrand.ResumeLayout(false);
            panelBrand.PerformLayout();
            panelCard.ResumeLayout(false);
            panelCard.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelBrand;
        private Label lblBrandMark;
        private Label lblBrandName;
        private Label lblTagline;
        private Label lblBrandBlurb;
        private Label lblDemoTitle;
        private Label lblDemoAccounts;

        private Panel panelCard;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblEmailError;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblPasswordError;
        private CheckBox chkShowPassword;
        private Button btnLogin;
        private Label lblFormError;
        private Label lblNoAccount;
        private Button btnGoSignUp;
    }
}
