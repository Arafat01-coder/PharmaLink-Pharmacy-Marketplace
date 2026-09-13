namespace PharmaLinkApp.Forms
{
    partial class SignUpForm
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
            panelHeader = new Panel();
            lblHeader = new Label();
            lblHeaderSub = new Label();

            lblRegisterAs = new Label();
            cmbRegisterAs = new ComboBox();

            grpPersonal = new GroupBox();
            lblFullName = new Label();
            txtFullName = new TextBox();
            lblFullNameError = new Label();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblEmailError = new Label();
            lblPhone = new Label();
            txtPhone = new TextBox();
            lblPhoneError = new Label();
            lblAddress = new Label();
            txtAddress = new TextBox();
            lblAddressError = new Label();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblPasswordError = new Label();
            lblConfirm = new Label();
            txtConfirm = new TextBox();
            lblConfirmError = new Label();

            grpPharmacy = new GroupBox();
            lblShopName = new Label();
            txtShopName = new TextBox();
            lblShopNameError = new Label();
            lblLicenseNo = new Label();
            txtLicenseNo = new TextBox();
            lblLicenseError = new Label();
            lblArea = new Label();
            cmbArea = new ComboBox();
            lblAreaError = new Label();
            lblShopAddress = new Label();
            txtShopAddress = new TextBox();
            lblShopAddressError = new Label();
            lblShopPhone = new Label();
            txtShopPhone = new TextBox();
            lblShopPhoneError = new Label();
            lblPendingNote = new Label();

            lblFormMessage = new Label();
            btnCreate = new Button();
            btnBack = new Button();

            panelHeader.SuspendLayout();
            grpPersonal.SuspendLayout();
            grpPharmacy.SuspendLayout();
            SuspendLayout();
            //
            // panelHeader
            //
            panelHeader.Controls.Add(lblHeader);
            panelHeader.Controls.Add(lblHeaderSub);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(900, 68);
            panelHeader.TabIndex = 0;
            //
            // lblHeader
            //
            lblHeader.AutoSize = true;
            lblHeader.Location = new Point(20, 10);
            lblHeader.Name = "lblHeader";
            lblHeader.Size = new Size(200, 30);
            lblHeader.TabIndex = 0;
            lblHeader.Text = "Create your account";
            //
            // lblHeaderSub
            //
            lblHeaderSub.AutoSize = true;
            lblHeaderSub.Location = new Point(23, 42);
            lblHeaderSub.Name = "lblHeaderSub";
            lblHeaderSub.Size = new Size(400, 16);
            lblHeaderSub.TabIndex = 1;
            lblHeaderSub.Text = "Customers can order immediately. Pharmacy owners wait for Super Admin approval.";
            //
            // lblRegisterAs
            //
            lblRegisterAs.AutoSize = true;
            lblRegisterAs.Location = new Point(24, 86);
            lblRegisterAs.Name = "lblRegisterAs";
            lblRegisterAs.Size = new Size(80, 18);
            lblRegisterAs.TabIndex = 1;
            lblRegisterAs.Text = "Register as";
            //
            // cmbRegisterAs
            //
            cmbRegisterAs.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRegisterAs.Location = new Point(110, 82);
            cmbRegisterAs.Name = "cmbRegisterAs";
            cmbRegisterAs.Size = new Size(240, 27);
            cmbRegisterAs.TabIndex = 2;
            cmbRegisterAs.SelectedIndexChanged += cmbRegisterAs_SelectedIndexChanged;
            //
            // grpPersonal
            //
            grpPersonal.Controls.Add(lblFullName);
            grpPersonal.Controls.Add(txtFullName);
            grpPersonal.Controls.Add(lblFullNameError);
            grpPersonal.Controls.Add(lblEmail);
            grpPersonal.Controls.Add(txtEmail);
            grpPersonal.Controls.Add(lblEmailError);
            grpPersonal.Controls.Add(lblPhone);
            grpPersonal.Controls.Add(txtPhone);
            grpPersonal.Controls.Add(lblPhoneError);
            grpPersonal.Controls.Add(lblAddress);
            grpPersonal.Controls.Add(txtAddress);
            grpPersonal.Controls.Add(lblAddressError);
            grpPersonal.Controls.Add(lblPassword);
            grpPersonal.Controls.Add(txtPassword);
            grpPersonal.Controls.Add(lblPasswordError);
            grpPersonal.Controls.Add(lblConfirm);
            grpPersonal.Controls.Add(txtConfirm);
            grpPersonal.Controls.Add(lblConfirmError);
            grpPersonal.Location = new Point(24, 122);
            grpPersonal.Name = "grpPersonal";
            grpPersonal.Size = new Size(410, 462);
            grpPersonal.TabIndex = 3;
            grpPersonal.TabStop = false;
            grpPersonal.Text = "  Your details  ";
            //
            // lblFullName
            //
            lblFullName.AutoSize = true;
            lblFullName.Location = new Point(18, 32);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new Size(70, 18);
            lblFullName.TabIndex = 0;
            lblFullName.Text = "Full name";
            //
            // txtFullName
            //
            txtFullName.Location = new Point(18, 54);
            txtFullName.MaxLength = 100;
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(370, 27);
            txtFullName.TabIndex = 1;
            txtFullName.TextChanged += Field_Changed;
            //
            // lblFullNameError
            //
            lblFullNameError.AutoSize = false;
            lblFullNameError.Location = new Point(18, 82);
            lblFullNameError.Name = "lblFullNameError";
            lblFullNameError.Size = new Size(370, 16);
            lblFullNameError.TabIndex = 2;
            lblFullNameError.Visible = false;
            //
            // lblEmail
            //
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(18, 102);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(90, 18);
            lblEmail.TabIndex = 3;
            lblEmail.Text = "Email address";
            //
            // txtEmail
            //
            txtEmail.Location = new Point(18, 124);
            txtEmail.MaxLength = 120;
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(370, 27);
            txtEmail.TabIndex = 4;
            txtEmail.TextChanged += Field_Changed;
            //
            // lblEmailError
            //
            lblEmailError.AutoSize = false;
            lblEmailError.Location = new Point(18, 152);
            lblEmailError.Name = "lblEmailError";
            lblEmailError.Size = new Size(370, 16);
            lblEmailError.TabIndex = 5;
            lblEmailError.Visible = false;
            //
            // lblPhone
            //
            lblPhone.AutoSize = true;
            lblPhone.Location = new Point(18, 172);
            lblPhone.Name = "lblPhone";
            lblPhone.Size = new Size(120, 18);
            lblPhone.TabIndex = 6;
            lblPhone.Text = "Mobile (11 digits)";
            //
            // txtPhone
            //
            txtPhone.Location = new Point(18, 194);
            txtPhone.MaxLength = 20;
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(370, 27);
            txtPhone.TabIndex = 7;
            txtPhone.TextChanged += Field_Changed;
            //
            // lblPhoneError
            //
            lblPhoneError.AutoSize = false;
            lblPhoneError.Location = new Point(18, 222);
            lblPhoneError.Name = "lblPhoneError";
            lblPhoneError.Size = new Size(370, 16);
            lblPhoneError.TabIndex = 8;
            lblPhoneError.Visible = false;
            //
            // lblAddress
            //
            lblAddress.AutoSize = true;
            lblAddress.Location = new Point(18, 242);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(110, 18);
            lblAddress.TabIndex = 9;
            lblAddress.Text = "Delivery address";
            //
            // txtAddress
            //
            txtAddress.Location = new Point(18, 264);
            txtAddress.MaxLength = 250;
            txtAddress.Multiline = true;
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(370, 48);
            txtAddress.TabIndex = 10;
            txtAddress.TextChanged += Field_Changed;
            //
            // lblAddressError
            //
            lblAddressError.AutoSize = false;
            lblAddressError.Location = new Point(18, 314);
            lblAddressError.Name = "lblAddressError";
            lblAddressError.Size = new Size(370, 16);
            lblAddressError.TabIndex = 11;
            lblAddressError.Visible = false;
            //
            // lblPassword
            //
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(18, 334);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(70, 18);
            lblPassword.TabIndex = 12;
            lblPassword.Text = "Password";
            //
            // txtPassword
            //
            txtPassword.Location = new Point(18, 356);
            txtPassword.MaxLength = 100;
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(180, 27);
            txtPassword.TabIndex = 13;
            txtPassword.TextChanged += Field_Changed;
            //
            // lblPasswordError
            //
            lblPasswordError.AutoSize = false;
            lblPasswordError.Location = new Point(18, 386);
            lblPasswordError.Name = "lblPasswordError";
            lblPasswordError.Size = new Size(370, 16);
            lblPasswordError.TabIndex = 14;
            lblPasswordError.Visible = false;
            //
            // lblConfirm
            //
            lblConfirm.AutoSize = true;
            lblConfirm.Location = new Point(208, 334);
            lblConfirm.Name = "lblConfirm";
            lblConfirm.Size = new Size(120, 18);
            lblConfirm.TabIndex = 15;
            lblConfirm.Text = "Confirm password";
            //
            // txtConfirm
            //
            txtConfirm.Location = new Point(208, 356);
            txtConfirm.MaxLength = 100;
            txtConfirm.Name = "txtConfirm";
            txtConfirm.PasswordChar = '*';
            txtConfirm.Size = new Size(180, 27);
            txtConfirm.TabIndex = 16;
            txtConfirm.TextChanged += Field_Changed;
            //
            // lblConfirmError
            //
            lblConfirmError.AutoSize = false;
            lblConfirmError.Location = new Point(18, 406);
            lblConfirmError.Name = "lblConfirmError";
            lblConfirmError.Size = new Size(370, 16);
            lblConfirmError.TabIndex = 17;
            lblConfirmError.Visible = false;
            //
            // grpPharmacy
            //
            grpPharmacy.Controls.Add(lblShopName);
            grpPharmacy.Controls.Add(txtShopName);
            grpPharmacy.Controls.Add(lblShopNameError);
            grpPharmacy.Controls.Add(lblLicenseNo);
            grpPharmacy.Controls.Add(txtLicenseNo);
            grpPharmacy.Controls.Add(lblLicenseError);
            grpPharmacy.Controls.Add(lblArea);
            grpPharmacy.Controls.Add(cmbArea);
            grpPharmacy.Controls.Add(lblAreaError);
            grpPharmacy.Controls.Add(lblShopAddress);
            grpPharmacy.Controls.Add(txtShopAddress);
            grpPharmacy.Controls.Add(lblShopAddressError);
            grpPharmacy.Controls.Add(lblShopPhone);
            grpPharmacy.Controls.Add(txtShopPhone);
            grpPharmacy.Controls.Add(lblShopPhoneError);
            grpPharmacy.Controls.Add(lblPendingNote);
            grpPharmacy.Location = new Point(454, 122);
            grpPharmacy.Name = "grpPharmacy";
            grpPharmacy.Size = new Size(410, 462);
            grpPharmacy.TabIndex = 4;
            grpPharmacy.TabStop = false;
            grpPharmacy.Text = "  Your pharmacy  ";
            //
            // lblShopName
            //
            lblShopName.AutoSize = true;
            lblShopName.Location = new Point(18, 32);
            lblShopName.Name = "lblShopName";
            lblShopName.Size = new Size(80, 18);
            lblShopName.TabIndex = 0;
            lblShopName.Text = "Shop name";
            //
            // txtShopName
            //
            txtShopName.Location = new Point(18, 54);
            txtShopName.MaxLength = 120;
            txtShopName.Name = "txtShopName";
            txtShopName.Size = new Size(370, 27);
            txtShopName.TabIndex = 1;
            txtShopName.TextChanged += Field_Changed;
            //
            // lblShopNameError
            //
            lblShopNameError.AutoSize = false;
            lblShopNameError.Location = new Point(18, 82);
            lblShopNameError.Name = "lblShopNameError";
            lblShopNameError.Size = new Size(370, 16);
            lblShopNameError.TabIndex = 2;
            lblShopNameError.Visible = false;
            //
            // lblLicenseNo
            //
            lblLicenseNo.AutoSize = true;
            lblLicenseNo.Location = new Point(18, 102);
            lblLicenseNo.Name = "lblLicenseNo";
            lblLicenseNo.Size = new Size(190, 18);
            lblLicenseNo.TabIndex = 3;
            lblLicenseNo.Text = "DGDA drug licence number";
            //
            // txtLicenseNo
            //
            txtLicenseNo.Location = new Point(18, 124);
            txtLicenseNo.MaxLength = 40;
            txtLicenseNo.Name = "txtLicenseNo";
            txtLicenseNo.Size = new Size(370, 27);
            txtLicenseNo.TabIndex = 4;
            txtLicenseNo.TextChanged += Field_Changed;
            //
            // lblLicenseError
            //
            lblLicenseError.AutoSize = false;
            lblLicenseError.Location = new Point(18, 152);
            lblLicenseError.Name = "lblLicenseError";
            lblLicenseError.Size = new Size(370, 16);
            lblLicenseError.TabIndex = 5;
            lblLicenseError.Visible = false;
            //
            // lblArea
            //
            lblArea.AutoSize = true;
            lblArea.Location = new Point(18, 172);
            lblArea.Name = "lblArea";
            lblArea.Size = new Size(40, 18);
            lblArea.TabIndex = 6;
            lblArea.Text = "Area";
            //
            // cmbArea
            //
            cmbArea.Location = new Point(18, 194);
            cmbArea.MaxLength = 60;
            cmbArea.Name = "cmbArea";
            cmbArea.Size = new Size(370, 27);
            cmbArea.TabIndex = 7;
            cmbArea.TextChanged += Field_Changed;
            //
            // lblAreaError
            //
            lblAreaError.AutoSize = false;
            lblAreaError.Location = new Point(18, 222);
            lblAreaError.Name = "lblAreaError";
            lblAreaError.Size = new Size(370, 16);
            lblAreaError.TabIndex = 8;
            lblAreaError.Visible = false;
            //
            // lblShopAddress
            //
            lblShopAddress.AutoSize = true;
            lblShopAddress.Location = new Point(18, 242);
            lblShopAddress.Name = "lblShopAddress";
            lblShopAddress.Size = new Size(100, 18);
            lblShopAddress.TabIndex = 9;
            lblShopAddress.Text = "Shop address";
            //
            // txtShopAddress
            //
            txtShopAddress.Location = new Point(18, 264);
            txtShopAddress.MaxLength = 250;
            txtShopAddress.Multiline = true;
            txtShopAddress.Name = "txtShopAddress";
            txtShopAddress.Size = new Size(370, 48);
            txtShopAddress.TabIndex = 10;
            txtShopAddress.TextChanged += Field_Changed;
            //
            // lblShopAddressError
            //
            lblShopAddressError.AutoSize = false;
            lblShopAddressError.Location = new Point(18, 314);
            lblShopAddressError.Name = "lblShopAddressError";
            lblShopAddressError.Size = new Size(370, 16);
            lblShopAddressError.TabIndex = 11;
            lblShopAddressError.Visible = false;
            //
            // lblShopPhone
            //
            lblShopPhone.AutoSize = true;
            lblShopPhone.Location = new Point(18, 334);
            lblShopPhone.Name = "lblShopPhone";
            lblShopPhone.Size = new Size(90, 18);
            lblShopPhone.TabIndex = 12;
            lblShopPhone.Text = "Shop contact";
            //
            // txtShopPhone
            //
            txtShopPhone.Location = new Point(18, 356);
            txtShopPhone.MaxLength = 20;
            txtShopPhone.Name = "txtShopPhone";
            txtShopPhone.Size = new Size(370, 27);
            txtShopPhone.TabIndex = 13;
            txtShopPhone.TextChanged += Field_Changed;
            //
            // lblShopPhoneError
            //
            lblShopPhoneError.AutoSize = false;
            lblShopPhoneError.Location = new Point(18, 386);
            lblShopPhoneError.Name = "lblShopPhoneError";
            lblShopPhoneError.Size = new Size(370, 16);
            lblShopPhoneError.TabIndex = 14;
            lblShopPhoneError.Visible = false;
            //
            // lblPendingNote
            //
            lblPendingNote.AutoSize = true;
            lblPendingNote.Location = new Point(18, 404);
            lblPendingNote.MaximumSize = new Size(374, 0);
            lblPendingNote.Name = "lblPendingNote";
            lblPendingNote.Size = new Size(374, 48);
            lblPendingNote.TabIndex = 15;
            lblPendingNote.Text = "Your account and your pharmacy both start as Pending. Until the Super Admin checks the licence and approves it, you cannot log in and customers cannot see your medicines.";
            //
            // lblFormMessage
            //
            lblFormMessage.AutoSize = false;
            lblFormMessage.Location = new Point(24, 594);
            lblFormMessage.Name = "lblFormMessage";
            lblFormMessage.Size = new Size(460, 52);
            lblFormMessage.TabIndex = 5;
            lblFormMessage.Visible = false;
            //
            // btnCreate
            //
            btnCreate.Location = new Point(626, 596);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(238, 40);
            btnCreate.TabIndex = 6;
            btnCreate.Text = "Create account";
            btnCreate.Click += btnCreate_Click;
            //
            // btnBack
            //
            btnBack.Location = new Point(500, 596);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 40);
            btnBack.TabIndex = 7;
            btnBack.Text = "Back to login";
            btnBack.Click += btnBack_Click;
            //
            // SignUpForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 656);
            Controls.Add(btnBack);
            Controls.Add(btnCreate);
            Controls.Add(lblFormMessage);
            Controls.Add(grpPharmacy);
            Controls.Add(grpPersonal);
            Controls.Add(cmbRegisterAs);
            Controls.Add(lblRegisterAs);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SignUpForm";
            Text = "PharmaLink - Create account";
            Load += SignUpForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            grpPersonal.ResumeLayout(false);
            grpPersonal.PerformLayout();
            grpPharmacy.ResumeLayout(false);
            grpPharmacy.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblHeader;
        private Label lblHeaderSub;
        private Label lblRegisterAs;
        private ComboBox cmbRegisterAs;

        private GroupBox grpPersonal;
        private Label lblFullName;
        private TextBox txtFullName;
        private Label lblFullNameError;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblEmailError;
        private Label lblPhone;
        private TextBox txtPhone;
        private Label lblPhoneError;
        private Label lblAddress;
        private TextBox txtAddress;
        private Label lblAddressError;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblPasswordError;
        private Label lblConfirm;
        private TextBox txtConfirm;
        private Label lblConfirmError;

        private GroupBox grpPharmacy;
        private Label lblShopName;
        private TextBox txtShopName;
        private Label lblShopNameError;
        private Label lblLicenseNo;
        private TextBox txtLicenseNo;
        private Label lblLicenseError;
        private Label lblArea;
        private ComboBox cmbArea;
        private Label lblAreaError;
        private Label lblShopAddress;
        private TextBox txtShopAddress;
        private Label lblShopAddressError;
        private Label lblShopPhone;
        private TextBox txtShopPhone;
        private Label lblShopPhoneError;
        private Label lblPendingNote;

        private Label lblFormMessage;
        private Button btnCreate;
        private Button btnBack;
    }
}
