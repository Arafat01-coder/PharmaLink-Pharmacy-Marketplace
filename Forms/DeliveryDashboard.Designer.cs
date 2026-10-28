namespace PharmaLinkApp.Forms
{
    partial class DeliveryDashboard
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
            panelSide = new Panel();
            lblBrand = new Label();
            lblRole = new Label();
            lblUserName = new Label();
            btnAvailable = new Button();
            btnMyDeliveries = new Button();
            btnHistory = new Button();
            btnMyProfile = new Button();
            btnLogout = new Button();

            panelHeader = new Panel();
            lblHeaderTitle = new Label();
            lblHeaderSub = new Label();
            btnRefresh = new Button();

            lblSectionTitle = new Label();
            lblSectionHint = new Label();
            lblFrom = new Label();
            dtpFrom = new DateTimePicker();
            lblTo = new Label();
            dtpTo = new DateTimePicker();
            btnShowHistory = new Button();
            dgvJobs = new DataGridView();

            btnAccept = new Button();
            btnPickedUp = new Button();
            btnDelivered = new Button();
            btnRelease = new Button();
            lblActionHint = new Label();

            panelSide.SuspendLayout();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvJobs).BeginInit();
            SuspendLayout();
            //
            // sidebar
            //
            panelSide.Controls.Add(lblBrand);
            panelSide.Controls.Add(lblRole);
            panelSide.Controls.Add(lblUserName);
            panelSide.Controls.Add(btnAvailable);
            panelSide.Controls.Add(btnMyDeliveries);
            panelSide.Controls.Add(btnHistory);
            panelSide.Controls.Add(btnMyProfile);
            panelSide.Controls.Add(btnLogout);
            panelSide.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panelSide.Location = new Point(0, 0);
            panelSide.Name = "panelSide";
            panelSide.Size = new Size(230, 700);
            panelSide.TabIndex = 0;
            //
            lblBrand.AutoSize = true;
            lblBrand.Location = new Point(20, 20);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(140, 28);
            lblBrand.TabIndex = 0;
            lblBrand.Text = "PharmaLink";
            //
            lblRole.AutoSize = true;
            lblRole.Location = new Point(22, 50);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(120, 16);
            lblRole.TabIndex = 1;
            lblRole.Text = "DELIVERY MAN";
            //
            lblUserName.AutoSize = false;
            lblUserName.Location = new Point(22, 70);
            lblUserName.Name = "lblUserName";
            lblUserName.Size = new Size(196, 34);
            lblUserName.TabIndex = 2;
            //
            btnAvailable.Location = new Point(0, 112);
            btnAvailable.Name = "btnAvailable";
            btnAvailable.Size = new Size(230, 42);
            btnAvailable.TabIndex = 3;
            btnAvailable.Text = "Available Orders";
            btnAvailable.Click += btnAvailable_Click;
            //
            btnMyDeliveries.Location = new Point(0, 156);
            btnMyDeliveries.Name = "btnMyDeliveries";
            btnMyDeliveries.Size = new Size(230, 42);
            btnMyDeliveries.TabIndex = 4;
            btnMyDeliveries.Text = "My Deliveries";
            btnMyDeliveries.Click += btnMyDeliveries_Click;
            //
            btnHistory.Location = new Point(0, 200);
            btnHistory.Name = "btnHistory";
            btnHistory.Size = new Size(230, 42);
            btnHistory.TabIndex = 5;
            btnHistory.Text = "Delivery History";
            btnHistory.Click += btnHistory_Click;
            //
            btnMyProfile.Location = new Point(0, 244);
            btnMyProfile.Name = "btnMyProfile";
            btnMyProfile.Size = new Size(230, 42);
            btnMyProfile.TabIndex = 6;
            btnMyProfile.Text = "My Account && Password";
            btnMyProfile.Click += btnMyProfile_Click;
            //
            btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLogout.Location = new Point(0, 636);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(230, 44);
            btnLogout.TabIndex = 7;
            btnLogout.Text = "Log out";
            btnLogout.Click += btnLogout_Click;
            //
            // header
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
            lblHeaderTitle.AutoSize = true;
            lblHeaderTitle.Location = new Point(20, 10);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(240, 30);
            lblHeaderTitle.TabIndex = 0;
            lblHeaderTitle.Text = "Delivery Dashboard";
            //
            lblHeaderSub.AutoSize = true;
            lblHeaderSub.Location = new Point(23, 42);
            lblHeaderSub.Name = "lblHeaderSub";
            lblHeaderSub.Size = new Size(600, 16);
            lblHeaderSub.TabIndex = 1;
            //
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(896, 16);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(110, 36);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Refresh";
            btnRefresh.Click += btnRefresh_Click;
            //
            // section heading and history filter
            //
            lblSectionTitle.AutoSize = true;
            lblSectionTitle.Location = new Point(254, 190);
            lblSectionTitle.Name = "lblSectionTitle";
            lblSectionTitle.Size = new Size(200, 22);
            lblSectionTitle.TabIndex = 2;
            lblSectionTitle.Text = "Available orders";
            //
            lblSectionHint.AutoSize = false;
            lblSectionHint.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblSectionHint.Location = new Point(254, 214);
            lblSectionHint.Name = "lblSectionHint";
            lblSectionHint.Size = new Size(560, 18);
            lblSectionHint.TabIndex = 3;
            //
            lblFrom.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblFrom.AutoSize = true;
            lblFrom.Location = new Point(830, 199);
            lblFrom.Name = "lblFrom";
            lblFrom.Size = new Size(40, 18);
            lblFrom.TabIndex = 4;
            lblFrom.Text = "From";
            //
            dtpFrom.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dtpFrom.Format = DateTimePickerFormat.Custom;
            dtpFrom.CustomFormat = "dd MMM yyyy";
            dtpFrom.Location = new Point(872, 195);
            dtpFrom.Name = "dtpFrom";
            dtpFrom.Size = new Size(118, 27);
            dtpFrom.TabIndex = 5;
            //
            lblTo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTo.AutoSize = true;
            lblTo.Location = new Point(996, 199);
            lblTo.Name = "lblTo";
            lblTo.Size = new Size(22, 18);
            lblTo.TabIndex = 6;
            lblTo.Text = "To";
            //
            dtpTo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dtpTo.Format = DateTimePickerFormat.Custom;
            dtpTo.CustomFormat = "dd MMM yyyy";
            dtpTo.Location = new Point(1020, 195);
            dtpTo.Name = "dtpTo";
            dtpTo.Size = new Size(118, 27);
            dtpTo.TabIndex = 7;
            //
            btnShowHistory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnShowHistory.Location = new Point(1146, 192);
            btnShowHistory.Name = "btnShowHistory";
            btnShowHistory.Size = new Size(90, 33);
            btnShowHistory.TabIndex = 8;
            btnShowHistory.Text = "Show";
            btnShowHistory.Click += btnShowHistory_Click;
            //
            // grid
            //
            dgvJobs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvJobs.Location = new Point(254, 240);
            dgvJobs.Name = "dgvJobs";
            dgvJobs.Size = new Size(982, 360);
            dgvJobs.TabIndex = 9;
            dgvJobs.SelectionChanged += dgvJobs_SelectionChanged;
            //
            // actions
            //
            btnAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAccept.Location = new Point(254, 616);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(170, 42);
            btnAccept.TabIndex = 10;
            btnAccept.Text = "Accept order";
            btnAccept.Click += btnAccept_Click;
            //
            btnPickedUp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPickedUp.Location = new Point(254, 616);
            btnPickedUp.Name = "btnPickedUp";
            btnPickedUp.Size = new Size(170, 42);
            btnPickedUp.TabIndex = 11;
            btnPickedUp.Text = "Picked up";
            btnPickedUp.Click += btnPickedUp_Click;
            //
            btnDelivered.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelivered.Location = new Point(434, 616);
            btnDelivered.Name = "btnDelivered";
            btnDelivered.Size = new Size(170, 42);
            btnDelivered.TabIndex = 12;
            btnDelivered.Text = "Delivered";
            btnDelivered.Click += btnDelivered_Click;
            //
            btnRelease.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRelease.Location = new Point(614, 616);
            btnRelease.Name = "btnRelease";
            btnRelease.Size = new Size(170, 42);
            btnRelease.TabIndex = 13;
            btnRelease.Text = "Give back";
            btnRelease.Click += btnRelease_Click;
            //
            lblActionHint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblActionHint.AutoSize = false;
            lblActionHint.Location = new Point(796, 616);
            lblActionHint.Name = "lblActionHint";
            lblActionHint.Size = new Size(440, 42);
            lblActionHint.TabIndex = 14;
            lblActionHint.TextAlign = ContentAlignment.MiddleLeft;
            //
            // DeliveryDashboard
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1260, 680);
            Controls.Add(lblActionHint);
            Controls.Add(btnRelease);
            Controls.Add(btnDelivered);
            Controls.Add(btnPickedUp);
            Controls.Add(btnAccept);
            Controls.Add(dgvJobs);
            Controls.Add(btnShowHistory);
            Controls.Add(dtpTo);
            Controls.Add(lblTo);
            Controls.Add(dtpFrom);
            Controls.Add(lblFrom);
            Controls.Add(lblSectionHint);
            Controls.Add(lblSectionTitle);
            Controls.Add(panelHeader);
            Controls.Add(panelSide);
            Name = "DeliveryDashboard";
            Text = "PharmaLink  -  Delivery";
            Load += DeliveryDashboard_Load;
            panelSide.ResumeLayout(false);
            panelSide.PerformLayout();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvJobs).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelSide;
        private Label lblBrand;
        private Label lblRole;
        private Label lblUserName;
        private Button btnAvailable;
        private Button btnMyDeliveries;
        private Button btnHistory;
        private Button btnMyProfile;
        private Button btnLogout;

        private Panel panelHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSub;
        private Button btnRefresh;

        private Label lblSectionTitle;
        private Label lblSectionHint;
        private Label lblFrom;
        private DateTimePicker dtpFrom;
        private Label lblTo;
        private DateTimePicker dtpTo;
        private Button btnShowHistory;
        private DataGridView dgvJobs;

        private Button btnAccept;
        private Button btnPickedUp;
        private Button btnDelivered;
        private Button btnRelease;
        private Label lblActionHint;
    }
}
