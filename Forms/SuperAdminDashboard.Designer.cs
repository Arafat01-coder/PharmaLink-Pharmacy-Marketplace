namespace PharmaLinkApp.Forms
{
    partial class SuperAdminDashboard
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
            panelSide = new Panel();
            lblBrand = new Label();
            lblRole = new Label();
            lblUserName = new Label();
            btnManagePharmacies = new Button();
            btnManageUsers = new Button();
            btnCategories = new Button();
            btnSalesReport = new Button();
            btnLowRated = new Button();
            btnModerateReviews = new Button();
            btnLogout = new Button();

            panelHeader = new Panel();
            lblHeaderTitle = new Label();
            lblHeaderSub = new Label();
            btnRefresh = new Button();

            lblPendingTitle = new Label();
            lblPendingHint = new Label();
            dgvPending = new DataGridView();
            btnApprove = new Button();
            btnReject = new Button();
            btnOpenPharmacies = new Button();

            lblLowRatedTitle = new Label();
            lblLowRatedHint = new Label();
            dgvLowRated = new DataGridView();

            panelSide.SuspendLayout();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPending).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvLowRated).BeginInit();
            SuspendLayout();
            //
            // panelSide
            //
            panelSide.Controls.Add(lblBrand);
            panelSide.Controls.Add(lblRole);
            panelSide.Controls.Add(lblUserName);
            panelSide.Controls.Add(btnManagePharmacies);
            panelSide.Controls.Add(btnManageUsers);
            panelSide.Controls.Add(btnCategories);
            panelSide.Controls.Add(btnSalesReport);
            panelSide.Controls.Add(btnLowRated);
            panelSide.Controls.Add(btnModerateReviews);
            panelSide.Controls.Add(btnLogout);
            panelSide.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panelSide.Location = new Point(0, 0);
            panelSide.Name = "panelSide";
            panelSide.Size = new Size(230, 700);
            panelSide.TabIndex = 0;
            //
            // lblBrand
            //
            lblBrand.AutoSize = true;
            lblBrand.Location = new Point(20, 22);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(140, 28);
            lblBrand.TabIndex = 0;
            lblBrand.Text = "PharmaLink";
            //
            // lblRole
            //
            lblRole.AutoSize = true;
            lblRole.Location = new Point(22, 54);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(100, 16);
            lblRole.TabIndex = 1;
            lblRole.Text = "SUPER ADMIN";
            //
            // lblUserName
            //
            lblUserName.AutoSize = false;
            lblUserName.Location = new Point(22, 74);
            lblUserName.Name = "lblUserName";
            lblUserName.Size = new Size(190, 18);
            lblUserName.TabIndex = 2;
            //
            // btnManagePharmacies
            //
            btnManagePharmacies.Location = new Point(0, 112);
            btnManagePharmacies.Name = "btnManagePharmacies";
            btnManagePharmacies.Size = new Size(230, 44);
            btnManagePharmacies.TabIndex = 3;
            btnManagePharmacies.Text = "Manage Pharmacies";
            btnManagePharmacies.Click += btnManagePharmacies_Click;
            //
            // btnManageUsers
            //
            btnManageUsers.Location = new Point(0, 158);
            btnManageUsers.Name = "btnManageUsers";
            btnManageUsers.Size = new Size(230, 44);
            btnManageUsers.TabIndex = 4;
            btnManageUsers.Text = "Manage Users";
            btnManageUsers.Click += btnManageUsers_Click;
            //
            // btnCategories
            //
            btnCategories.Location = new Point(0, 204);
            btnCategories.Name = "btnCategories";
            btnCategories.Size = new Size(230, 44);
            btnCategories.TabIndex = 5;
            btnCategories.Text = "Medicine Categories";
            btnCategories.Click += btnCategories_Click;
            //
            // btnSalesReport
            //
            btnSalesReport.Location = new Point(0, 250);
            btnSalesReport.Name = "btnSalesReport";
            btnSalesReport.Size = new Size(230, 44);
            btnSalesReport.TabIndex = 6;
            btnSalesReport.Text = "Sales && Commission";
            btnSalesReport.Click += btnSalesReport_Click;
            //
            // btnLowRated
            //
            btnLowRated.Location = new Point(0, 296);
            btnLowRated.Name = "btnLowRated";
            btnLowRated.Size = new Size(230, 44);
            btnLowRated.TabIndex = 7;
            btnLowRated.Text = "Low Rated Pharmacies";
            btnLowRated.Click += btnLowRated_Click;
            //
            // btnModerateReviews
            //
            btnModerateReviews.Location = new Point(0, 342);
            btnModerateReviews.Name = "btnModerateReviews";
            btnModerateReviews.Size = new Size(230, 44);
            btnModerateReviews.TabIndex = 8;
            btnModerateReviews.Text = "Moderate Reviews";
            btnModerateReviews.Click += btnModerateReviews_Click;
            //
            // btnLogout
            //
            btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLogout.Location = new Point(0, 636);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(230, 44);
            btnLogout.TabIndex = 9;
            btnLogout.Text = "Log out";
            btnLogout.Click += btnLogout_Click;
            //
            // panelHeader
            //
            panelHeader.Controls.Add(lblHeaderTitle);
            panelHeader.Controls.Add(lblHeaderSub);
            panelHeader.Controls.Add(btnRefresh);
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelHeader.Location = new Point(230, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1030, 68);
            panelHeader.TabIndex = 1;
            //
            // lblHeaderTitle
            //
            lblHeaderTitle.AutoSize = true;
            lblHeaderTitle.Location = new Point(20, 10);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(220, 30);
            lblHeaderTitle.TabIndex = 0;
            lblHeaderTitle.Text = "Platform Overview";
            //
            // lblHeaderSub
            //
            lblHeaderSub.AutoSize = true;
            lblHeaderSub.Location = new Point(23, 42);
            lblHeaderSub.Name = "lblHeaderSub";
            lblHeaderSub.Size = new Size(500, 16);
            lblHeaderSub.TabIndex = 1;
            lblHeaderSub.Text = "Approve pharmacies, watch the commission and act on poor service.";
            //
            // btnRefresh
            //
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(896, 16);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(110, 36);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Refresh";
            btnRefresh.Click += btnRefresh_Click;
            //
            // lblPendingTitle
            //
            lblPendingTitle.AutoSize = true;
            lblPendingTitle.Location = new Point(250, 192);
            lblPendingTitle.Name = "lblPendingTitle";
            lblPendingTitle.Size = new Size(300, 22);
            lblPendingTitle.TabIndex = 2;
            lblPendingTitle.Text = "Pharmacies waiting for approval";
            //
            // lblPendingHint
            //
            lblPendingHint.AutoSize = true;
            lblPendingHint.Location = new Point(252, 216);
            lblPendingHint.Name = "lblPendingHint";
            lblPendingHint.Size = new Size(600, 16);
            lblPendingHint.TabIndex = 3;
            lblPendingHint.Text = "Check the DGDA licence number before approving. Approve sets the shop to Approved and the owner's account to Active.";
            //
            // dgvPending
            //
            dgvPending.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvPending.Location = new Point(250, 238);
            dgvPending.Name = "dgvPending";
            dgvPending.Size = new Size(990, 176);
            dgvPending.TabIndex = 4;
            //
            // btnApprove
            //
            btnApprove.Location = new Point(250, 424);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(150, 36);
            btnApprove.TabIndex = 5;
            btnApprove.Text = "Approve selected";
            btnApprove.Click += btnApprove_Click;
            //
            // btnReject
            //
            btnReject.Location = new Point(410, 424);
            btnReject.Name = "btnReject";
            btnReject.Size = new Size(150, 36);
            btnReject.TabIndex = 6;
            btnReject.Text = "Reject selected";
            btnReject.Click += btnReject_Click;
            //
            // btnOpenPharmacies
            //
            btnOpenPharmacies.Location = new Point(570, 424);
            btnOpenPharmacies.Name = "btnOpenPharmacies";
            btnOpenPharmacies.Size = new Size(200, 36);
            btnOpenPharmacies.TabIndex = 7;
            btnOpenPharmacies.Text = "Open Manage Pharmacies";
            btnOpenPharmacies.Click += btnManagePharmacies_Click;
            //
            // lblLowRatedTitle
            //
            lblLowRatedTitle.AutoSize = true;
            lblLowRatedTitle.Location = new Point(250, 474);
            lblLowRatedTitle.Name = "lblLowRatedTitle";
            lblLowRatedTitle.Size = new Size(300, 22);
            lblLowRatedTitle.TabIndex = 8;
            lblLowRatedTitle.Text = "Pharmacies rated below 2.5";
            //
            // lblLowRatedHint
            //
            lblLowRatedHint.AutoSize = true;
            lblLowRatedHint.Location = new Point(252, 498);
            lblLowRatedHint.Name = "lblLowRatedHint";
            lblLowRatedHint.Size = new Size(600, 16);
            lblLowRatedHint.TabIndex = 9;
            lblLowRatedHint.Text = "Average rating below 2.5 from at least 2 visible reviews, so one angry review cannot put a shop on this list. Double click a row to open it.";
            //
            // dgvLowRated
            //
            dgvLowRated.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvLowRated.Location = new Point(250, 520);
            dgvLowRated.Name = "dgvLowRated";
            dgvLowRated.Size = new Size(990, 160);
            dgvLowRated.TabIndex = 10;
            dgvLowRated.CellDoubleClick += dgvLowRated_CellDoubleClick;
            //
            // SuperAdminDashboard
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1260, 700);
            Controls.Add(dgvLowRated);
            Controls.Add(lblLowRatedHint);
            Controls.Add(lblLowRatedTitle);
            Controls.Add(btnOpenPharmacies);
            Controls.Add(btnReject);
            Controls.Add(btnApprove);
            Controls.Add(dgvPending);
            Controls.Add(lblPendingHint);
            Controls.Add(lblPendingTitle);
            Controls.Add(panelHeader);
            Controls.Add(panelSide);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SuperAdminDashboard";
            Text = "PharmaLink - Super Admin";
            Load += SuperAdminDashboard_Load;
            panelSide.ResumeLayout(false);
            panelSide.PerformLayout();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPending).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvLowRated).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelSide;
        private Label lblBrand;
        private Label lblRole;
        private Label lblUserName;
        private Button btnManagePharmacies;
        private Button btnManageUsers;
        private Button btnCategories;
        private Button btnSalesReport;
        private Button btnLowRated;
        private Button btnModerateReviews;
        private Button btnLogout;

        private Panel panelHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSub;
        private Button btnRefresh;

        private Label lblPendingTitle;
        private Label lblPendingHint;
        private DataGridView dgvPending;
        private Button btnApprove;
        private Button btnReject;
        private Button btnOpenPharmacies;

        private Label lblLowRatedTitle;
        private Label lblLowRatedHint;
        private DataGridView dgvLowRated;
    }
}
