namespace PharmaLinkApp.Forms
{
    partial class AdminEarningsForm
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

            lblFrom = new Label();
            dtpFrom = new DateTimePicker();
            lblTo = new Label();
            dtpTo = new DateTimePicker();
            lblMedicine = new Label();
            cmbMedicine = new ComboBox();
            btnGenerate = new Button();
            btnExport = new Button();

            dgvSales = new DataGridView();
            lblNote = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSales).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1240, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Sales and Earnings";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Who bought what, on which date and time, at what unit price, and exactly what PharmaLink deducts.";
            //
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(1104, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            lblFrom.AutoSize = true;
            lblFrom.Location = new Point(20, 90);
            lblFrom.Name = "lblFrom";
            lblFrom.Size = new Size(40, 18);
            lblFrom.TabIndex = 1;
            lblFrom.Text = "From";
            //
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.Location = new Point(64, 86);
            dtpFrom.Name = "dtpFrom";
            dtpFrom.Size = new Size(140, 27);
            dtpFrom.TabIndex = 2;
            //
            lblTo.AutoSize = true;
            lblTo.Location = new Point(216, 90);
            lblTo.Name = "lblTo";
            lblTo.Size = new Size(24, 18);
            lblTo.TabIndex = 3;
            lblTo.Text = "To";
            //
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.Location = new Point(244, 86);
            dtpTo.Name = "dtpTo";
            dtpTo.Size = new Size(140, 27);
            dtpTo.TabIndex = 4;
            //
            lblMedicine.AutoSize = true;
            lblMedicine.Location = new Point(400, 90);
            lblMedicine.Name = "lblMedicine";
            lblMedicine.Size = new Size(70, 18);
            lblMedicine.TabIndex = 5;
            lblMedicine.Text = "Medicine";
            //
            cmbMedicine.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMedicine.Location = new Point(476, 86);
            cmbMedicine.Name = "cmbMedicine";
            cmbMedicine.Size = new Size(400, 27);
            cmbMedicine.TabIndex = 6;
            //
            btnGenerate.Location = new Point(894, 84);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(140, 31);
            btnGenerate.TabIndex = 7;
            btnGenerate.Text = "Generate";
            btnGenerate.Click += btnGenerate_Click;
            //
            btnExport.Location = new Point(1044, 84);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(170, 31);
            btnExport.TabIndex = 8;
            btnExport.Text = "Export CSV";
            btnExport.Click += btnExport_Click;
            //
            dgvSales.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvSales.Location = new Point(20, 226);
            dgvSales.Name = "dgvSales";
            dgvSales.Size = new Size(1194, 356);
            dgvSales.TabIndex = 9;
            //
            lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblNote.AutoSize = false;
            lblNote.Location = new Point(20, 592);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(1194, 40);
            lblNote.TabIndex = 10;
            lblNote.Text = "The totals count confirmed and delivered orders only, without delivery charges. Commission is fixed when each order is placed, at your rate on that day, so a later change to the rate or to a price never changes what was owed on past sales.";
            //
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 644);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1194, 20);
            lblStatus.TabIndex = 11;
            //
            // AdminEarningsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1240, 676);
            Controls.Add(lblStatus);
            Controls.Add(lblNote);
            Controls.Add(dgvSales);
            Controls.Add(btnExport);
            Controls.Add(btnGenerate);
            Controls.Add(cmbMedicine);
            Controls.Add(lblMedicine);
            Controls.Add(dtpTo);
            Controls.Add(lblTo);
            Controls.Add(dtpFrom);
            Controls.Add(lblFrom);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "AdminEarningsForm";
            Text = "PharmaLink - Sales and Earnings";
            Load += AdminEarningsForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSales).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private Label lblFrom;
        private DateTimePicker dtpFrom;
        private Label lblTo;
        private DateTimePicker dtpTo;
        private Label lblMedicine;
        private ComboBox cmbMedicine;
        private Button btnGenerate;
        private Button btnExport;
        private DataGridView dgvSales;
        private Label lblNote;
        private Label lblStatus;
    }
}
