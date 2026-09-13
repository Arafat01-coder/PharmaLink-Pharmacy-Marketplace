namespace PharmaLinkApp.Forms
{
    partial class SuperAdminLowRatedShopsForm
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

            lblThreshold = new Label();
            numThreshold = new NumericUpDown();
            lblMinReviews = new Label();
            numMinReviews = new NumericUpDown();
            btnGenerate = new Button();
            btnExport = new Button();

            dgvLowRated = new DataGridView();
            btnOpenPharmacy = new Button();
            lblSqlNote = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numThreshold).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMinReviews).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvLowRated).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1120, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Low Rated Pharmacies";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Act on poor service before customers leave the platform.";
            //
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(984, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            lblThreshold.AutoSize = true;
            lblThreshold.Location = new Point(20, 92);
            lblThreshold.Name = "lblThreshold";
            lblThreshold.Size = new Size(180, 18);
            lblThreshold.TabIndex = 1;
            lblThreshold.Text = "Average rating below";
            //
            numThreshold.DecimalPlaces = 1;
            numThreshold.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numThreshold.Location = new Point(178, 88);
            numThreshold.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            numThreshold.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numThreshold.Name = "numThreshold";
            numThreshold.Size = new Size(80, 27);
            numThreshold.TabIndex = 2;
            numThreshold.Value = new decimal(new int[] { 25, 0, 0, 65536 });
            //
            lblMinReviews.AutoSize = true;
            lblMinReviews.Location = new Point(280, 92);
            lblMinReviews.Name = "lblMinReviews";
            lblMinReviews.Size = new Size(190, 18);
            lblMinReviews.TabIndex = 3;
            lblMinReviews.Text = "with at least this many reviews";
            //
            numMinReviews.Location = new Point(480, 88);
            numMinReviews.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numMinReviews.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMinReviews.Name = "numMinReviews";
            numMinReviews.Size = new Size(80, 27);
            numMinReviews.TabIndex = 4;
            numMinReviews.Value = new decimal(new int[] { 2, 0, 0, 0 });
            //
            btnGenerate.Location = new Point(582, 86);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(150, 31);
            btnGenerate.TabIndex = 5;
            btnGenerate.Text = "Run report";
            btnGenerate.Click += btnGenerate_Click;
            //
            btnExport.Location = new Point(742, 86);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(150, 31);
            btnExport.TabIndex = 6;
            btnExport.Text = "Export CSV";
            btnExport.Click += btnExport_Click;
            //
            dgvLowRated.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvLowRated.Location = new Point(20, 130);
            dgvLowRated.Name = "dgvLowRated";
            dgvLowRated.Size = new Size(1074, 340);
            dgvLowRated.TabIndex = 7;
            dgvLowRated.CellDoubleClick += dgvLowRated_CellDoubleClick;
            //
            btnOpenPharmacy.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOpenPharmacy.Location = new Point(20, 484);
            btnOpenPharmacy.Name = "btnOpenPharmacy";
            btnOpenPharmacy.Size = new Size(260, 38);
            btnOpenPharmacy.TabIndex = 8;
            btnOpenPharmacy.Text = "Open in Manage Pharmacies";
            btnOpenPharmacy.Click += btnOpenPharmacy_Click;
            //
            lblSqlNote.AutoSize = false;
            lblSqlNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblSqlNote.Location = new Point(20, 534);
            lblSqlNote.Name = "lblSqlNote";
            lblSqlNote.Size = new Size(1074, 78);
            lblSqlNote.TabIndex = 9;
            lblSqlNote.Text = "";
            //
            lblStatus.AutoSize = false;
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Location = new Point(20, 618);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1074, 20);
            lblStatus.TabIndex = 10;
            //
            // SuperAdminLowRatedShopsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 650);
            Controls.Add(lblStatus);
            Controls.Add(lblSqlNote);
            Controls.Add(btnOpenPharmacy);
            Controls.Add(dgvLowRated);
            Controls.Add(btnExport);
            Controls.Add(btnGenerate);
            Controls.Add(numMinReviews);
            Controls.Add(lblMinReviews);
            Controls.Add(numThreshold);
            Controls.Add(lblThreshold);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SuperAdminLowRatedShopsForm";
            Text = "PharmaLink - Low Rated Pharmacies";
            Load += SuperAdminLowRatedShopsForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numThreshold).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMinReviews).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvLowRated).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private Label lblThreshold;
        private NumericUpDown numThreshold;
        private Label lblMinReviews;
        private NumericUpDown numMinReviews;
        private Button btnGenerate;
        private Button btnExport;
        private DataGridView dgvLowRated;
        private Button btnOpenPharmacy;
        private Label lblSqlNote;
        private Label lblStatus;
    }
}
