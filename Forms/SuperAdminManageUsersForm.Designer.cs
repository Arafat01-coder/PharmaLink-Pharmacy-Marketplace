namespace PharmaLinkApp.Forms
{
    partial class SuperAdminManageUsersForm
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
            lblStatusFilter = new Label();
            cmbStatus = new ComboBox();
            lblTypeFilter = new Label();
            cmbUserType = new ComboBox();
            btnSearch = new Button();
            btnClear = new Button();

            dgvUsers = new DataGridView();

            btnSuspend = new Button();
            btnActivate = new Button();
            lblNote = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUsers).BeginInit();
            SuspendLayout();
            //
            // panelHeader
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
            lblTitle.Size = new Size(180, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Manage Users";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(600, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Every Admin and Customer in one grid. An empty search box means no filter rather than no results.";
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
            lblSearch.Location = new Point(20, 90);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(50, 18);
            lblSearch.TabIndex = 1;
            lblSearch.Text = "Search";
            //
            txtSearch.Location = new Point(76, 86);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "name or email";
            txtSearch.Size = new Size(280, 27);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += Filter_Changed;
            //
            lblStatusFilter.AutoSize = true;
            lblStatusFilter.Location = new Point(374, 90);
            lblStatusFilter.Name = "lblStatusFilter";
            lblStatusFilter.Size = new Size(50, 18);
            lblStatusFilter.TabIndex = 3;
            lblStatusFilter.Text = "Status";
            //
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Location = new Point(430, 86);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(150, 27);
            cmbStatus.TabIndex = 4;
            cmbStatus.SelectedIndexChanged += Filter_Changed;
            //
            lblTypeFilter.AutoSize = true;
            lblTypeFilter.Location = new Point(598, 90);
            lblTypeFilter.Name = "lblTypeFilter";
            lblTypeFilter.Size = new Size(70, 18);
            lblTypeFilter.TabIndex = 5;
            lblTypeFilter.Text = "User type";
            //
            cmbUserType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUserType.Location = new Point(674, 86);
            cmbUserType.Name = "cmbUserType";
            cmbUserType.Size = new Size(170, 27);
            cmbUserType.TabIndex = 6;
            cmbUserType.SelectedIndexChanged += Filter_Changed;
            //
            btnSearch.Location = new Point(864, 84);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(100, 31);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "Search";
            btnSearch.Click += btnSearch_Click;
            //
            btnClear.Location = new Point(974, 84);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(100, 31);
            btnClear.TabIndex = 8;
            btnClear.Text = "Clear filters";
            btnClear.Click += btnClear_Click;
            //
            dgvUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvUsers.Location = new Point(20, 128);
            dgvUsers.Name = "dgvUsers";
            dgvUsers.Size = new Size(1154, 420);
            dgvUsers.TabIndex = 9;
            dgvUsers.SelectionChanged += dgvUsers_SelectionChanged;
            //
            btnSuspend.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSuspend.Location = new Point(20, 562);
            btnSuspend.Name = "btnSuspend";
            btnSuspend.Size = new Size(160, 36);
            btnSuspend.TabIndex = 10;
            btnSuspend.Text = "Suspend account";
            btnSuspend.Click += btnSuspend_Click;
            //
            btnActivate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnActivate.Location = new Point(190, 562);
            btnActivate.Name = "btnActivate";
            btnActivate.Size = new Size(160, 36);
            btnActivate.TabIndex = 11;
            btnActivate.Text = "Activate account";
            btnActivate.Click += btnActivate_Click;
            //
            lblNote.AutoSize = false;
            lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblNote.Location = new Point(370, 560);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(804, 40);
            lblNote.TabIndex = 12;
            lblNote.Text = "Suspending a pharmacy owner also suspends their approved pharmacy, which hides it from customers. Activating an owner reinstates a suspended pharmacy. A pending or rejected registration cannot be activated here - approve it from Manage Pharmacies.";
            //
            lblStatus.AutoSize = false;
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Location = new Point(20, 612);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1154, 20);
            lblStatus.TabIndex = 13;
            //
            // SuperAdminManageUsersForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 646);
            Controls.Add(lblStatus);
            Controls.Add(lblNote);
            Controls.Add(btnActivate);
            Controls.Add(btnSuspend);
            Controls.Add(dgvUsers);
            Controls.Add(btnClear);
            Controls.Add(btnSearch);
            Controls.Add(cmbUserType);
            Controls.Add(lblTypeFilter);
            Controls.Add(cmbStatus);
            Controls.Add(lblStatusFilter);
            Controls.Add(txtSearch);
            Controls.Add(lblSearch);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SuperAdminManageUsersForm";
            Text = "PharmaLink - Manage Users";
            Load += SuperAdminManageUsersForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUsers).EndInit();
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
        private Label lblStatusFilter;
        private ComboBox cmbStatus;
        private Label lblTypeFilter;
        private ComboBox cmbUserType;
        private Button btnSearch;
        private Button btnClear;
        private DataGridView dgvUsers;
        private Button btnSuspend;
        private Button btnActivate;
        private Label lblNote;
        private Label lblStatus;
    }
}
