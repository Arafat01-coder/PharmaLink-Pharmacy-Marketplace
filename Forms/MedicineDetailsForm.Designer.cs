namespace PharmaLinkApp.Forms
{
    partial class MedicineDetailsForm
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
            lblMedicineName = new Label();
            lblGenericName = new Label();
            btnBack = new Button();

            grpFacts = new GroupBox();
            lblManufacturerCaption = new Label();
            lblManufacturer = new Label();
            lblStrengthCaption = new Label();
            lblStrength = new Label();
            lblCategoryCaption = new Label();
            lblCategory = new Label();
            lblExpiryCaption = new Label();
            lblExpiry = new Label();
            lblPharmacyCaption = new Label();
            lblPharmacy = new Label();
            lblStockCaption = new Label();
            lblStock = new Label();
            lblRxBadge = new Label();
            lblDescription = new Label();

            grpPrice = new GroupBox();
            lblOriginalPrice = new Label();
            lblFinalPrice = new Label();
            lblDiscountBadge = new Label();
            lblQuantity = new Label();
            txtQuantity = new TextBox();
            btnAddToCart = new Button();
            lblAddMessage = new Label();

            lblReviewsTitle = new Label();
            lblAverageRating = new Label();
            dgvReviews = new DataGridView();
            lblReviewNote = new Label();

            panelHeader.SuspendLayout();
            grpFacts.SuspendLayout();
            grpPrice.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReviews).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblMedicineName);
            panelHeader.Controls.Add(lblGenericName);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(960, 68);
            panelHeader.TabIndex = 0;
            //
            lblMedicineName.AutoSize = true;
            lblMedicineName.Location = new Point(20, 8);
            lblMedicineName.Name = "lblMedicineName";
            lblMedicineName.Size = new Size(300, 32);
            lblMedicineName.TabIndex = 0;
            //
            lblGenericName.AutoSize = true;
            lblGenericName.Location = new Point(23, 42);
            lblGenericName.Name = "lblGenericName";
            lblGenericName.Size = new Size(400, 18);
            lblGenericName.TabIndex = 1;
            //
            btnBack.Location = new Point(824, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            grpFacts.Controls.Add(lblManufacturerCaption);
            grpFacts.Controls.Add(lblManufacturer);
            grpFacts.Controls.Add(lblStrengthCaption);
            grpFacts.Controls.Add(lblStrength);
            grpFacts.Controls.Add(lblCategoryCaption);
            grpFacts.Controls.Add(lblCategory);
            grpFacts.Controls.Add(lblExpiryCaption);
            grpFacts.Controls.Add(lblExpiry);
            grpFacts.Controls.Add(lblPharmacyCaption);
            grpFacts.Controls.Add(lblPharmacy);
            grpFacts.Controls.Add(lblStockCaption);
            grpFacts.Controls.Add(lblStock);
            grpFacts.Controls.Add(lblRxBadge);
            grpFacts.Controls.Add(lblDescription);
            grpFacts.Location = new Point(20, 84);
            grpFacts.Name = "grpFacts";
            grpFacts.Size = new Size(600, 280);
            grpFacts.TabIndex = 1;
            grpFacts.TabStop = false;
            grpFacts.Text = "  About this medicine  ";
            //
            lblManufacturerCaption.AutoSize = true;
            lblManufacturerCaption.Location = new Point(18, 34);
            lblManufacturerCaption.Name = "lblManufacturerCaption";
            lblManufacturerCaption.Size = new Size(110, 16);
            lblManufacturerCaption.TabIndex = 0;
            lblManufacturerCaption.Text = "MANUFACTURER";
            //
            lblManufacturer.AutoSize = true;
            lblManufacturer.Location = new Point(18, 52);
            lblManufacturer.Name = "lblManufacturer";
            lblManufacturer.Size = new Size(240, 20);
            lblManufacturer.TabIndex = 1;
            //
            lblStrengthCaption.AutoSize = true;
            lblStrengthCaption.Location = new Point(310, 34);
            lblStrengthCaption.Name = "lblStrengthCaption";
            lblStrengthCaption.Size = new Size(80, 16);
            lblStrengthCaption.TabIndex = 2;
            lblStrengthCaption.Text = "STRENGTH";
            //
            lblStrength.AutoSize = true;
            lblStrength.Location = new Point(310, 52);
            lblStrength.Name = "lblStrength";
            lblStrength.Size = new Size(240, 20);
            lblStrength.TabIndex = 3;
            //
            lblCategoryCaption.AutoSize = true;
            lblCategoryCaption.Location = new Point(18, 88);
            lblCategoryCaption.Name = "lblCategoryCaption";
            lblCategoryCaption.Size = new Size(80, 16);
            lblCategoryCaption.TabIndex = 4;
            lblCategoryCaption.Text = "CATEGORY";
            //
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(18, 106);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(240, 20);
            lblCategory.TabIndex = 5;
            //
            lblExpiryCaption.AutoSize = true;
            lblExpiryCaption.Location = new Point(310, 88);
            lblExpiryCaption.Name = "lblExpiryCaption";
            lblExpiryCaption.Size = new Size(90, 16);
            lblExpiryCaption.TabIndex = 6;
            lblExpiryCaption.Text = "EXPIRY DATE";
            //
            lblExpiry.AutoSize = true;
            lblExpiry.Location = new Point(310, 106);
            lblExpiry.Name = "lblExpiry";
            lblExpiry.Size = new Size(240, 20);
            lblExpiry.TabIndex = 7;
            //
            lblPharmacyCaption.AutoSize = true;
            lblPharmacyCaption.Location = new Point(18, 142);
            lblPharmacyCaption.Name = "lblPharmacyCaption";
            lblPharmacyCaption.Size = new Size(110, 16);
            lblPharmacyCaption.TabIndex = 8;
            lblPharmacyCaption.Text = "SELLING PHARMACY";
            //
            lblPharmacy.AutoSize = true;
            lblPharmacy.Location = new Point(18, 160);
            lblPharmacy.Name = "lblPharmacy";
            lblPharmacy.Size = new Size(280, 20);
            lblPharmacy.TabIndex = 9;
            //
            lblStockCaption.AutoSize = true;
            lblStockCaption.Location = new Point(310, 142);
            lblStockCaption.Name = "lblStockCaption";
            lblStockCaption.Size = new Size(110, 16);
            lblStockCaption.TabIndex = 10;
            lblStockCaption.Text = "AVAILABILITY";
            //
            lblStock.AutoSize = true;
            lblStock.Location = new Point(310, 160);
            lblStock.Name = "lblStock";
            lblStock.Size = new Size(240, 20);
            lblStock.TabIndex = 11;
            //
            lblRxBadge.AutoSize = false;
            lblRxBadge.Location = new Point(18, 192);
            lblRxBadge.Name = "lblRxBadge";
            lblRxBadge.Size = new Size(564, 24);
            lblRxBadge.TextAlign = ContentAlignment.MiddleLeft;
            lblRxBadge.TabIndex = 12;
            //
            lblDescription.AutoSize = false;
            lblDescription.Location = new Point(18, 222);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(564, 48);
            lblDescription.TabIndex = 13;
            //
            grpPrice.Controls.Add(lblOriginalPrice);
            grpPrice.Controls.Add(lblFinalPrice);
            grpPrice.Controls.Add(lblDiscountBadge);
            grpPrice.Controls.Add(lblQuantity);
            grpPrice.Controls.Add(txtQuantity);
            grpPrice.Controls.Add(btnAddToCart);
            grpPrice.Controls.Add(lblAddMessage);
            grpPrice.Location = new Point(640, 84);
            grpPrice.Name = "grpPrice";
            grpPrice.Size = new Size(300, 280);
            grpPrice.TabIndex = 2;
            grpPrice.TabStop = false;
            grpPrice.Text = "  Price and cart  ";
            //
            lblOriginalPrice.AutoSize = true;
            lblOriginalPrice.Location = new Point(18, 40);
            lblOriginalPrice.Name = "lblOriginalPrice";
            lblOriginalPrice.Size = new Size(160, 22);
            lblOriginalPrice.TabIndex = 0;
            //
            lblFinalPrice.AutoSize = true;
            lblFinalPrice.Location = new Point(18, 64);
            lblFinalPrice.Name = "lblFinalPrice";
            lblFinalPrice.Size = new Size(200, 36);
            lblFinalPrice.TabIndex = 1;
            //
            lblDiscountBadge.AutoSize = false;
            lblDiscountBadge.Location = new Point(18, 106);
            lblDiscountBadge.Name = "lblDiscountBadge";
            lblDiscountBadge.Size = new Size(264, 40);
            lblDiscountBadge.TabIndex = 2;
            //
            lblQuantity.AutoSize = true;
            lblQuantity.Location = new Point(18, 148);
            lblQuantity.Name = "lblQuantity";
            lblQuantity.Size = new Size(64, 18);
            lblQuantity.TabIndex = 3;
            lblQuantity.Text = "Quantity";
            //
            txtQuantity.Location = new Point(18, 168);
            txtQuantity.Name = "txtQuantity";
            txtQuantity.Size = new Size(264, 27);
            txtQuantity.TabIndex = 4;
            txtQuantity.Text = "1";
            //
            btnAddToCart.Location = new Point(18, 198);
            btnAddToCart.Name = "btnAddToCart";
            btnAddToCart.Size = new Size(264, 40);
            btnAddToCart.TabIndex = 5;
            btnAddToCart.Text = "Add to cart";
            btnAddToCart.Click += btnAddToCart_Click;
            //
            lblAddMessage.AutoSize = false;
            lblAddMessage.Location = new Point(18, 240);
            lblAddMessage.Name = "lblAddMessage";
            lblAddMessage.Size = new Size(264, 36);
            lblAddMessage.TabIndex = 6;
            //
            lblReviewsTitle.AutoSize = true;
            lblReviewsTitle.Location = new Point(20, 380);
            lblReviewsTitle.Name = "lblReviewsTitle";
            lblReviewsTitle.Size = new Size(200, 22);
            lblReviewsTitle.TabIndex = 3;
            lblReviewsTitle.Text = "What other patients said";
            //
            lblAverageRating.AutoSize = true;
            lblAverageRating.Location = new Point(640, 378);
            lblAverageRating.Name = "lblAverageRating";
            lblAverageRating.Size = new Size(300, 26);
            lblAverageRating.TextAlign = ContentAlignment.MiddleRight;
            lblAverageRating.TabIndex = 4;
            //
            dgvReviews.Location = new Point(20, 410);
            dgvReviews.Name = "dgvReviews";
            dgvReviews.Size = new Size(920, 200);
            dgvReviews.TabIndex = 5;
            //
            lblReviewNote.AutoSize = false;
            lblReviewNote.Location = new Point(20, 618);
            lblReviewNote.Name = "lblReviewNote";
            lblReviewNote.Size = new Size(920, 40);
            lblReviewNote.TabIndex = 6;
            lblReviewNote.Text = "Every review here comes from a customer who received this medicine from a delivered order.";
            //
            // MedicineDetailsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(960, 670);
            Controls.Add(lblReviewNote);
            Controls.Add(dgvReviews);
            Controls.Add(lblAverageRating);
            Controls.Add(lblReviewsTitle);
            Controls.Add(grpPrice);
            Controls.Add(grpFacts);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MedicineDetailsForm";
            Text = "PharmaLink - Medicine Details";
            Load += MedicineDetailsForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            grpFacts.ResumeLayout(false);
            grpFacts.PerformLayout();
            grpPrice.ResumeLayout(false);
            grpPrice.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReviews).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblMedicineName;
        private Label lblGenericName;
        private Button btnBack;
        private GroupBox grpFacts;
        private Label lblManufacturerCaption;
        private Label lblManufacturer;
        private Label lblStrengthCaption;
        private Label lblStrength;
        private Label lblCategoryCaption;
        private Label lblCategory;
        private Label lblExpiryCaption;
        private Label lblExpiry;
        private Label lblPharmacyCaption;
        private Label lblPharmacy;
        private Label lblStockCaption;
        private Label lblStock;
        private Label lblRxBadge;
        private Label lblDescription;
        private GroupBox grpPrice;
        private Label lblOriginalPrice;
        private Label lblFinalPrice;
        private Label lblDiscountBadge;
        private Label lblQuantity;
        private TextBox txtQuantity;
        private Button btnAddToCart;
        private Label lblAddMessage;
        private Label lblReviewsTitle;
        private Label lblAverageRating;
        private DataGridView dgvReviews;
        private Label lblReviewNote;
    }
}
