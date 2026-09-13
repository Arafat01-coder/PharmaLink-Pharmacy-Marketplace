namespace PharmaLinkApp.Forms
{
    partial class ChangePasswordRequiredForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();

            lblIntro = new Label();
            lblNew = new Label();
            txtNewPassword = new TextBox();
            lblNewError = new Label();
            lblConfirm = new Label();
            txtConfirmPassword = new TextBox();
            lblConfirmError = new Label();
            chkShowPasswords = new CheckBox();
            lblRules = new Label();
            lblFormError = new Label();
            btnSavePassword = new Button();
            btnCancelChange = new Button();

            panelHeader.SuspendLayout();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(480, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(260, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Choose a new password";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(400, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "You signed in with a temporary password.";
            //
            lblIntro.AutoSize = false;
            lblIntro.Location = new Point(24, 82);
            lblIntro.Name = "lblIntro";
            lblIntro.Size = new Size(432, 40);
            lblIntro.TabIndex = 1;
            lblIntro.Text = "The temporary password the administrator gave you must be replaced before you continue. Choose a password only you know.";
            //
            lblNew.AutoSize = true;
            lblNew.Location = new Point(24, 132);
            lblNew.Name = "lblNew";
            lblNew.Size = new Size(100, 18);
            lblNew.TabIndex = 2;
            lblNew.Text = "New password";
            //
            txtNewPassword.Location = new Point(24, 154);
            txtNewPassword.MaxLength = 100;
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.PasswordChar = '*';
            txtNewPassword.Size = new Size(432, 27);
            txtNewPassword.TabIndex = 3;
            txtNewPassword.TextChanged += Field_Changed;
            //
            lblNewError.AutoSize = false;
            lblNewError.Location = new Point(24, 182);
            lblNewError.Name = "lblNewError";
            lblNewError.Size = new Size(432, 18);
            lblNewError.TabIndex = 4;
            lblNewError.Visible = false;
            //
            lblConfirm.AutoSize = true;
            lblConfirm.Location = new Point(24, 206);
            lblConfirm.Name = "lblConfirm";
            lblConfirm.Size = new Size(150, 18);
            lblConfirm.TabIndex = 5;
            lblConfirm.Text = "Confirm new password";
            //
            txtConfirmPassword.Location = new Point(24, 228);
            txtConfirmPassword.MaxLength = 100;
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.PasswordChar = '*';
            txtConfirmPassword.Size = new Size(432, 27);
            txtConfirmPassword.TabIndex = 6;
            txtConfirmPassword.TextChanged += Field_Changed;
            //
            lblConfirmError.AutoSize = false;
            lblConfirmError.Location = new Point(24, 256);
            lblConfirmError.Name = "lblConfirmError";
            lblConfirmError.Size = new Size(432, 18);
            lblConfirmError.TabIndex = 7;
            lblConfirmError.Visible = false;
            //
            chkShowPasswords.AutoSize = true;
            chkShowPasswords.Location = new Point(24, 280);
            chkShowPasswords.Name = "chkShowPasswords";
            chkShowPasswords.Size = new Size(140, 22);
            chkShowPasswords.TabIndex = 8;
            chkShowPasswords.Text = "Show passwords";
            chkShowPasswords.CheckedChanged += chkShowPasswords_CheckedChanged;
            //
            lblRules.AutoSize = false;
            lblRules.Location = new Point(24, 308);
            lblRules.Name = "lblRules";
            lblRules.Size = new Size(432, 20);
            lblRules.TabIndex = 9;
            lblRules.Text = "At least 8 characters, with at least one letter and one digit.";
            //
            lblFormError.AutoSize = false;
            lblFormError.Location = new Point(24, 330);
            lblFormError.Name = "lblFormError";
            lblFormError.Size = new Size(432, 36);
            lblFormError.TabIndex = 10;
            lblFormError.Visible = false;
            //
            btnSavePassword.Location = new Point(176, 376);
            btnSavePassword.Name = "btnSavePassword";
            btnSavePassword.Size = new Size(170, 42);
            btnSavePassword.TabIndex = 11;
            btnSavePassword.Text = "Save and continue";
            btnSavePassword.Click += btnSavePassword_Click;
            //
            btnCancelChange.Location = new Point(356, 376);
            btnCancelChange.Name = "btnCancelChange";
            btnCancelChange.Size = new Size(100, 42);
            btnCancelChange.TabIndex = 12;
            btnCancelChange.Text = "Cancel";
            btnCancelChange.Click += btnCancelChange_Click;
            //
            // ChangePasswordRequiredForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 436);
            Controls.Add(btnCancelChange);
            Controls.Add(btnSavePassword);
            Controls.Add(lblFormError);
            Controls.Add(lblRules);
            Controls.Add(chkShowPasswords);
            Controls.Add(lblConfirmError);
            Controls.Add(txtConfirmPassword);
            Controls.Add(lblConfirm);
            Controls.Add(lblNewError);
            Controls.Add(txtNewPassword);
            Controls.Add(lblNew);
            Controls.Add(lblIntro);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChangePasswordRequiredForm";
            ShowInTaskbar = false;
            Text = "PharmaLink - Choose a new password";
            Load += ChangePasswordRequiredForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblIntro;
        private Label lblNew;
        private TextBox txtNewPassword;
        private Label lblNewError;
        private Label lblConfirm;
        private TextBox txtConfirmPassword;
        private Label lblConfirmError;
        private CheckBox chkShowPasswords;
        private Label lblRules;
        private Label lblFormError;
        private Button btnSavePassword;
        private Button btnCancelChange;
    }
}
