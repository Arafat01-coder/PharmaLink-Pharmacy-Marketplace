namespace PharmaLinkApp.Forms
{
    partial class CartForm
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

            dgvCart = new DataGridView();
            lblQuantity = new Label();
            txtQuantity = new TextBox();
            btnUpdateQuantity = new Button();
            btnRemoveLine = new Button();
            btnClearCart = new Button();
            lblSplitNote = new Label();

            grpSummary = new GroupBox();
            lblItemsCaption = new Label();
            lblItemsValue = new Label();
            lblDiscountCaption = new Label();
            lblDiscountValue = new Label();
            lblDeliveryCaption = new Label();
            lblDeliveryValue = new Label();
            lblPayableCaption = new Label();
            lblPayableValue = new Label();
            lblSummaryNote = new Label();

            btnCheckout = new Button();
            btnContinueShopping = new Button();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            grpSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCart).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1160, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(160, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "My Cart";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(600, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Change your mind before you pay. Prices already include today's offers.";
            //
            btnBack.Location = new Point(1024, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            dgvCart.Location = new Point(20, 88);
            dgvCart.Name = "dgvCart";
            dgvCart.Size = new Size(800, 356);
            dgvCart.TabIndex = 1;
            dgvCart.SelectionChanged += dgvCart_SelectionChanged;
            //
            lblQuantity.AutoSize = true;
            lblQuantity.Location = new Point(20, 464);
            lblQuantity.Name = "lblQuantity";
            lblQuantity.Size = new Size(64, 18);
            lblQuantity.TabIndex = 2;
            lblQuantity.Text = "Quantity";
            //
            txtQuantity.Location = new Point(88, 460);
            txtQuantity.Name = "txtQuantity";
            txtQuantity.Size = new Size(60, 27);
            txtQuantity.TabIndex = 3;
            //
            btnUpdateQuantity.Location = new Point(158, 456);
            btnUpdateQuantity.Name = "btnUpdateQuantity";
            btnUpdateQuantity.Size = new Size(150, 36);
            btnUpdateQuantity.TabIndex = 4;
            btnUpdateQuantity.Text = "Update quantity";
            btnUpdateQuantity.Click += btnUpdateQuantity_Click;
            //
            btnRemoveLine.Location = new Point(318, 456);
            btnRemoveLine.Name = "btnRemoveLine";
            btnRemoveLine.Size = new Size(150, 36);
            btnRemoveLine.TabIndex = 5;
            btnRemoveLine.Text = "Remove line";
            btnRemoveLine.Click += btnRemoveLine_Click;
            //
            btnClearCart.Location = new Point(478, 456);
            btnClearCart.Name = "btnClearCart";
            btnClearCart.Size = new Size(150, 36);
            btnClearCart.TabIndex = 6;
            btnClearCart.Text = "Empty the cart";
            btnClearCart.Click += btnClearCart_Click;
            //
            lblSplitNote.AutoSize = false;
            lblSplitNote.Location = new Point(20, 502);
            lblSplitNote.Name = "lblSplitNote";
            lblSplitNote.Size = new Size(800, 74);
            lblSplitNote.TabIndex = 7;
            //
            grpSummary.Controls.Add(lblItemsCaption);
            grpSummary.Controls.Add(lblItemsValue);
            grpSummary.Controls.Add(lblDiscountCaption);
            grpSummary.Controls.Add(lblDiscountValue);
            grpSummary.Controls.Add(lblDeliveryCaption);
            grpSummary.Controls.Add(lblDeliveryValue);
            grpSummary.Controls.Add(lblPayableCaption);
            grpSummary.Controls.Add(lblPayableValue);
            grpSummary.Controls.Add(lblSummaryNote);
            grpSummary.Location = new Point(840, 88);
            grpSummary.Name = "grpSummary";
            grpSummary.Size = new Size(294, 356);
            grpSummary.TabIndex = 8;
            grpSummary.TabStop = false;
            grpSummary.Text = "  What you will pay  ";
            //
            lblItemsCaption.AutoSize = true;
            lblItemsCaption.Location = new Point(18, 42);
            lblItemsCaption.Name = "lblItemsCaption";
            lblItemsCaption.Size = new Size(100, 18);
            lblItemsCaption.TabIndex = 0;
            lblItemsCaption.Text = "Items total";
            //
            lblItemsValue.AutoSize = false;
            lblItemsValue.Location = new Point(140, 42);
            lblItemsValue.Name = "lblItemsValue";
            lblItemsValue.Size = new Size(136, 20);
            lblItemsValue.TextAlign = ContentAlignment.MiddleRight;
            lblItemsValue.TabIndex = 1;
            //
            lblDiscountCaption.AutoSize = true;
            lblDiscountCaption.Location = new Point(18, 78);
            lblDiscountCaption.Name = "lblDiscountCaption";
            lblDiscountCaption.Size = new Size(120, 18);
            lblDiscountCaption.TabIndex = 2;
            lblDiscountCaption.Text = "Offer discount";
            //
            lblDiscountValue.AutoSize = false;
            lblDiscountValue.Location = new Point(140, 78);
            lblDiscountValue.Name = "lblDiscountValue";
            lblDiscountValue.Size = new Size(136, 20);
            lblDiscountValue.TextAlign = ContentAlignment.MiddleRight;
            lblDiscountValue.TabIndex = 3;
            //
            lblDeliveryCaption.AutoSize = true;
            lblDeliveryCaption.Location = new Point(18, 114);
            lblDeliveryCaption.Name = "lblDeliveryCaption";
            lblDeliveryCaption.Size = new Size(120, 18);
            lblDeliveryCaption.TabIndex = 4;
            lblDeliveryCaption.Text = "Delivery charge";
            //
            lblDeliveryValue.AutoSize = false;
            lblDeliveryValue.Location = new Point(140, 114);
            lblDeliveryValue.Name = "lblDeliveryValue";
            lblDeliveryValue.Size = new Size(136, 20);
            lblDeliveryValue.TextAlign = ContentAlignment.MiddleRight;
            lblDeliveryValue.TabIndex = 5;
            //
            lblPayableCaption.AutoSize = true;
            lblPayableCaption.Location = new Point(18, 158);
            lblPayableCaption.Name = "lblPayableCaption";
            lblPayableCaption.Size = new Size(100, 22);
            lblPayableCaption.TabIndex = 6;
            lblPayableCaption.Text = "Payable";
            //
            lblPayableValue.AutoSize = false;
            lblPayableValue.Location = new Point(100, 152);
            lblPayableValue.Name = "lblPayableValue";
            lblPayableValue.Size = new Size(176, 34);
            lblPayableValue.TextAlign = ContentAlignment.MiddleRight;
            lblPayableValue.TabIndex = 7;
            //
            lblSummaryNote.AutoSize = false;
            lblSummaryNote.Location = new Point(18, 198);
            lblSummaryNote.Name = "lblSummaryNote";
            lblSummaryNote.Size = new Size(258, 144);
            lblSummaryNote.TabIndex = 8;
            //
            btnCheckout.Location = new Point(840, 456);
            btnCheckout.Name = "btnCheckout";
            btnCheckout.Size = new Size(294, 48);
            btnCheckout.TabIndex = 9;
            btnCheckout.Text = "Proceed to checkout";
            btnCheckout.Click += btnCheckout_Click;
            //
            btnContinueShopping.Location = new Point(840, 512);
            btnContinueShopping.Name = "btnContinueShopping";
            btnContinueShopping.Size = new Size(294, 38);
            btnContinueShopping.TabIndex = 10;
            btnContinueShopping.Text = "Continue shopping";
            btnContinueShopping.Click += btnBack_Click;
            //
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 586);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1114, 36);
            lblStatus.TabIndex = 11;
            //
            // CartForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1160, 630);
            Controls.Add(lblStatus);
            Controls.Add(btnContinueShopping);
            Controls.Add(btnCheckout);
            Controls.Add(grpSummary);
            Controls.Add(lblSplitNote);
            Controls.Add(btnClearCart);
            Controls.Add(btnRemoveLine);
            Controls.Add(btnUpdateQuantity);
            Controls.Add(txtQuantity);
            Controls.Add(lblQuantity);
            Controls.Add(dgvCart);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "CartForm";
            Text = "PharmaLink - My Cart";
            Load += CartForm_Load;
            // Anchors are set once every size above is final, so the form can be resized.
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dgvCart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblQuantity.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtQuantity.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUpdateQuantity.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRemoveLine.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnClearCart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblSplitNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpSummary.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCheckout.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnContinueShopping.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            grpSummary.ResumeLayout(false);
            grpSummary.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCart).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private DataGridView dgvCart;
        private Label lblQuantity;
        private TextBox txtQuantity;
        private Button btnUpdateQuantity;
        private Button btnRemoveLine;
        private Button btnClearCart;
        private Label lblSplitNote;
        private GroupBox grpSummary;
        private Label lblItemsCaption;
        private Label lblItemsValue;
        private Label lblDiscountCaption;
        private Label lblDiscountValue;
        private Label lblDeliveryCaption;
        private Label lblDeliveryValue;
        private Label lblPayableCaption;
        private Label lblPayableValue;
        private Label lblSummaryNote;
        private Button btnCheckout;
        private Button btnContinueShopping;
        private Label lblStatus;
    }
}
