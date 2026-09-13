namespace PharmaLinkApp.Forms
{
    partial class PharmacyProfileForm
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

            grpShop = new GroupBox();
            lblShopName = new Label();
            txtShopName = new TextBox();
            lblShopNameError = new Label();
            lblLicense = new Label();
            txtLicense = new TextBox();
            lblLicenseNote = new Label();
            lblArea = new Label();
            cmbArea = new ComboBox();
            lblAreaError = new Label();
            lblAddress = new Label();
            txtAddress = new TextBox();
            lblAddressError = new Label();
            lblContact = new Label();
            txtContact = new TextBox();
            lblContactError = new Label();
            lblLogo = new Label();
            txtLogoPath = new TextBox();
            btnBrowseLogo = new Button();
            btnClearLogo = new Button();
            picLogo = new PictureBox();
            lblLogoHint = new Label();
            btnSave = new Button();

            grpFacts = new GroupBox();
            lblStatusCaption = new Label();
            lblStatusValue = new Label();
            lblCommissionCaption = new Label();
            lblCommissionValue = new Label();
            lblRatingCaption = new Label();
            lblRatingValue = new Label();
            lblRegisteredCaption = new Label();
            lblRegisteredValue = new Label();
            lblFactsNote = new Label();

            lblStatus = new Label();

            panelHeader.SuspendLayout();
            grpShop.SuspendLayout();
            grpFacts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
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
            lblTitle.Size = new Size(300, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "My Pharmacy Profile";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(600, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "What customers see about your shop on the catalogue and on every invoice.";
            //
            btnBack.Location = new Point(804, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            grpShop.Controls.Add(lblShopName);
            grpShop.Controls.Add(txtShopName);
            grpShop.Controls.Add(lblShopNameError);
            grpShop.Controls.Add(lblLicense);
            grpShop.Controls.Add(txtLicense);
            grpShop.Controls.Add(lblLicenseNote);
            grpShop.Controls.Add(lblArea);
            grpShop.Controls.Add(cmbArea);
            grpShop.Controls.Add(lblAreaError);
            grpShop.Controls.Add(lblAddress);
            grpShop.Controls.Add(txtAddress);
            grpShop.Controls.Add(lblAddressError);
            grpShop.Controls.Add(lblContact);
            grpShop.Controls.Add(txtContact);
            grpShop.Controls.Add(lblContactError);
            grpShop.Controls.Add(lblLogo);
            grpShop.Controls.Add(txtLogoPath);
            grpShop.Controls.Add(btnBrowseLogo);
            grpShop.Controls.Add(btnClearLogo);
            grpShop.Controls.Add(picLogo);
            grpShop.Controls.Add(lblLogoHint);
            grpShop.Controls.Add(btnSave);
            grpShop.Location = new Point(20, 88);
            grpShop.Name = "grpShop";
            grpShop.Size = new Size(560, 520);
            grpShop.TabIndex = 1;
            grpShop.TabStop = false;
            grpShop.Text = "  Shop details  ";
            //
            lblShopName.AutoSize = true;
            lblShopName.Location = new Point(18, 34);
            lblShopName.Name = "lblShopName";
            lblShopName.Size = new Size(80, 18);
            lblShopName.TabIndex = 0;
            lblShopName.Text = "Shop name";
            //
            txtShopName.Location = new Point(18, 56);
            txtShopName.MaxLength = 120;
            txtShopName.Name = "txtShopName";
            txtShopName.Size = new Size(520, 27);
            txtShopName.TabIndex = 1;
            txtShopName.TextChanged += Field_Changed;
            //
            lblShopNameError.AutoSize = false;
            lblShopNameError.Location = new Point(18, 84);
            lblShopNameError.Name = "lblShopNameError";
            lblShopNameError.Size = new Size(520, 16);
            lblShopNameError.TabIndex = 2;
            lblShopNameError.Visible = false;
            //
            lblLicense.AutoSize = true;
            lblLicense.Location = new Point(18, 104);
            lblLicense.Name = "lblLicense";
            lblLicense.Size = new Size(190, 18);
            lblLicense.TabIndex = 3;
            lblLicense.Text = "DGDA drug licence number";
            //
            txtLicense.Location = new Point(18, 126);
            txtLicense.Name = "txtLicense";
            txtLicense.ReadOnly = true;
            txtLicense.Size = new Size(300, 27);
            txtLicense.TabIndex = 4;
            //
            lblLicenseNote.AutoSize = false;
            lblLicenseNote.Location = new Point(326, 130);
            lblLicenseNote.Name = "lblLicenseNote";
            lblLicenseNote.Size = new Size(212, 32);
            lblLicenseNote.TabIndex = 5;
            lblLicenseNote.Text = "Read only. Changing it would mean a new licence and a fresh approval.";
            //
            lblArea.AutoSize = true;
            lblArea.Location = new Point(18, 168);
            lblArea.Name = "lblArea";
            lblArea.Size = new Size(40, 18);
            lblArea.TabIndex = 6;
            lblArea.Text = "Area";
            //
            cmbArea.Location = new Point(18, 190);
            cmbArea.MaxLength = 60;
            cmbArea.Name = "cmbArea";
            cmbArea.Size = new Size(300, 27);
            cmbArea.TabIndex = 7;
            cmbArea.TextChanged += Field_Changed;
            //
            lblAreaError.AutoSize = false;
            lblAreaError.Location = new Point(18, 218);
            lblAreaError.Name = "lblAreaError";
            lblAreaError.Size = new Size(300, 16);
            lblAreaError.TabIndex = 8;
            lblAreaError.Visible = false;
            //
            lblAddress.AutoSize = true;
            lblAddress.Location = new Point(18, 238);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(220, 18);
            lblAddress.TabIndex = 9;
            lblAddress.Text = "Shop address (printed on invoices)";
            //
            txtAddress.Location = new Point(18, 260);
            txtAddress.MaxLength = 250;
            txtAddress.Multiline = true;
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(520, 56);
            txtAddress.TabIndex = 10;
            txtAddress.TextChanged += Field_Changed;
            //
            lblAddressError.AutoSize = false;
            lblAddressError.Location = new Point(18, 318);
            lblAddressError.Name = "lblAddressError";
            lblAddressError.Size = new Size(520, 16);
            lblAddressError.TabIndex = 11;
            lblAddressError.Visible = false;
            //
            lblContact.AutoSize = true;
            lblContact.Location = new Point(326, 168);
            lblContact.Name = "lblContact";
            lblContact.Size = new Size(150, 18);
            lblContact.TabIndex = 12;
            lblContact.Text = "Shop contact number";
            //
            txtContact.Location = new Point(326, 190);
            txtContact.MaxLength = 20;
            txtContact.Name = "txtContact";
            txtContact.Size = new Size(212, 27);
            txtContact.TabIndex = 13;
            txtContact.TextChanged += Field_Changed;
            //
            lblContactError.AutoSize = false;
            lblContactError.Location = new Point(326, 218);
            lblContactError.Name = "lblContactError";
            lblContactError.Size = new Size(212, 16);
            lblContactError.TabIndex = 14;
            lblContactError.Visible = false;
            //
            lblLogo.AutoSize = true;
            lblLogo.Location = new Point(18, 338);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(80, 18);
            lblLogo.TabIndex = 15;
            lblLogo.Text = "Shop logo";
            //
            picLogo.BorderStyle = BorderStyle.FixedSingle;
            picLogo.Location = new Point(18, 360);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(96, 96);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 19;
            picLogo.TabStop = false;
            //
            txtLogoPath.Location = new Point(124, 360);
            txtLogoPath.Name = "txtLogoPath";
            txtLogoPath.ReadOnly = true;
            txtLogoPath.Size = new Size(414, 27);
            txtLogoPath.TabIndex = 16;
            txtLogoPath.TabStop = false;
            //
            btnBrowseLogo.Location = new Point(124, 394);
            btnBrowseLogo.Name = "btnBrowseLogo";
            btnBrowseLogo.Size = new Size(130, 31);
            btnBrowseLogo.TabIndex = 17;
            btnBrowseLogo.Text = "Choose logo...";
            btnBrowseLogo.Click += btnBrowseLogo_Click;
            //
            btnClearLogo.Location = new Point(262, 394);
            btnClearLogo.Name = "btnClearLogo";
            btnClearLogo.Size = new Size(130, 31);
            btnClearLogo.TabIndex = 20;
            btnClearLogo.Text = "Remove logo";
            btnClearLogo.Click += btnClearLogo_Click;
            //
            lblLogoHint.AutoSize = false;
            lblLogoHint.Location = new Point(124, 430);
            lblLogoHint.Name = "lblLogoHint";
            lblLogoHint.Size = new Size(414, 30);
            lblLogoHint.TabIndex = 21;
            lblLogoHint.Text = "JPG or PNG, smaller than 2 MB. The picture is copied into PharmaLink when you save.";
            //
            btnSave.Location = new Point(18, 468);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(520, 40);
            btnSave.TabIndex = 18;
            btnSave.Text = "Save shop profile";
            btnSave.Click += btnSave_Click;
            //
            grpFacts.Controls.Add(lblStatusCaption);
            grpFacts.Controls.Add(lblStatusValue);
            grpFacts.Controls.Add(lblCommissionCaption);
            grpFacts.Controls.Add(lblCommissionValue);
            grpFacts.Controls.Add(lblRatingCaption);
            grpFacts.Controls.Add(lblRatingValue);
            grpFacts.Controls.Add(lblRegisteredCaption);
            grpFacts.Controls.Add(lblRegisteredValue);
            grpFacts.Controls.Add(lblFactsNote);
            grpFacts.Location = new Point(600, 88);
            grpFacts.Name = "grpFacts";
            grpFacts.Size = new Size(320, 520);
            grpFacts.TabIndex = 2;
            grpFacts.TabStop = false;
            grpFacts.Text = "  Set by the Super Admin  ";
            //
            lblStatusCaption.AutoSize = true;
            lblStatusCaption.Location = new Point(18, 40);
            lblStatusCaption.Name = "lblStatusCaption";
            lblStatusCaption.Size = new Size(120, 16);
            lblStatusCaption.TabIndex = 0;
            lblStatusCaption.Text = "PHARMACY STATUS";
            //
            lblStatusValue.AutoSize = true;
            lblStatusValue.Location = new Point(18, 60);
            lblStatusValue.Name = "lblStatusValue";
            lblStatusValue.Size = new Size(200, 26);
            lblStatusValue.TabIndex = 1;
            //
            lblCommissionCaption.AutoSize = true;
            lblCommissionCaption.Location = new Point(18, 108);
            lblCommissionCaption.Name = "lblCommissionCaption";
            lblCommissionCaption.Size = new Size(140, 16);
            lblCommissionCaption.TabIndex = 2;
            lblCommissionCaption.Text = "COMMISSION RATE";
            //
            lblCommissionValue.AutoSize = true;
            lblCommissionValue.Location = new Point(18, 128);
            lblCommissionValue.Name = "lblCommissionValue";
            lblCommissionValue.Size = new Size(200, 26);
            lblCommissionValue.TabIndex = 3;
            //
            lblRatingCaption.AutoSize = true;
            lblRatingCaption.Location = new Point(18, 176);
            lblRatingCaption.Name = "lblRatingCaption";
            lblRatingCaption.Size = new Size(160, 16);
            lblRatingCaption.TabIndex = 4;
            lblRatingCaption.Text = "AVERAGE CUSTOMER RATING";
            //
            lblRatingValue.AutoSize = true;
            lblRatingValue.Location = new Point(18, 196);
            lblRatingValue.Name = "lblRatingValue";
            lblRatingValue.Size = new Size(200, 26);
            lblRatingValue.TabIndex = 5;
            //
            lblRegisteredCaption.AutoSize = true;
            lblRegisteredCaption.Location = new Point(18, 244);
            lblRegisteredCaption.Name = "lblRegisteredCaption";
            lblRegisteredCaption.Size = new Size(140, 16);
            lblRegisteredCaption.TabIndex = 6;
            lblRegisteredCaption.Text = "REGISTERED ON";
            //
            lblRegisteredValue.AutoSize = true;
            lblRegisteredValue.Location = new Point(18, 264);
            lblRegisteredValue.Name = "lblRegisteredValue";
            lblRegisteredValue.Size = new Size(200, 26);
            lblRegisteredValue.TabIndex = 7;
            //
            lblFactsNote.AutoSize = false;
            lblFactsNote.Location = new Point(18, 316);
            lblFactsNote.Name = "lblFactsNote";
            lblFactsNote.Size = new Size(284, 180);
            lblFactsNote.TabIndex = 8;
            lblFactsNote.Text = "These four values are not yours to edit. Only the Super Admin can change the commission rate, and because each order keeps the commission worked out when it was placed, a change to the rate today never changes what was owed on past sales.";
            //
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 620);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(900, 20);
            lblStatus.TabIndex = 3;
            //
            // PharmacyProfileForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(940, 650);
            Controls.Add(lblStatus);
            Controls.Add(grpFacts);
            Controls.Add(grpShop);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "PharmacyProfileForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "PharmaLink - My Pharmacy Profile";
            Load += PharmacyProfileForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            grpShop.ResumeLayout(false);
            grpShop.PerformLayout();
            grpFacts.ResumeLayout(false);
            grpFacts.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private GroupBox grpShop;
        private Label lblShopName;
        private TextBox txtShopName;
        private Label lblShopNameError;
        private Label lblLicense;
        private TextBox txtLicense;
        private Label lblLicenseNote;
        private Label lblArea;
        private ComboBox cmbArea;
        private Label lblAreaError;
        private Label lblAddress;
        private TextBox txtAddress;
        private Label lblAddressError;
        private Label lblContact;
        private TextBox txtContact;
        private Label lblContactError;
        private Label lblLogo;
        private TextBox txtLogoPath;
        private Button btnBrowseLogo;
        private Button btnClearLogo;
        private PictureBox picLogo;
        private Label lblLogoHint;
        private Button btnSave;
        private GroupBox grpFacts;
        private Label lblStatusCaption;
        private Label lblStatusValue;
        private Label lblCommissionCaption;
        private Label lblCommissionValue;
        private Label lblRatingCaption;
        private Label lblRatingValue;
        private Label lblRegisteredCaption;
        private Label lblRegisteredValue;
        private Label lblFactsNote;
        private Label lblStatus;
    }
}
