namespace PharmaLinkApp.Forms
{
    partial class SuperAdminSalesReportForm
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
            lblArea = new Label();
            cmbArea = new ComboBox();
            lblStatusFilter = new Label();
            cmbStatus = new ComboBox();
            btnGenerate = new Button();
            btnExport = new Button();

            dgvReport = new DataGridView();
            lblNote = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReport).BeginInit();
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
            lblTitle.Size = new Size(400, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Platform Sales and Commission";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "What each pharmacy sold, what the platform earned in commission and what it settles to the shops.";
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
            lblArea.AutoSize = true;
            lblArea.Location = new Point(400, 90);
            lblArea.Name = "lblArea";
            lblArea.Size = new Size(40, 18);
            lblArea.TabIndex = 5;
            lblArea.Text = "Area";
            //
            cmbArea.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbArea.Location = new Point(444, 86);
            cmbArea.Name = "cmbArea";
            cmbArea.Size = new Size(160, 27);
            cmbArea.TabIndex = 6;
            //
            lblStatusFilter.AutoSize = true;
            lblStatusFilter.Location = new Point(620, 90);
            lblStatusFilter.Name = "lblStatusFilter";
            lblStatusFilter.Size = new Size(90, 18);
            lblStatusFilter.TabIndex = 7;
            lblStatusFilter.Text = "Order status";
            //
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Location = new Point(716, 86);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(160, 27);
            cmbStatus.TabIndex = 8;
            //
            btnGenerate.Location = new Point(894, 84);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(140, 31);
            btnGenerate.TabIndex = 9;
            btnGenerate.Text = "Generate";
            btnGenerate.Click += btnGenerate_Click;
            //
            btnExport.Location = new Point(1044, 84);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(170, 31);
            btnExport.TabIndex = 10;
            btnExport.Text = "Export CSV";
            btnExport.Click += btnExport_Click;
            //
            dgvReport.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvReport.Location = new Point(20, 226);
            dgvReport.Name = "dgvReport";
            dgvReport.Size = new Size(1194, 356);
            dgvReport.TabIndex = 11;
            //
            lblNote.AutoSize = false;
            lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblNote.Location = new Point(20, 592);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(1194, 40);
            lblNote.TabIndex = 12;
            lblNote.Text = "Sales count confirmed and delivered orders only (choose a single status to see just that status), and item sales leave out the delivery charge. Commission is the amount frozen on each order at checkout, so changing a pharmacy's rate never rewrites past figures. The last row of the grid is the platform total; it is not included in the CSV export.";
            //
            lblStatus.AutoSize = false;
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Location = new Point(20, 644);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1194, 20);
            lblStatus.TabIndex = 13;
            //
            // SuperAdminSalesReportForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1240, 676);
            Controls.Add(lblStatus);
            Controls.Add(lblNote);
            Controls.Add(dgvReport);
            Controls.Add(btnExport);
            Controls.Add(btnGenerate);
            Controls.Add(cmbStatus);
            Controls.Add(lblStatusFilter);
            Controls.Add(cmbArea);
            Controls.Add(lblArea);
            Controls.Add(dtpTo);
            Controls.Add(lblTo);
            Controls.Add(dtpFrom);
            Controls.Add(lblFrom);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SuperAdminSalesReportForm";
            Text = "PharmaLink - Sales and Commission";
            Load += SuperAdminSalesReportForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReport).EndInit();
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
        private Label lblArea;
        private ComboBox cmbArea;
        private Label lblStatusFilter;
        private ComboBox cmbStatus;
        private Button btnGenerate;
        private Button btnExport;
        private DataGridView dgvReport;
        private Label lblNote;
        private Label lblStatus;
    }
}
