namespace PharmaLinkApp.Forms
{
    partial class MyProfileForm
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
            btnBack = new Button();

            grpProfile = new GroupBox();
            lblFullName = new Label();
            txtFullName = new TextBox();
            lblFullNameError = new Label();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblEmailNote = new Label();
            lblPhone = new Label();
            txtPhone = new TextBox();
            lblPhoneError = new Label();
            lblAddress = new Label();
            txtAddress = new TextBox();
            lblAddressError = new Label();
            lblMemberSince = new Label();
            btnSaveProfile = new Button();

            grpPassword = new GroupBox();
            lblCurrent = new Label();
            txtCurrent = new TextBox();
            lblCurrentError = new Label();
            lblNew = new Label();
            txtNew = new TextBox();
            lblNewError = new Label();
            lblConfirm = new Label();
            txtConfirm = new TextBox();
            lblConfirmError = new Label();
            chkShowPasswords = new CheckBox();
            btnChangePassword = new Button();
            lblHashNote = new Label();

            lblStatus = new Label();

            panelHeader.SuspendLayout();
            grpProfile.SuspendLayout();
            grpPassword.SuspendLayout();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(940, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "My Account";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(600, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Your details on the left, your password on the right.";
            //
            btnBack.Location = new Point(804, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            grpProfile.Controls.Add(lblFullName);
            grpProfile.Controls.Add(txtFullName);
            grpProfile.Controls.Add(lblFullNameError);
            grpProfile.Controls.Add(lblEmail);
            grpProfile.Controls.Add(txtEmail);
            grpProfile.Controls.Add(lblEmailNote);
            grpProfile.Controls.Add(lblPhone);
            grpProfile.Controls.Add(txtPhone);
            grpProfile.Controls.Add(lblPhoneError);
            grpProfile.Controls.Add(lblAddress);
            grpProfile.Controls.Add(txtAddress);
            grpProfile.Controls.Add(lblAddressError);
            grpProfile.Controls.Add(lblMemberSince);
            grpProfile.Controls.Add(btnSaveProfile);
            grpProfile.Location = new Point(20, 88);
            grpProfile.Name = "grpProfile";
            grpProfile.Size = new Size(450, 460);
            grpProfile.TabIndex = 1;
            grpProfile.TabStop = false;
            grpProfile.Text = "  My details  ";
            //
            lblFullName.AutoSize = true;
            lblFullName.Location = new Point(18, 34);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new Size(70, 18);
            lblFullName.TabIndex = 0;
            lblFullName.Text = "Full name";
            //
            txtFullName.Location = new Point(18, 56);
            txtFullName.MaxLength = 100;
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(410, 27);
            txtFullName.TabIndex = 1;
            txtFullName.TextChanged += Profile_Changed;
            //
            lblFullNameError.AutoSize = false;
            lblFullNameError.Location = new Point(18, 84);
            lblFullNameError.Name = "lblFullNameError";
            lblFullNameError.Size = new Size(410, 16);
            lblFullNameError.TabIndex = 2;
            lblFullNameError.Visible = false;
            //
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(18, 104);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(90, 18);
            lblEmail.TabIndex = 3;
            lblEmail.Text = "Email address";
            //
            txtEmail.Location = new Point(18, 126);
            txtEmail.Name = "txtEmail";
            txtEmail.ReadOnly = true;
            txtEmail.Size = new Size(410, 27);
            txtEmail.TabIndex = 4;
            //
            lblEmailNote.AutoSize = false;
            lblEmailNote.Location = new Point(18, 154);
            lblEmailNote.Name = "lblEmailNote";
            lblEmailNote.Size = new Size(410, 16);
            lblEmailNote.TabIndex = 5;
            lblEmailNote.Text = "Read only - your email is your login identifier.";
            //
            lblPhone.AutoSize = true;
            lblPhone.Location = new Point(18, 176);
            lblPhone.Name = "lblPhone";
            lblPhone.Size = new Size(120, 18);
            lblPhone.TabIndex = 6;
            lblPhone.Text = "Mobile (11 digits)";
            //
            txtPhone.Location = new Point(18, 198);
            txtPhone.MaxLength = 20;
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(410, 27);
            txtPhone.TabIndex = 7;
            txtPhone.TextChanged += Profile_Changed;
            //
            lblPhoneError.AutoSize = false;
            lblPhoneError.Location = new Point(18, 226);
            lblPhoneError.Name = "lblPhoneError";
            lblPhoneError.Size = new Size(410, 16);
            lblPhoneError.TabIndex = 8;
            lblPhoneError.Visible = false;
            //
            lblAddress.AutoSize = true;
            lblAddress.Location = new Point(18, 246);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(230, 18);
            lblAddress.TabIndex = 9;
            lblAddress.Text = "Address (pre-filled at checkout)";
            //
            txtAddress.Location = new Point(18, 268);
            txtAddress.MaxLength = 250;
            txtAddress.Multiline = true;
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(410, 64);
            txtAddress.TabIndex = 10;
            txtAddress.TextChanged += Profile_Changed;
            //
            lblAddressError.AutoSize = false;
            lblAddressError.Location = new Point(18, 334);
            lblAddressError.Name = "lblAddressError";
            lblAddressError.Size = new Size(410, 16);
            lblAddressError.TabIndex = 11;
            lblAddressError.Visible = false;
            //
            lblMemberSince.AutoSize = false;
            lblMemberSince.Location = new Point(18, 356);
            lblMemberSince.Name = "lblMemberSince";
            lblMemberSince.Size = new Size(410, 20);
            lblMemberSince.TabIndex = 12;
            //
            btnSaveProfile.Location = new Point(18, 388);
            btnSaveProfile.Name = "btnSaveProfile";
            btnSaveProfile.Size = new Size(410, 42);
            btnSaveProfile.TabIndex = 13;
            btnSaveProfile.Text = "Save my details";
            btnSaveProfile.Click += btnSaveProfile_Click;
            //
            grpPassword.Controls.Add(lblCurrent);
            grpPassword.Controls.Add(txtCurrent);
            grpPassword.Controls.Add(lblCurrentError);
            grpPassword.Controls.Add(lblNew);
            grpPassword.Controls.Add(txtNew);
            grpPassword.Controls.Add(lblNewError);
            grpPassword.Controls.Add(lblConfirm);
            grpPassword.Controls.Add(txtConfirm);
            grpPassword.Controls.Add(lblConfirmError);
            grpPassword.Controls.Add(chkShowPasswords);
            grpPassword.Controls.Add(btnChangePassword);
            grpPassword.Controls.Add(lblHashNote);
            grpPassword.Location = new Point(490, 88);
            grpPassword.Name = "grpPassword";
            grpPassword.Size = new Size(430, 460);
            grpPassword.TabIndex = 2;
            grpPassword.TabStop = false;
            grpPassword.Text = "  Change my password  ";
            //
            lblCurrent.AutoSize = true;
            lblCurrent.Location = new Point(18, 34);
            lblCurrent.Name = "lblCurrent";
            lblCurrent.Size = new Size(130, 18);
            lblCurrent.TabIndex = 0;
            lblCurrent.Text = "Current password";
            //
            txtCurrent.Location = new Point(18, 56);
            txtCurrent.MaxLength = 100;
            txtCurrent.Name = "txtCurrent";
            txtCurrent.PasswordChar = '*';
            txtCurrent.Size = new Size(390, 27);
            txtCurrent.TabIndex = 1;
            txtCurrent.TextChanged += Password_Changed;
            //
            lblCurrentError.AutoSize = false;
            lblCurrentError.Location = new Point(18, 84);
            lblCurrentError.Name = "lblCurrentError";
            lblCurrentError.Size = new Size(390, 16);
            lblCurrentError.TabIndex = 2;
            lblCurrentError.Visible = false;
            //
            lblNew.AutoSize = true;
            lblNew.Location = new Point(18, 104);
            lblNew.Name = "lblNew";
            lblNew.Size = new Size(110, 18);
            lblNew.TabIndex = 3;
            lblNew.Text = "New password";
            //
            txtNew.Location = new Point(18, 126);
            txtNew.MaxLength = 100;
            txtNew.Name = "txtNew";
            txtNew.PasswordChar = '*';
            txtNew.Size = new Size(390, 27);
            txtNew.TabIndex = 4;
            txtNew.TextChanged += Password_Changed;
            //
            lblNewError.AutoSize = false;
            lblNewError.Location = new Point(18, 154);
            lblNewError.Name = "lblNewError";
            lblNewError.Size = new Size(390, 16);
            lblNewError.TabIndex = 5;
            lblNewError.Visible = false;
            //
            lblConfirm.AutoSize = true;
            lblConfirm.Location = new Point(18, 176);
            lblConfirm.Name = "lblConfirm";
            lblConfirm.Size = new Size(160, 18);
            lblConfirm.TabIndex = 6;
            lblConfirm.Text = "Confirm new password";
            //
            txtConfirm.Location = new Point(18, 198);
            txtConfirm.MaxLength = 100;
            txtConfirm.Name = "txtConfirm";
            txtConfirm.PasswordChar = '*';
            txtConfirm.Size = new Size(390, 27);
            txtConfirm.TabIndex = 7;
            txtConfirm.TextChanged += Password_Changed;
            //
            lblConfirmError.AutoSize = false;
            lblConfirmError.Location = new Point(18, 226);
            lblConfirmError.Name = "lblConfirmError";
            lblConfirmError.Size = new Size(390, 16);
            lblConfirmError.TabIndex = 8;
            lblConfirmError.Visible = false;
            //
            chkShowPasswords.AutoSize = true;
            chkShowPasswords.Location = new Point(18, 248);
            chkShowPasswords.Name = "chkShowPasswords";
            chkShowPasswords.Size = new Size(160, 22);
            chkShowPasswords.TabIndex = 9;
            chkShowPasswords.Text = "Show passwords";
            chkShowPasswords.CheckedChanged += chkShowPasswords_CheckedChanged;
            //
            btnChangePassword.Location = new Point(18, 282);
            btnChangePassword.Name = "btnChangePassword";
            btnChangePassword.Size = new Size(390, 42);
            btnChangePassword.TabIndex = 10;
            btnChangePassword.Text = "Update password";
            btnChangePassword.Click += btnChangePassword_Click;
            //
            lblHashNote.AutoSize = false;
            lblHashNote.Location = new Point(18, 336);
            lblHashNote.Name = "lblHashNote";
            lblHashNote.Size = new Size(390, 110);
            lblHashNote.TabIndex = 11;
            lblHashNote.Text = "For your security, your password is stored in scrambled form and is never shown to anyone, including PharmaLink staff. You need your current password to set a new one.";
            //
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 560);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(900, 36);
            lblStatus.TabIndex = 3;
            //
            // MyProfileForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(940, 606);
            Controls.Add(lblStatus);
            Controls.Add(grpPassword);
            Controls.Add(grpProfile);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MyProfileForm";
            Text = "PharmaLink - My Account";
            Load += MyProfileForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            grpProfile.ResumeLayout(false);
            grpProfile.PerformLayout();
            grpPassword.ResumeLayout(false);
            grpPassword.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private GroupBox grpProfile;
        private Label lblFullName;
        private TextBox txtFullName;
        private Label lblFullNameError;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblEmailNote;
        private Label lblPhone;
        private TextBox txtPhone;
        private Label lblPhoneError;
        private Label lblAddress;
        private TextBox txtAddress;
        private Label lblAddressError;
        private Label lblMemberSince;
        private Button btnSaveProfile;
        private GroupBox grpPassword;
        private Label lblCurrent;
        private TextBox txtCurrent;
        private Label lblCurrentError;
        private Label lblNew;
        private TextBox txtNew;
        private Label lblNewError;
        private Label lblConfirm;
        private TextBox txtConfirm;
        private Label lblConfirmError;
        private CheckBox chkShowPasswords;
        private Button btnChangePassword;
        private Label lblHashNote;
        private Label lblStatus;
    }
}
