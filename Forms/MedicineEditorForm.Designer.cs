namespace PharmaLinkApp.Forms
{
    partial class MedicineEditorForm
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

            lblName = new Label();
            txtName = new TextBox();
            lblNameError = new Label();

            lblGeneric = new Label();
            txtGeneric = new TextBox();
            lblGenericError = new Label();

            lblCategory = new Label();
            cmbCategory = new ComboBox();
            lblManufacturer = new Label();
            txtManufacturer = new TextBox();
            lblManufacturerError = new Label();

            lblStrength = new Label();
            txtStrength = new TextBox();
            lblExpiry = new Label();
            dtpExpiry = new DateTimePicker();
            lblExpiryError = new Label();

            lblUnitPrice = new Label();
            txtUnitPrice = new TextBox();
            lblUnitPriceError = new Label();

            lblStock = new Label();
            txtStock = new TextBox();
            lblStockError = new Label();

            lblMinStock = new Label();
            txtMinStock = new TextBox();
            lblMinStockError = new Label();

            chkRequiresRx = new CheckBox();
            lblDescription = new Label();
            txtDescription = new TextBox();

            lblConstraintNote = new Label();
            btnSave = new Button();
            btnCancel = new Button();

            panelHeader.SuspendLayout();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(680, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Add Medicine";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(500, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Save stays disabled until every rule below is satisfied.";
            //
            lblName.AutoSize = true;
            lblName.Location = new Point(24, 86);
            lblName.Name = "lblName";
            lblName.Size = new Size(110, 18);
            lblName.TabIndex = 1;
            lblName.Text = "Brand name";
            //
            txtName.Location = new Point(24, 108);
            txtName.MaxLength = 120;
            txtName.Name = "txtName";
            txtName.Size = new Size(300, 27);
            txtName.TabIndex = 2;
            txtName.TextChanged += Field_Changed;
            //
            lblNameError.AutoSize = false;
            lblNameError.Location = new Point(24, 136);
            lblNameError.Name = "lblNameError";
            lblNameError.Size = new Size(632, 16);
            lblNameError.TabIndex = 3;
            lblNameError.Visible = false;
            //
            lblGeneric.AutoSize = true;
            lblGeneric.Location = new Point(346, 86);
            lblGeneric.Name = "lblGeneric";
            lblGeneric.Size = new Size(120, 18);
            lblGeneric.TabIndex = 4;
            lblGeneric.Text = "Generic name";
            //
            txtGeneric.Location = new Point(346, 108);
            txtGeneric.MaxLength = 120;
            txtGeneric.Name = "txtGeneric";
            txtGeneric.Size = new Size(310, 27);
            txtGeneric.TabIndex = 5;
            txtGeneric.TextChanged += Field_Changed;
            //
            lblGenericError.AutoSize = false;
            lblGenericError.Location = new Point(24, 154);
            lblGenericError.Name = "lblGenericError";
            lblGenericError.Size = new Size(632, 16);
            lblGenericError.TabIndex = 6;
            lblGenericError.Visible = false;
            //
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(24, 178);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(70, 18);
            lblCategory.TabIndex = 7;
            lblCategory.Text = "Category";
            //
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Location = new Point(24, 200);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new Size(300, 27);
            cmbCategory.TabIndex = 8;
            cmbCategory.SelectedIndexChanged += Field_Changed;
            //
            lblManufacturer.AutoSize = true;
            lblManufacturer.Location = new Point(346, 178);
            lblManufacturer.Name = "lblManufacturer";
            lblManufacturer.Size = new Size(100, 18);
            lblManufacturer.TabIndex = 9;
            lblManufacturer.Text = "Manufacturer";
            //
            txtManufacturer.Location = new Point(346, 200);
            txtManufacturer.MaxLength = 100;
            txtManufacturer.Name = "txtManufacturer";
            txtManufacturer.Size = new Size(310, 27);
            txtManufacturer.TabIndex = 10;
            txtManufacturer.TextChanged += Field_Changed;
            //
            lblManufacturerError.AutoSize = false;
            lblManufacturerError.Location = new Point(24, 228);
            lblManufacturerError.Name = "lblManufacturerError";
            lblManufacturerError.Size = new Size(632, 16);
            lblManufacturerError.TabIndex = 11;
            lblManufacturerError.Visible = false;
            //
            lblStrength.AutoSize = true;
            lblStrength.Location = new Point(24, 252);
            lblStrength.Name = "lblStrength";
            lblStrength.Size = new Size(160, 18);
            lblStrength.TabIndex = 12;
            lblStrength.Text = "Strength (500mg, 100ml)";
            //
            txtStrength.Location = new Point(24, 274);
            txtStrength.MaxLength = 40;
            txtStrength.Name = "txtStrength";
            txtStrength.Size = new Size(300, 27);
            txtStrength.TabIndex = 13;
            txtStrength.TextChanged += Field_Changed;
            //
            lblExpiry.AutoSize = true;
            lblExpiry.Location = new Point(346, 252);
            lblExpiry.Name = "lblExpiry";
            lblExpiry.Size = new Size(90, 18);
            lblExpiry.TabIndex = 14;
            lblExpiry.Text = "Expiry date";
            //
            dtpExpiry.Format = DateTimePickerFormat.Short;
            dtpExpiry.Location = new Point(346, 274);
            dtpExpiry.Name = "dtpExpiry";
            dtpExpiry.Size = new Size(310, 27);
            dtpExpiry.TabIndex = 15;
            dtpExpiry.ValueChanged += Field_Changed;
            //
            lblExpiryError.AutoSize = false;
            lblExpiryError.Location = new Point(24, 302);
            lblExpiryError.Name = "lblExpiryError";
            lblExpiryError.Size = new Size(632, 16);
            lblExpiryError.TabIndex = 16;
            lblExpiryError.Visible = false;
            //
            lblUnitPrice.AutoSize = true;
            lblUnitPrice.Location = new Point(24, 326);
            lblUnitPrice.Name = "lblUnitPrice";
            lblUnitPrice.Size = new Size(120, 18);
            lblUnitPrice.TabIndex = 17;
            lblUnitPrice.Text = "Unit price (Tk)";
            //
            txtUnitPrice.Location = new Point(24, 348);
            txtUnitPrice.Name = "txtUnitPrice";
            txtUnitPrice.Size = new Size(190, 27);
            txtUnitPrice.TabIndex = 18;
            txtUnitPrice.TextChanged += Field_Changed;
            //
            lblUnitPriceError.AutoSize = false;
            lblUnitPriceError.Location = new Point(24, 376);
            lblUnitPriceError.Name = "lblUnitPriceError";
            lblUnitPriceError.Size = new Size(632, 16);
            lblUnitPriceError.TabIndex = 19;
            lblUnitPriceError.Visible = false;
            //
            lblStock.AutoSize = true;
            lblStock.Location = new Point(236, 326);
            lblStock.Name = "lblStock";
            lblStock.Size = new Size(110, 18);
            lblStock.TabIndex = 20;
            lblStock.Text = "Units in stock";
            //
            txtStock.Location = new Point(236, 348);
            txtStock.Name = "txtStock";
            txtStock.Size = new Size(190, 27);
            txtStock.TabIndex = 21;
            txtStock.TextChanged += Field_Changed;
            //
            lblStockError.AutoSize = false;
            lblStockError.Location = new Point(24, 394);
            lblStockError.Name = "lblStockError";
            lblStockError.Size = new Size(632, 16);
            lblStockError.TabIndex = 22;
            lblStockError.Visible = false;
            //
            lblMinStock.AutoSize = true;
            lblMinStock.Location = new Point(448, 326);
            lblMinStock.Name = "lblMinStock";
            lblMinStock.Size = new Size(180, 18);
            lblMinStock.TabIndex = 23;
            lblMinStock.Text = "Minimum stock (alert level)";
            //
            txtMinStock.Location = new Point(448, 348);
            txtMinStock.Name = "txtMinStock";
            txtMinStock.Size = new Size(208, 27);
            txtMinStock.TabIndex = 24;
            txtMinStock.TextChanged += Field_Changed;
            //
            lblMinStockError.AutoSize = false;
            lblMinStockError.Location = new Point(24, 412);
            lblMinStockError.Name = "lblMinStockError";
            lblMinStockError.Size = new Size(632, 16);
            lblMinStockError.TabIndex = 25;
            lblMinStockError.Visible = false;
            //
            chkRequiresRx.AutoSize = true;
            chkRequiresRx.Location = new Point(24, 436);
            chkRequiresRx.Name = "chkRequiresRx";
            chkRequiresRx.Size = new Size(400, 22);
            chkRequiresRx.TabIndex = 26;
            chkRequiresRx.Text = "Prescription only - the customer must upload a doctor's prescription";
            //
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(24, 466);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(90, 18);
            lblDescription.TabIndex = 27;
            lblDescription.Text = "Description";
            //
            txtDescription.Location = new Point(24, 488);
            txtDescription.MaxLength = 400;
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(632, 60);
            txtDescription.TabIndex = 28;
            //
            lblConstraintNote.AutoSize = false;
            lblConstraintNote.Location = new Point(24, 556);
            lblConstraintNote.Name = "lblConstraintNote";
            lblConstraintNote.Size = new Size(632, 48);
            lblConstraintNote.TabIndex = 29;
            lblConstraintNote.Text = "The price must be above zero, stock and minimum stock cannot be negative, and each brand and strength can be listed only once in your pharmacy. Stock you enter is added on top of any sales made while this window is open.";
            //
            btnSave.Location = new Point(386, 612);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(140, 40);
            btnSave.TabIndex = 30;
            btnSave.Text = "Save";
            btnSave.Click += btnSave_Click;
            //
            btnCancel.Location = new Point(536, 612);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 40);
            btnCancel.TabIndex = 31;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            //
            // MedicineEditorForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(680, 670);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(lblConstraintNote);
            Controls.Add(txtDescription);
            Controls.Add(lblDescription);
            Controls.Add(chkRequiresRx);
            Controls.Add(lblMinStockError);
            Controls.Add(txtMinStock);
            Controls.Add(lblMinStock);
            Controls.Add(lblStockError);
            Controls.Add(txtStock);
            Controls.Add(lblStock);
            Controls.Add(lblUnitPriceError);
            Controls.Add(txtUnitPrice);
            Controls.Add(lblUnitPrice);
            Controls.Add(lblExpiryError);
            Controls.Add(dtpExpiry);
            Controls.Add(lblExpiry);
            Controls.Add(txtStrength);
            Controls.Add(lblStrength);
            Controls.Add(lblManufacturerError);
            Controls.Add(txtManufacturer);
            Controls.Add(lblManufacturer);
            Controls.Add(cmbCategory);
            Controls.Add(lblCategory);
            Controls.Add(lblGenericError);
            Controls.Add(txtGeneric);
            Controls.Add(lblGeneric);
            Controls.Add(lblNameError);
            Controls.Add(txtName);
            Controls.Add(lblName);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MedicineEditorForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "PharmaLink - Medicine";
            Load += MedicineEditorForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblName;
        private TextBox txtName;
        private Label lblNameError;
        private Label lblGeneric;
        private TextBox txtGeneric;
        private Label lblGenericError;
        private Label lblCategory;
        private ComboBox cmbCategory;
        private Label lblManufacturer;
        private TextBox txtManufacturer;
        private Label lblManufacturerError;
        private Label lblStrength;
        private TextBox txtStrength;
        private Label lblExpiry;
        private DateTimePicker dtpExpiry;
        private Label lblExpiryError;
        private Label lblUnitPrice;
        private TextBox txtUnitPrice;
        private Label lblUnitPriceError;
        private Label lblStock;
        private TextBox txtStock;
        private Label lblStockError;
        private Label lblMinStock;
        private TextBox txtMinStock;
        private Label lblMinStockError;
        private CheckBox chkRequiresRx;
        private Label lblDescription;
        private TextBox txtDescription;
        private Label lblConstraintNote;
        private Button btnSave;
        private Button btnCancel;
    }
}
