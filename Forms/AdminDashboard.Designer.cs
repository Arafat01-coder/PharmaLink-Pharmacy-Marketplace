namespace PharmaLinkApp.Forms
{
    partial class AdminDashboard
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
            lblShopName = new Label();
            btnMedicines = new Button();
            btnInventory = new Button();
            btnPrescriptions = new Button();
            btnEarnings = new Button();
            btnOffers = new Button();
            btnReviews = new Button();
            btnShopProfile = new Button();
            btnMyProfile = new Button();
            btnLogout = new Button();

            panelHeader = new Panel();
            lblHeaderTitle = new Label();
            lblHeaderSub = new Label();
            btnRefresh = new Button();

            panelWarning = new Panel();
            panelWarningStripe = new Panel();
            lblWarning = new Label();
            btnAcknowledgeWarning = new Button();

            lblOrdersTitle = new Label();
            lblOrdersHint = new Label();
            lblOrderStatusFilter = new Label();
            cmbOrderStatus = new ComboBox();
            dgvOrders = new DataGridView();
            btnConfirmOrder = new Button();
            btnDeliverOrder = new Button();
            btnCancelOrder = new Button();
            btnViewInvoice = new Button();
            lblOrderActionHint = new Label();

            lblLowStockTitle = new Label();
            dgvLowStock = new DataGridView();

            panelSide.SuspendLayout();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOrders).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvLowStock).BeginInit();
            SuspendLayout();
            //
            panelSide.Controls.Add(lblBrand);
            panelSide.Controls.Add(lblRole);
            panelSide.Controls.Add(lblShopName);
            panelSide.Controls.Add(btnMedicines);
            panelSide.Controls.Add(btnInventory);
            panelSide.Controls.Add(btnPrescriptions);
            panelSide.Controls.Add(btnEarnings);
            panelSide.Controls.Add(btnOffers);
            panelSide.Controls.Add(btnReviews);
            panelSide.Controls.Add(btnShopProfile);
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
            lblRole.Text = "PHARMACY OWNER";
            //
            lblShopName.AutoSize = false;
            lblShopName.Location = new Point(22, 70);
            lblShopName.Name = "lblShopName";
            lblShopName.Size = new Size(196, 34);
            lblShopName.TabIndex = 2;
            //
            btnMedicines.Location = new Point(0, 112);
            btnMedicines.Name = "btnMedicines";
            btnMedicines.Size = new Size(230, 42);
            btnMedicines.TabIndex = 3;
            btnMedicines.Text = "My Medicines";
            btnMedicines.Click += btnMedicines_Click;
            //
            btnInventory.Location = new Point(0, 156);
            btnInventory.Name = "btnInventory";
            btnInventory.Size = new Size(230, 42);
            btnInventory.TabIndex = 4;
            btnInventory.Text = "Stock && Inventory";
            btnInventory.Click += btnInventory_Click;
            //
            btnPrescriptions.Location = new Point(0, 200);
            btnPrescriptions.Name = "btnPrescriptions";
            btnPrescriptions.Size = new Size(230, 42);
            btnPrescriptions.TabIndex = 5;
            btnPrescriptions.Text = "Verify Prescriptions";
            btnPrescriptions.Click += btnPrescriptions_Click;
            //
            btnEarnings.Location = new Point(0, 244);
            btnEarnings.Name = "btnEarnings";
            btnEarnings.Size = new Size(230, 42);
            btnEarnings.TabIndex = 6;
            btnEarnings.Text = "Sales && Earnings";
            btnEarnings.Click += btnEarnings_Click;
            //
            btnOffers.Location = new Point(0, 288);
            btnOffers.Name = "btnOffers";
            btnOffers.Size = new Size(230, 42);
            btnOffers.TabIndex = 7;
            btnOffers.Text = "Discount Offers";
            btnOffers.Click += btnOffers_Click;
            //
            btnReviews.Location = new Point(0, 332);
            btnReviews.Name = "btnReviews";
            btnReviews.Size = new Size(230, 42);
            btnReviews.TabIndex = 8;
            btnReviews.Text = "Customer Reviews";
            btnReviews.Click += btnReviews_Click;
            //
            btnShopProfile.Location = new Point(0, 376);
            btnShopProfile.Name = "btnShopProfile";
            btnShopProfile.Size = new Size(230, 42);
            btnShopProfile.TabIndex = 9;
            btnShopProfile.Text = "My Pharmacy Profile";
            btnShopProfile.Click += btnShopProfile_Click;
            //
            btnMyProfile.Location = new Point(0, 420);
            btnMyProfile.Name = "btnMyProfile";
            btnMyProfile.Size = new Size(230, 42);
            btnMyProfile.TabIndex = 10;
            btnMyProfile.Text = "My Account && Password";
            btnMyProfile.Click += btnMyProfile_Click;
            //
            btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLogout.Location = new Point(0, 636);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(230, 44);
            btnLogout.TabIndex = 11;
            btnLogout.Text = "Log out";
            btnLogout.Click += btnLogout_Click;
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
            lblHeaderTitle.Text = "Pharmacy Dashboard";
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
            // panelWarning: the Super Admin's warning banner. Hidden until
            // LoadEverything finds an unread warning; AdminDashboard then places
            // it under the tiles and moves the orders section down to make room.
            panelWarning.Controls.Add(lblWarning);
            panelWarning.Controls.Add(btnAcknowledgeWarning);
            panelWarning.Controls.Add(panelWarningStripe);
            panelWarning.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelWarning.BorderStyle = BorderStyle.FixedSingle;
            panelWarning.Location = new Point(250, 178);
            panelWarning.Name = "panelWarning";
            panelWarning.Size = new Size(990, 56);
            panelWarning.TabIndex = 14;
            panelWarning.Visible = false;
            //
            panelWarningStripe.Dock = DockStyle.Left;
            panelWarningStripe.Location = new Point(0, 0);
            panelWarningStripe.Name = "panelWarningStripe";
            panelWarningStripe.Size = new Size(5, 54);
            panelWarningStripe.TabIndex = 0;
            //
            lblWarning.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblWarning.AutoSize = false;
            lblWarning.AutoEllipsis = true;
            lblWarning.Location = new Point(16, 6);
            lblWarning.Name = "lblWarning";
            lblWarning.Size = new Size(820, 42);
            lblWarning.TabIndex = 1;
            lblWarning.TextAlign = ContentAlignment.MiddleLeft;
            lblWarning.UseMnemonic = false;
            //
            btnAcknowledgeWarning.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAcknowledgeWarning.Location = new Point(846, 10);
            btnAcknowledgeWarning.Name = "btnAcknowledgeWarning";
            btnAcknowledgeWarning.Size = new Size(130, 34);
            btnAcknowledgeWarning.TabIndex = 2;
            btnAcknowledgeWarning.Text = "I've read this";
            btnAcknowledgeWarning.Click += btnAcknowledgeWarning_Click;
            //
            lblOrdersTitle.AutoSize = true;
            lblOrdersTitle.Location = new Point(250, 190);
            lblOrdersTitle.Name = "lblOrdersTitle";
            lblOrdersTitle.Size = new Size(200, 22);
            lblOrdersTitle.TabIndex = 2;
            lblOrdersTitle.Text = "Orders for my pharmacy";
            //
            lblOrdersHint.AutoSize = true;
            lblOrdersHint.Location = new Point(252, 214);
            lblOrdersHint.Name = "lblOrdersHint";
            lblOrdersHint.Size = new Size(600, 16);
            lblOrdersHint.TabIndex = 3;
            lblOrdersHint.Text = "Select an order to confirm it, mark it delivered, cancel it or open its invoice. Only your pharmacy's orders are listed.";
            //
            lblOrderStatusFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblOrderStatusFilter.AutoSize = true;
            lblOrderStatusFilter.Location = new Point(880, 190);
            lblOrderStatusFilter.Name = "lblOrderStatusFilter";
            lblOrderStatusFilter.Size = new Size(50, 18);
            lblOrderStatusFilter.TabIndex = 4;
            lblOrderStatusFilter.Text = "Status";
            //
            cmbOrderStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmbOrderStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOrderStatus.Location = new Point(936, 186);
            cmbOrderStatus.Name = "cmbOrderStatus";
            cmbOrderStatus.Size = new Size(304, 27);
            cmbOrderStatus.TabIndex = 5;
            cmbOrderStatus.SelectedIndexChanged += cmbOrderStatus_SelectedIndexChanged;
            //
            dgvOrders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvOrders.Location = new Point(250, 236);
            dgvOrders.Name = "dgvOrders";
            dgvOrders.Size = new Size(990, 198);
            dgvOrders.TabIndex = 6;
            dgvOrders.SelectionChanged += dgvOrders_SelectionChanged;
            //
            btnConfirmOrder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnConfirmOrder.Location = new Point(250, 444);
            btnConfirmOrder.Name = "btnConfirmOrder";
            btnConfirmOrder.Size = new Size(150, 36);
            btnConfirmOrder.TabIndex = 7;
            btnConfirmOrder.Text = "Confirm order";
            btnConfirmOrder.Click += btnConfirmOrder_Click;
            //
            btnDeliverOrder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeliverOrder.Location = new Point(410, 444);
            btnDeliverOrder.Name = "btnDeliverOrder";
            btnDeliverOrder.Size = new Size(150, 36);
            btnDeliverOrder.TabIndex = 8;
            btnDeliverOrder.Text = "Mark delivered";
            btnDeliverOrder.Click += btnDeliverOrder_Click;
            //
            btnCancelOrder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCancelOrder.Location = new Point(570, 444);
            btnCancelOrder.Name = "btnCancelOrder";
            btnCancelOrder.Size = new Size(150, 36);
            btnCancelOrder.TabIndex = 9;
            btnCancelOrder.Text = "Cancel order";
            btnCancelOrder.Click += btnCancelOrder_Click;
            //
            btnViewInvoice.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnViewInvoice.Location = new Point(730, 444);
            btnViewInvoice.Name = "btnViewInvoice";
            btnViewInvoice.Size = new Size(150, 36);
            btnViewInvoice.TabIndex = 10;
            btnViewInvoice.Text = "View invoice";
            btnViewInvoice.Click += btnViewInvoice_Click;
            //
            lblOrderActionHint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblOrderActionHint.AutoSize = false;
            lblOrderActionHint.Location = new Point(890, 444);
            lblOrderActionHint.Name = "lblOrderActionHint";
            lblOrderActionHint.Size = new Size(350, 36);
            lblOrderActionHint.TabIndex = 13;
            lblOrderActionHint.TextAlign = ContentAlignment.MiddleLeft;
            //
            lblLowStockTitle.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblLowStockTitle.AutoSize = true;
            lblLowStockTitle.Location = new Point(250, 494);
            lblLowStockTitle.Name = "lblLowStockTitle";
            lblLowStockTitle.Size = new Size(300, 22);
            lblLowStockTitle.TabIndex = 11;
            lblLowStockTitle.Text = "Low stock alert";
            //
            dgvLowStock.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvLowStock.Location = new Point(250, 520);
            dgvLowStock.Name = "dgvLowStock";
            dgvLowStock.Size = new Size(990, 160);
            dgvLowStock.TabIndex = 12;
            dgvLowStock.CellDoubleClick += dgvLowStock_CellDoubleClick;
            //
            // AdminDashboard
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1260, 700);
            Controls.Add(dgvLowStock);
            Controls.Add(lblLowStockTitle);
            Controls.Add(lblOrderActionHint);
            Controls.Add(btnViewInvoice);
            Controls.Add(btnCancelOrder);
            Controls.Add(btnDeliverOrder);
            Controls.Add(btnConfirmOrder);
            Controls.Add(dgvOrders);
            Controls.Add(cmbOrderStatus);
            Controls.Add(lblOrderStatusFilter);
            Controls.Add(lblOrdersHint);
            Controls.Add(lblOrdersTitle);
            Controls.Add(panelWarning);
            Controls.Add(panelHeader);
            Controls.Add(panelSide);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "AdminDashboard";
            Text = "PharmaLink - Pharmacy Owner";
            Load += AdminDashboard_Load;
            panelSide.ResumeLayout(false);
            panelSide.PerformLayout();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOrders).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvLowStock).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelSide;
        private Label lblBrand;
        private Label lblRole;
        private Label lblShopName;
        private Button btnMedicines;
        private Button btnInventory;
        private Button btnPrescriptions;
        private Button btnEarnings;
        private Button btnOffers;
        private Button btnReviews;
        private Button btnShopProfile;
        private Button btnMyProfile;
        private Button btnLogout;

        private Panel panelHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSub;
        private Button btnRefresh;

        private Panel panelWarning;
        private Panel panelWarningStripe;
        private Label lblWarning;
        private Button btnAcknowledgeWarning;

        private Label lblOrdersTitle;
        private Label lblOrdersHint;
        private Label lblOrderStatusFilter;
        private ComboBox cmbOrderStatus;
        private DataGridView dgvOrders;
        private Button btnConfirmOrder;
        private Button btnDeliverOrder;
        private Button btnCancelOrder;
        private Button btnViewInvoice;
        private Label lblOrderActionHint;

        private Label lblLowStockTitle;
        private DataGridView dgvLowStock;
    }
}
