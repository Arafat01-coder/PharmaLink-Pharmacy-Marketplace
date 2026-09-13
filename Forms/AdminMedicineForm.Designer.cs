namespace PharmaLinkApp.Forms
{
    partial class AdminMedicineForm
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

            lblSearch = new Label();
            txtSearch = new TextBox();
            chkShowDelisted = new CheckBox();
            btnRefresh = new Button();

            dgvMedicines = new DataGridView();

            btnAdd = new Button();
            btnEdit = new Button();
            btnDelist = new Button();
            btnRelist = new Button();
            btnCreateOffer = new Button();

            lblIsolationHint = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMedicines).BeginInit();
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
            lblTitle.Size = new Size(200, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "My Medicines";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Create, read, update and delist the medicines this pharmacy sells.";
            //
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(1064, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(20, 92);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(50, 18);
            lblSearch.TabIndex = 1;
            lblSearch.Text = "Search";
            //
            txtSearch.Location = new Point(76, 88);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "brand name or generic name";
            txtSearch.Size = new Size(300, 27);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += txtSearch_TextChanged;
            //
            chkShowDelisted.AutoSize = true;
            chkShowDelisted.Location = new Point(396, 91);
            chkShowDelisted.Name = "chkShowDelisted";
            chkShowDelisted.Size = new Size(200, 22);
            chkShowDelisted.TabIndex = 3;
            chkShowDelisted.Text = "Show delisted medicines too";
            chkShowDelisted.CheckedChanged += Filter_Changed;
            //
            btnRefresh.Location = new Point(614, 86);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(110, 31);
            btnRefresh.TabIndex = 4;
            btnRefresh.Text = "Refresh";
            btnRefresh.Click += Filter_Changed;
            //
            dgvMedicines.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMedicines.Location = new Point(20, 128);
            dgvMedicines.Name = "dgvMedicines";
            dgvMedicines.Size = new Size(1154, 400);
            dgvMedicines.TabIndex = 5;
            dgvMedicines.SelectionChanged += dgvMedicines_SelectionChanged;
            dgvMedicines.CellDoubleClick += dgvMedicines_CellDoubleClick;
            //
            btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAdd.Location = new Point(20, 544);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(160, 40);
            btnAdd.TabIndex = 6;
            btnAdd.Text = "Add medicine";
            btnAdd.Click += btnAdd_Click;
            //
            btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEdit.Location = new Point(190, 544);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(160, 40);
            btnEdit.TabIndex = 7;
            btnEdit.Text = "Update selected";
            btnEdit.Click += btnEdit_Click;
            //
            btnDelist.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelist.Location = new Point(360, 544);
            btnDelist.Name = "btnDelist";
            btnDelist.Size = new Size(160, 40);
            btnDelist.TabIndex = 8;
            btnDelist.Text = "Delete (delist)";
            btnDelist.Click += btnDelist_Click;
            //
            btnRelist.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRelist.Location = new Point(530, 544);
            btnRelist.Name = "btnRelist";
            btnRelist.Size = new Size(160, 40);
            btnRelist.TabIndex = 9;
            btnRelist.Text = "Put back on sale";
            btnRelist.Click += btnRelist_Click;
            //
            btnCreateOffer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCreateOffer.Location = new Point(700, 544);
            btnCreateOffer.Name = "btnCreateOffer";
            btnCreateOffer.Size = new Size(160, 40);
            btnCreateOffer.TabIndex = 10;
            btnCreateOffer.Text = "Create offer";
            btnCreateOffer.Click += btnCreateOffer_Click;
            //
            lblIsolationHint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblIsolationHint.AutoSize = false;
            lblIsolationHint.Location = new Point(20, 594);
            lblIsolationHint.Name = "lblIsolationHint";
            lblIsolationHint.Size = new Size(1154, 34);
            lblIsolationHint.TabIndex = 11;
            //
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 630);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1154, 20);
            lblStatus.TabIndex = 12;
            //
            // AdminMedicineForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 660);
            Controls.Add(lblStatus);
            Controls.Add(lblIsolationHint);
            Controls.Add(btnCreateOffer);
            Controls.Add(btnRelist);
            Controls.Add(btnDelist);
            Controls.Add(btnEdit);
            Controls.Add(btnAdd);
            Controls.Add(dgvMedicines);
            Controls.Add(btnRefresh);
            Controls.Add(chkShowDelisted);
            Controls.Add(txtSearch);
            Controls.Add(lblSearch);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "AdminMedicineForm";
            Text = "PharmaLink - My Medicines";
            Load += AdminMedicineForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMedicines).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private Label lblSearch;
        private TextBox txtSearch;
        private CheckBox chkShowDelisted;
        private Button btnRefresh;
        private DataGridView dgvMedicines;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelist;
        private Button btnRelist;
        private Button btnCreateOffer;
        private Label lblIsolationHint;
        private Label lblStatus;
    }
}
