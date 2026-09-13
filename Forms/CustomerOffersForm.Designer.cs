namespace PharmaLinkApp.Forms
{
    partial class CustomerOffersForm
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

            lblCategory = new Label();
            cmbCategory = new ComboBox();
            lblArea = new Label();
            cmbArea = new ComboBox();
            btnRefresh = new Button();
            lblToday = new Label();

            dgvOffers = new DataGridView();

            lblQuantity = new Label();
            txtQuantity = new TextBox();
            btnAddToCart = new Button();
            btnViewDetails = new Button();
            lblNote = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOffers).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1180, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Offers";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Discounts running today, with the price you will actually pay already worked out.";
            //
            btnBack.Location = new Point(1044, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(20, 92);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(64, 18);
            lblCategory.TabIndex = 1;
            lblCategory.Text = "Category";
            //
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Location = new Point(90, 88);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new Size(200, 27);
            cmbCategory.TabIndex = 2;
            cmbCategory.SelectedIndexChanged += Filter_Changed;
            //
            lblArea.AutoSize = true;
            lblArea.Location = new Point(306, 92);
            lblArea.Name = "lblArea";
            lblArea.Size = new Size(36, 18);
            lblArea.TabIndex = 3;
            lblArea.Text = "Area";
            //
            cmbArea.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbArea.Location = new Point(348, 88);
            cmbArea.Name = "cmbArea";
            cmbArea.Size = new Size(180, 27);
            cmbArea.TabIndex = 4;
            cmbArea.SelectedIndexChanged += Filter_Changed;
            //
            btnRefresh.Location = new Point(544, 86);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(110, 31);
            btnRefresh.TabIndex = 5;
            btnRefresh.Text = "Refresh";
            btnRefresh.Click += Filter_Changed;
            //
            lblToday.AutoSize = false;
            lblToday.Location = new Point(680, 90);
            lblToday.Name = "lblToday";
            lblToday.Size = new Size(474, 24);
            lblToday.TextAlign = ContentAlignment.MiddleRight;
            lblToday.TabIndex = 6;
            //
            dgvOffers.Location = new Point(20, 130);
            dgvOffers.Name = "dgvOffers";
            dgvOffers.Size = new Size(1134, 380);
            dgvOffers.TabIndex = 7;
            dgvOffers.SelectionChanged += dgvOffers_SelectionChanged;
            dgvOffers.CellDoubleClick += dgvOffers_CellDoubleClick;
            //
            lblQuantity.AutoSize = true;
            lblQuantity.Location = new Point(20, 534);
            lblQuantity.Name = "lblQuantity";
            lblQuantity.Size = new Size(64, 18);
            lblQuantity.TabIndex = 8;
            lblQuantity.Text = "Quantity";
            //
            txtQuantity.Location = new Point(90, 530);
            txtQuantity.Name = "txtQuantity";
            txtQuantity.Size = new Size(70, 27);
            txtQuantity.TabIndex = 9;
            txtQuantity.Text = "1";
            //
            btnAddToCart.Location = new Point(176, 526);
            btnAddToCart.Name = "btnAddToCart";
            btnAddToCart.Size = new Size(210, 40);
            btnAddToCart.TabIndex = 10;
            btnAddToCart.Text = "Add to cart at this price";
            btnAddToCart.Click += btnAddToCart_Click;
            //
            btnViewDetails.Location = new Point(396, 526);
            btnViewDetails.Name = "btnViewDetails";
            btnViewDetails.Size = new Size(210, 40);
            btnViewDetails.TabIndex = 11;
            btnViewDetails.Text = "Details && reviews";
            btnViewDetails.Click += btnViewDetails_Click;
            //
            lblNote.AutoSize = false;
            lblNote.Location = new Point(624, 522);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(530, 48);
            lblNote.TabIndex = 12;
            lblNote.Text = "Prices shown already include the discount, and you pay the same price in your cart and on your invoice. Each offer ends on its 'Valid until' date.";
            //
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 578);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1134, 36);
            lblStatus.TabIndex = 13;
            //
            // CustomerOffersForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1180, 626);
            Controls.Add(lblStatus);
            Controls.Add(lblNote);
            Controls.Add(btnViewDetails);
            Controls.Add(btnAddToCart);
            Controls.Add(txtQuantity);
            Controls.Add(lblQuantity);
            Controls.Add(dgvOffers);
            Controls.Add(lblToday);
            Controls.Add(btnRefresh);
            Controls.Add(cmbArea);
            Controls.Add(lblArea);
            Controls.Add(cmbCategory);
            Controls.Add(lblCategory);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "CustomerOffersForm";
            Text = "PharmaLink - Offers and Packages";
            Load += CustomerOffersForm_Load;
            // Anchors are set once every size above is final, so the form can be resized.
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblToday.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvOffers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblQuantity.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtQuantity.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAddToCart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnViewDetails.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOffers).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private Label lblCategory;
        private ComboBox cmbCategory;
        private Label lblArea;
        private ComboBox cmbArea;
        private Button btnRefresh;
        private Label lblToday;
        private DataGridView dgvOffers;
        private Label lblQuantity;
        private TextBox txtQuantity;
        private Button btnAddToCart;
        private Button btnViewDetails;
        private Label lblNote;
        private Label lblStatus;
    }
}
