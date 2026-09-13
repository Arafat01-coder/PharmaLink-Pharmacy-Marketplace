namespace PharmaLinkApp.Forms
{
    partial class CustomerHomeForm
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
            btnBrowse = new Button();
            btnCart = new Button();
            btnOffers = new Button();
            btnOrders = new Button();
            btnMyAccount = new Button();
            btnLogout = new Button();

            panelHeader = new Panel();
            lblHeaderTitle = new Label();
            lblHeaderSub = new Label();
            lblCartSummary = new Label();

            lblSearch = new Label();
            txtSearch = new TextBox();
            btnSearch = new Button();
            btnClearFilters = new Button();

            lblCategory = new Label();
            cmbCategory = new ComboBox();
            lblPrice = new Label();
            cmbPriceRange = new ComboBox();
            lblArea = new Label();
            cmbArea = new ComboBox();
            lblPharmacy = new Label();
            cmbPharmacy = new ComboBox();
            lblAvailability = new Label();
            cmbAvailability = new ComboBox();

            dgvMedicines = new DataGridView();

            btnDetails = new Button();
            lblQuantity = new Label();
            txtQuantity = new TextBox();
            btnAddToCart = new Button();
            btnOpenCart = new Button();
            lblStatus = new Label();

            panelSide.SuspendLayout();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMedicines).BeginInit();
            SuspendLayout();
            //
            panelSide.Controls.Add(lblBrand);
            panelSide.Controls.Add(lblRole);
            panelSide.Controls.Add(lblUserName);
            panelSide.Controls.Add(btnBrowse);
            panelSide.Controls.Add(btnCart);
            panelSide.Controls.Add(btnOffers);
            panelSide.Controls.Add(btnOrders);
            panelSide.Controls.Add(btnMyAccount);
            panelSide.Controls.Add(btnLogout);
            panelSide.Location = new Point(0, 0);
            panelSide.Name = "panelSide";
            panelSide.Size = new Size(230, 700);
            panelSide.TabIndex = 0;
            //
            lblBrand.AutoSize = true;
            lblBrand.Location = new Point(20, 22);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(140, 28);
            lblBrand.TabIndex = 0;
            lblBrand.Text = "PharmaLink";
            //
            lblRole.AutoSize = true;
            lblRole.Location = new Point(22, 54);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(80, 16);
            lblRole.TabIndex = 1;
            lblRole.Text = "CUSTOMER";
            //
            lblUserName.AutoSize = false;
            lblUserName.Location = new Point(22, 74);
            lblUserName.Name = "lblUserName";
            lblUserName.Size = new Size(190, 18);
            lblUserName.TabIndex = 2;
            //
            btnBrowse.Location = new Point(0, 112);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(230, 44);
            btnBrowse.TabIndex = 3;
            btnBrowse.Text = "Browse Medicines";
            btnBrowse.Click += btnBrowse_Click;
            //
            btnCart.Location = new Point(0, 158);
            btnCart.Name = "btnCart";
            btnCart.Size = new Size(230, 44);
            btnCart.TabIndex = 4;
            btnCart.Text = "My Cart";
            btnCart.Click += btnCart_Click;
            //
            btnOffers.Location = new Point(0, 204);
            btnOffers.Name = "btnOffers";
            btnOffers.Size = new Size(230, 44);
            btnOffers.TabIndex = 5;
            btnOffers.Text = "Offers";
            btnOffers.Click += btnOffers_Click;
            //
            btnOrders.Location = new Point(0, 250);
            btnOrders.Name = "btnOrders";
            btnOrders.Size = new Size(230, 44);
            btnOrders.TabIndex = 6;
            btnOrders.Text = "My Orders";
            btnOrders.Click += btnOrders_Click;
            //
            btnMyAccount.Location = new Point(0, 296);
            btnMyAccount.Name = "btnMyAccount";
            btnMyAccount.Size = new Size(230, 44);
            btnMyAccount.TabIndex = 7;
            btnMyAccount.Text = "My Account && Password";
            btnMyAccount.Click += btnMyAccount_Click;
            //
            btnLogout.Location = new Point(0, 636);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(230, 44);
            btnLogout.TabIndex = 8;
            btnLogout.Text = "Log out";
            btnLogout.Click += btnLogout_Click;
            //
            panelHeader.Controls.Add(lblHeaderTitle);
            panelHeader.Controls.Add(lblHeaderSub);
            panelHeader.Controls.Add(lblCartSummary);
            panelHeader.Location = new Point(230, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1030, 68);
            panelHeader.TabIndex = 1;
            //
            lblHeaderTitle.AutoSize = true;
            lblHeaderTitle.Location = new Point(20, 10);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(300, 30);
            lblHeaderTitle.TabIndex = 0;
            lblHeaderTitle.Text = "Find your medicine";
            //
            lblHeaderSub.AutoSize = true;
            lblHeaderSub.Location = new Point(23, 42);
            lblHeaderSub.Name = "lblHeaderSub";
            lblHeaderSub.Size = new Size(600, 16);
            lblHeaderSub.TabIndex = 1;
            lblHeaderSub.Text = "Every licensed pharmacy on PharmaLink, searched at once.";
            //
            lblCartSummary.AutoSize = false;
            lblCartSummary.Location = new Point(760, 22);
            lblCartSummary.Name = "lblCartSummary";
            lblCartSummary.Size = new Size(250, 30);
            lblCartSummary.TextAlign = ContentAlignment.MiddleRight;
            lblCartSummary.TabIndex = 2;
            //
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(250, 92);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(50, 18);
            lblSearch.TabIndex = 2;
            lblSearch.Text = "Search";
            //
            txtSearch.Location = new Point(306, 88);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "brand name, generic name or manufacturer  -  try 'paracetamol'";
            txtSearch.Size = new Size(420, 27);
            txtSearch.TabIndex = 3;
            txtSearch.TextChanged += txtSearch_TextChanged;
            //
            btnSearch.Location = new Point(736, 86);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(100, 31);
            btnSearch.TabIndex = 4;
            btnSearch.Text = "Search";
            btnSearch.Click += btnSearch_Click;
            //
            btnClearFilters.Location = new Point(846, 86);
            btnClearFilters.Name = "btnClearFilters";
            btnClearFilters.Size = new Size(120, 31);
            btnClearFilters.TabIndex = 5;
            btnClearFilters.Text = "Clear filters";
            btnClearFilters.Click += btnClearFilters_Click;
            //
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(250, 132);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(64, 18);
            lblCategory.TabIndex = 6;
            lblCategory.Text = "Category";
            //
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Location = new Point(318, 128);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new Size(150, 27);
            cmbCategory.TabIndex = 7;
            cmbCategory.SelectedIndexChanged += Filter_Changed;
            //
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(478, 132);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(40, 18);
            lblPrice.TabIndex = 8;
            lblPrice.Text = "Price";
            //
            cmbPriceRange.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPriceRange.Location = new Point(520, 128);
            cmbPriceRange.Name = "cmbPriceRange";
            cmbPriceRange.Size = new Size(146, 27);
            cmbPriceRange.TabIndex = 9;
            cmbPriceRange.SelectedIndexChanged += Filter_Changed;
            //
            lblArea.AutoSize = true;
            lblArea.Location = new Point(676, 132);
            lblArea.Name = "lblArea";
            lblArea.Size = new Size(36, 18);
            lblArea.TabIndex = 10;
            lblArea.Text = "Area";
            //
            cmbArea.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbArea.Location = new Point(714, 128);
            cmbArea.Name = "cmbArea";
            cmbArea.Size = new Size(126, 27);
            cmbArea.TabIndex = 11;
            cmbArea.SelectedIndexChanged += Filter_Changed;
            //
            lblPharmacy.AutoSize = true;
            lblPharmacy.Location = new Point(850, 132);
            lblPharmacy.Name = "lblPharmacy";
            lblPharmacy.Size = new Size(68, 18);
            lblPharmacy.TabIndex = 12;
            lblPharmacy.Text = "Pharmacy";
            //
            cmbPharmacy.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPharmacy.Location = new Point(922, 128);
            cmbPharmacy.Name = "cmbPharmacy";
            cmbPharmacy.Size = new Size(160, 27);
            cmbPharmacy.TabIndex = 13;
            cmbPharmacy.SelectedIndexChanged += Filter_Changed;
            //
            lblAvailability.AutoSize = true;
            lblAvailability.Location = new Point(1092, 132);
            lblAvailability.Name = "lblAvailability";
            lblAvailability.Size = new Size(42, 18);
            lblAvailability.TabIndex = 14;
            lblAvailability.Text = "Stock";
            //
            cmbAvailability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAvailability.Location = new Point(1136, 128);
            cmbAvailability.Name = "cmbAvailability";
            cmbAvailability.Size = new Size(104, 27);
            cmbAvailability.TabIndex = 15;
            cmbAvailability.SelectedIndexChanged += Filter_Changed;
            //
            dgvMedicines.Location = new Point(250, 168);
            dgvMedicines.Name = "dgvMedicines";
            dgvMedicines.Size = new Size(990, 396);
            dgvMedicines.TabIndex = 16;
            dgvMedicines.SelectionChanged += dgvMedicines_SelectionChanged;
            dgvMedicines.CellDoubleClick += dgvMedicines_CellDoubleClick;
            //
            btnDetails.Location = new Point(250, 576);
            btnDetails.Name = "btnDetails";
            btnDetails.Size = new Size(190, 40);
            btnDetails.TabIndex = 17;
            btnDetails.Text = "Details && reviews";
            btnDetails.Click += btnDetails_Click;
            //
            lblQuantity.AutoSize = true;
            lblQuantity.Location = new Point(456, 586);
            lblQuantity.Name = "lblQuantity";
            lblQuantity.Size = new Size(64, 18);
            lblQuantity.TabIndex = 18;
            lblQuantity.Text = "Quantity";
            //
            txtQuantity.Location = new Point(524, 582);
            txtQuantity.Name = "txtQuantity";
            txtQuantity.Size = new Size(70, 27);
            txtQuantity.TabIndex = 19;
            txtQuantity.Text = "1";
            //
            btnAddToCart.Location = new Point(608, 576);
            btnAddToCart.Name = "btnAddToCart";
            btnAddToCart.Size = new Size(190, 40);
            btnAddToCart.TabIndex = 20;
            btnAddToCart.Text = "Add to cart";
            btnAddToCart.Click += btnAddToCart_Click;
            //
            btnOpenCart.Location = new Point(810, 576);
            btnOpenCart.Name = "btnOpenCart";
            btnOpenCart.Size = new Size(190, 40);
            btnOpenCart.TabIndex = 21;
            btnOpenCart.Text = "Go to cart";
            btnOpenCart.Click += btnCart_Click;
            //
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(250, 626);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(990, 56);
            lblStatus.TabIndex = 22;
            //
            // CustomerHomeForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1260, 700);
            Controls.Add(lblStatus);
            Controls.Add(btnOpenCart);
            Controls.Add(btnAddToCart);
            Controls.Add(txtQuantity);
            Controls.Add(lblQuantity);
            Controls.Add(btnDetails);
            Controls.Add(dgvMedicines);
            Controls.Add(cmbAvailability);
            Controls.Add(lblAvailability);
            Controls.Add(cmbPharmacy);
            Controls.Add(lblPharmacy);
            Controls.Add(cmbArea);
            Controls.Add(lblArea);
            Controls.Add(cmbPriceRange);
            Controls.Add(lblPrice);
            Controls.Add(cmbCategory);
            Controls.Add(lblCategory);
            Controls.Add(btnClearFilters);
            Controls.Add(btnSearch);
            Controls.Add(txtSearch);
            Controls.Add(lblSearch);
            Controls.Add(panelHeader);
            Controls.Add(panelSide);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "CustomerHomeForm";
            Text = "PharmaLink - Browse Medicines";
            Load += CustomerHomeForm_Load;
            // Anchors are set once every size above is final, so the form can be resized.
            panelSide.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblCartSummary.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dgvMedicines.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnDetails.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblQuantity.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtQuantity.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAddToCart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOpenCart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelSide.ResumeLayout(false);
            panelSide.PerformLayout();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMedicines).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelSide;
        private Label lblBrand;
        private Label lblRole;
        private Label lblUserName;
        private Button btnBrowse;
        private Button btnCart;
        private Button btnOffers;
        private Button btnOrders;
        private Button btnMyAccount;
        private Button btnLogout;

        private Panel panelHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSub;
        private Label lblCartSummary;

        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnClearFilters;
        private Label lblCategory;
        private ComboBox cmbCategory;
        private Label lblPrice;
        private ComboBox cmbPriceRange;
        private Label lblArea;
        private ComboBox cmbArea;
        private Label lblPharmacy;
        private ComboBox cmbPharmacy;
        private Label lblAvailability;
        private ComboBox cmbAvailability;

        private DataGridView dgvMedicines;
        private Button btnDetails;
        private Label lblQuantity;
        private TextBox txtQuantity;
        private Button btnAddToCart;
        private Button btnOpenCart;
        private Label lblStatus;
    }
}
