namespace PharmaLinkApp.Forms
{
    partial class DiscountOffersForm
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

            lblGridTitle = new Label();
            dgvOffers = new DataGridView();
            btnPause = new Button();
            btnResume = new Button();
            btnDelete = new Button();

            grpEditor = new GroupBox();
            lblMedicine = new Label();
            cmbMedicine = new ComboBox();
            lblMedicineError = new Label();
            lblOfferTitle = new Label();
            txtOfferTitle = new TextBox();
            lblOfferTitleError = new Label();
            lblPercent = new Label();
            txtPercent = new TextBox();
            lblPercentError = new Label();
            lblStart = new Label();
            dtpStart = new DateTimePicker();
            lblEnd = new Label();
            dtpEnd = new DateTimePicker();
            lblDateError = new Label();
            lblPreview = new Label();
            btnCreate = new Button();
            btnUpdate = new Button();
            btnClearEditor = new Button();

            lblNote = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            grpEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOffers).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1200, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(220, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Discount Offers";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "A percentage off one of your medicines, running between two dates.";
            //
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(1064, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            lblGridTitle.AutoSize = true;
            lblGridTitle.Location = new Point(20, 86);
            lblGridTitle.Name = "lblGridTitle";
            lblGridTitle.Size = new Size(220, 22);
            lblGridTitle.TabIndex = 1;
            lblGridTitle.Text = "My offers";
            //
            dgvOffers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvOffers.Location = new Point(20, 112);
            dgvOffers.Name = "dgvOffers";
            dgvOffers.Size = new Size(720, 420);
            dgvOffers.TabIndex = 2;
            dgvOffers.SelectionChanged += dgvOffers_SelectionChanged;
            //
            btnPause.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPause.Location = new Point(20, 546);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(160, 38);
            btnPause.TabIndex = 3;
            btnPause.Text = "Pause offer";
            btnPause.Click += btnPause_Click;
            //
            btnResume.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnResume.Location = new Point(190, 546);
            btnResume.Name = "btnResume";
            btnResume.Size = new Size(160, 38);
            btnResume.TabIndex = 4;
            btnResume.Text = "Resume offer";
            btnResume.Click += btnResume_Click;
            //
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(360, 546);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(160, 38);
            btnDelete.TabIndex = 5;
            btnDelete.Text = "Delete offer";
            btnDelete.Click += btnDelete_Click;
            //
            grpEditor.Controls.Add(lblMedicine);
            grpEditor.Controls.Add(cmbMedicine);
            grpEditor.Controls.Add(lblMedicineError);
            grpEditor.Controls.Add(lblOfferTitle);
            grpEditor.Controls.Add(txtOfferTitle);
            grpEditor.Controls.Add(lblOfferTitleError);
            grpEditor.Controls.Add(lblPercent);
            grpEditor.Controls.Add(txtPercent);
            grpEditor.Controls.Add(lblPercentError);
            grpEditor.Controls.Add(lblStart);
            grpEditor.Controls.Add(dtpStart);
            grpEditor.Controls.Add(lblEnd);
            grpEditor.Controls.Add(dtpEnd);
            grpEditor.Controls.Add(lblDateError);
            grpEditor.Controls.Add(lblPreview);
            grpEditor.Controls.Add(btnCreate);
            grpEditor.Controls.Add(btnUpdate);
            grpEditor.Controls.Add(btnClearEditor);
            grpEditor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpEditor.Location = new Point(760, 86);
            grpEditor.Name = "grpEditor";
            grpEditor.Size = new Size(414, 498);
            grpEditor.TabIndex = 6;
            grpEditor.TabStop = false;
            grpEditor.Text = "  Create or edit an offer  ";
            //
            lblMedicine.AutoSize = true;
            lblMedicine.Location = new Point(18, 34);
            lblMedicine.Name = "lblMedicine";
            lblMedicine.Size = new Size(70, 18);
            lblMedicine.TabIndex = 0;
            lblMedicine.Text = "Medicine";
            //
            cmbMedicine.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMedicine.Location = new Point(18, 56);
            cmbMedicine.Name = "cmbMedicine";
            cmbMedicine.Size = new Size(376, 27);
            cmbMedicine.TabIndex = 1;
            cmbMedicine.SelectedIndexChanged += Field_Changed;
            //
            lblMedicineError.AutoSize = false;
            lblMedicineError.Location = new Point(18, 84);
            lblMedicineError.Name = "lblMedicineError";
            lblMedicineError.Size = new Size(376, 16);
            lblMedicineError.TabIndex = 2;
            lblMedicineError.Visible = false;
            //
            lblOfferTitle.AutoSize = true;
            lblOfferTitle.Location = new Point(18, 104);
            lblOfferTitle.Name = "lblOfferTitle";
            lblOfferTitle.Size = new Size(80, 18);
            lblOfferTitle.TabIndex = 3;
            lblOfferTitle.Text = "Offer title";
            //
            txtOfferTitle.Location = new Point(18, 126);
            txtOfferTitle.MaxLength = 120;
            txtOfferTitle.Name = "txtOfferTitle";
            txtOfferTitle.Size = new Size(376, 27);
            txtOfferTitle.TabIndex = 4;
            txtOfferTitle.TextChanged += Field_Changed;
            //
            lblOfferTitleError.AutoSize = false;
            lblOfferTitleError.Location = new Point(18, 154);
            lblOfferTitleError.Name = "lblOfferTitleError";
            lblOfferTitleError.Size = new Size(376, 16);
            lblOfferTitleError.TabIndex = 5;
            lblOfferTitleError.Visible = false;
            //
            lblPercent.AutoSize = true;
            lblPercent.Location = new Point(18, 174);
            lblPercent.Name = "lblPercent";
            lblPercent.Size = new Size(200, 18);
            lblPercent.TabIndex = 6;
            lblPercent.Text = "Discount percent (over 0, up to 70)";
            //
            txtPercent.Location = new Point(18, 196);
            txtPercent.Name = "txtPercent";
            txtPercent.Size = new Size(120, 27);
            txtPercent.TabIndex = 7;
            txtPercent.TextChanged += Field_Changed;
            //
            lblPercentError.AutoSize = false;
            lblPercentError.Location = new Point(18, 226);
            lblPercentError.Name = "lblPercentError";
            lblPercentError.Size = new Size(376, 16);
            lblPercentError.TabIndex = 8;
            lblPercentError.Visible = false;
            //
            lblStart.AutoSize = true;
            lblStart.Location = new Point(18, 248);
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(70, 18);
            lblStart.TabIndex = 9;
            lblStart.Text = "Start date";
            //
            dtpStart.Format = DateTimePickerFormat.Short;
            dtpStart.Location = new Point(18, 270);
            dtpStart.Name = "dtpStart";
            dtpStart.Size = new Size(180, 27);
            dtpStart.TabIndex = 10;
            dtpStart.ValueChanged += Field_Changed;
            //
            lblEnd.AutoSize = true;
            lblEnd.Location = new Point(214, 248);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new Size(70, 18);
            lblEnd.TabIndex = 11;
            lblEnd.Text = "End date";
            //
            dtpEnd.Format = DateTimePickerFormat.Short;
            dtpEnd.Location = new Point(214, 270);
            dtpEnd.Name = "dtpEnd";
            dtpEnd.Size = new Size(180, 27);
            dtpEnd.TabIndex = 12;
            dtpEnd.ValueChanged += Field_Changed;
            //
            lblDateError.AutoSize = false;
            lblDateError.Location = new Point(18, 300);
            lblDateError.Name = "lblDateError";
            lblDateError.Size = new Size(376, 16);
            lblDateError.TabIndex = 13;
            lblDateError.Visible = false;
            //
            lblPreview.AutoSize = false;
            lblPreview.Location = new Point(18, 322);
            lblPreview.Name = "lblPreview";
            lblPreview.Size = new Size(376, 46);
            lblPreview.TabIndex = 14;
            //
            btnCreate.Location = new Point(18, 376);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(184, 40);
            btnCreate.TabIndex = 15;
            btnCreate.Text = "Create offer";
            btnCreate.Click += btnCreate_Click;
            //
            btnUpdate.Location = new Point(210, 376);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(184, 40);
            btnUpdate.TabIndex = 16;
            btnUpdate.Text = "Save changes";
            btnUpdate.Click += btnUpdate_Click;
            //
            btnClearEditor.Location = new Point(18, 424);
            btnClearEditor.Name = "btnClearEditor";
            btnClearEditor.Size = new Size(376, 34);
            btnClearEditor.TabIndex = 17;
            btnClearEditor.Text = "Clear the form";
            btnClearEditor.Click += btnClearEditor_Click;
            //
            lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblNote.AutoSize = false;
            lblNote.Location = new Point(20, 594);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(720, 46);
            lblNote.TabIndex = 7;
            lblNote.Text = "Once saved, the discounted price appears on the customer's Offers screen for exactly the dates chosen. If two offers on the same medicine run on the same day, customers get the higher discount, never both.";
            //
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 644);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1154, 20);
            lblStatus.TabIndex = 8;
            //
            // DiscountOffersForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 676);
            Controls.Add(lblStatus);
            Controls.Add(lblNote);
            Controls.Add(grpEditor);
            Controls.Add(btnDelete);
            Controls.Add(btnResume);
            Controls.Add(btnPause);
            Controls.Add(dgvOffers);
            Controls.Add(lblGridTitle);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "DiscountOffersForm";
            Text = "PharmaLink - Discount Offers";
            Load += DiscountOffersForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            grpEditor.ResumeLayout(false);
            grpEditor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOffers).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private Label lblGridTitle;
        private DataGridView dgvOffers;
        private Button btnPause;
        private Button btnResume;
        private Button btnDelete;
        private GroupBox grpEditor;
        private Label lblMedicine;
        private ComboBox cmbMedicine;
        private Label lblMedicineError;
        private Label lblOfferTitle;
        private TextBox txtOfferTitle;
        private Label lblOfferTitleError;
        private Label lblPercent;
        private TextBox txtPercent;
        private Label lblPercentError;
        private Label lblStart;
        private DateTimePicker dtpStart;
        private Label lblEnd;
        private DateTimePicker dtpEnd;
        private Label lblDateError;
        private Label lblPreview;
        private Button btnCreate;
        private Button btnUpdate;
        private Button btnClearEditor;
        private Label lblNote;
        private Label lblStatus;
    }
}
