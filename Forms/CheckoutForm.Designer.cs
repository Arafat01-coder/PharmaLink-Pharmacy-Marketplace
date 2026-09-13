namespace PharmaLinkApp.Forms
{
    partial class CheckoutForm
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
            btnCancel = new Button();

            grpDelivery = new GroupBox();
            lblAddress = new Label();
            txtAddress = new TextBox();
            lblAddressError = new Label();
            lblPayment = new Label();
            cmbPayment = new ComboBox();
            lblPaymentError = new Label();
            lblMobile = new Label();
            txtMobile = new TextBox();
            lblMobileError = new Label();
            lblRxWarning = new Label();
            btnUploadRx = new Button();
            lblRxState = new Label();

            grpReview = new GroupBox();
            dgvReview = new DataGridView();
            lblItemsCaption = new Label();
            lblItemsValue = new Label();
            lblDeliveryCaption = new Label();
            lblDeliveryValue = new Label();
            lblPayableCaption = new Label();
            lblPayableValue = new Label();

            lblHint = new Label();
            lblStatus = new Label();
            btnConfirm = new Button();

            panelHeader.SuspendLayout();
            grpDelivery.SuspendLayout();
            grpReview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReview).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnCancel);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1060, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Checkout";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(600, 16);
            lblSubtitle.TabIndex = 1;
            //
            btnCancel.Location = new Point(924, 16);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(110, 36);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            //
            grpDelivery.Controls.Add(lblAddress);
            grpDelivery.Controls.Add(txtAddress);
            grpDelivery.Controls.Add(lblAddressError);
            grpDelivery.Controls.Add(lblPayment);
            grpDelivery.Controls.Add(cmbPayment);
            grpDelivery.Controls.Add(lblPaymentError);
            grpDelivery.Controls.Add(lblMobile);
            grpDelivery.Controls.Add(txtMobile);
            grpDelivery.Controls.Add(lblMobileError);
            grpDelivery.Controls.Add(lblRxWarning);
            grpDelivery.Controls.Add(btnUploadRx);
            grpDelivery.Controls.Add(lblRxState);
            grpDelivery.Location = new Point(20, 84);
            grpDelivery.Name = "grpDelivery";
            grpDelivery.Size = new Size(480, 404);
            grpDelivery.TabIndex = 1;
            grpDelivery.TabStop = false;
            grpDelivery.Text = "  Delivery and payment  ";
            //
            lblAddress.AutoSize = true;
            lblAddress.Location = new Point(18, 34);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(120, 18);
            lblAddress.TabIndex = 0;
            lblAddress.Text = "Delivery address";
            //
            txtAddress.Location = new Point(18, 56);
            txtAddress.MaxLength = 250;
            txtAddress.Multiline = true;
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(440, 62);
            txtAddress.TabIndex = 1;
            txtAddress.TextChanged += Field_Changed;
            //
            lblAddressError.AutoSize = false;
            lblAddressError.Location = new Point(18, 120);
            lblAddressError.Name = "lblAddressError";
            lblAddressError.Size = new Size(440, 16);
            lblAddressError.TabIndex = 2;
            lblAddressError.Visible = false;
            //
            lblPayment.AutoSize = true;
            lblPayment.Location = new Point(18, 142);
            lblPayment.Name = "lblPayment";
            lblPayment.Size = new Size(120, 18);
            lblPayment.TabIndex = 3;
            lblPayment.Text = "Payment method";
            //
            cmbPayment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayment.Location = new Point(18, 164);
            cmbPayment.Name = "cmbPayment";
            cmbPayment.Size = new Size(440, 27);
            cmbPayment.TabIndex = 4;
            cmbPayment.SelectedIndexChanged += cmbPayment_SelectedIndexChanged;
            //
            lblPaymentError.AutoSize = false;
            lblPaymentError.Location = new Point(18, 192);
            lblPaymentError.Name = "lblPaymentError";
            lblPaymentError.Size = new Size(440, 16);
            lblPaymentError.TabIndex = 5;
            lblPaymentError.Visible = false;
            //
            lblMobile.AutoSize = true;
            lblMobile.Location = new Point(18, 214);
            lblMobile.Name = "lblMobile";
            lblMobile.Size = new Size(200, 18);
            lblMobile.TabIndex = 6;
            lblMobile.Text = "bKash number (11 digits)";
            lblMobile.Visible = false;
            //
            txtMobile.Location = new Point(18, 236);
            txtMobile.MaxLength = 11;
            txtMobile.Name = "txtMobile";
            txtMobile.Size = new Size(440, 27);
            txtMobile.TabIndex = 7;
            txtMobile.Visible = false;
            txtMobile.TextChanged += Field_Changed;
            //
            lblMobileError.AutoSize = false;
            lblMobileError.Location = new Point(18, 264);
            lblMobileError.Name = "lblMobileError";
            lblMobileError.Size = new Size(440, 16);
            lblMobileError.TabIndex = 8;
            lblMobileError.Visible = false;
            //
            lblRxWarning.AutoSize = false;
            lblRxWarning.Location = new Point(18, 288);
            lblRxWarning.Name = "lblRxWarning";
            lblRxWarning.Size = new Size(440, 44);
            lblRxWarning.TabIndex = 9;
            //
            btnUploadRx.Location = new Point(18, 336);
            btnUploadRx.Name = "btnUploadRx";
            btnUploadRx.Size = new Size(440, 36);
            btnUploadRx.TabIndex = 10;
            btnUploadRx.Text = "Upload my prescription";
            btnUploadRx.Click += btnUploadRx_Click;
            //
            lblRxState.AutoSize = false;
            lblRxState.Location = new Point(18, 374);
            lblRxState.Name = "lblRxState";
            lblRxState.Size = new Size(440, 20);
            lblRxState.TabIndex = 11;
            //
            grpReview.Controls.Add(dgvReview);
            grpReview.Controls.Add(lblItemsCaption);
            grpReview.Controls.Add(lblItemsValue);
            grpReview.Controls.Add(lblDeliveryCaption);
            grpReview.Controls.Add(lblDeliveryValue);
            grpReview.Controls.Add(lblPayableCaption);
            grpReview.Controls.Add(lblPayableValue);
            grpReview.Location = new Point(520, 84);
            grpReview.Name = "grpReview";
            grpReview.Size = new Size(516, 404);
            grpReview.TabIndex = 2;
            grpReview.TabStop = false;
            grpReview.Text = "  What you are paying for  ";
            //
            dgvReview.Location = new Point(18, 34);
            dgvReview.Name = "dgvReview";
            dgvReview.Size = new Size(480, 240);
            dgvReview.TabIndex = 0;
            //
            lblItemsCaption.AutoSize = true;
            lblItemsCaption.Location = new Point(18, 288);
            lblItemsCaption.Name = "lblItemsCaption";
            lblItemsCaption.Size = new Size(200, 18);
            lblItemsCaption.TabIndex = 1;
            lblItemsCaption.Text = "Items total (offers applied)";
            //
            lblItemsValue.AutoSize = false;
            lblItemsValue.Location = new Point(318, 288);
            lblItemsValue.Name = "lblItemsValue";
            lblItemsValue.Size = new Size(180, 20);
            lblItemsValue.TextAlign = ContentAlignment.MiddleRight;
            lblItemsValue.TabIndex = 2;
            //
            lblDeliveryCaption.AutoSize = true;
            lblDeliveryCaption.Location = new Point(18, 316);
            lblDeliveryCaption.Name = "lblDeliveryCaption";
            lblDeliveryCaption.Size = new Size(200, 18);
            lblDeliveryCaption.TabIndex = 3;
            lblDeliveryCaption.Text = "Delivery charge";
            //
            lblDeliveryValue.AutoSize = false;
            lblDeliveryValue.Location = new Point(318, 316);
            lblDeliveryValue.Name = "lblDeliveryValue";
            lblDeliveryValue.Size = new Size(180, 20);
            lblDeliveryValue.TextAlign = ContentAlignment.MiddleRight;
            lblDeliveryValue.TabIndex = 4;
            //
            lblPayableCaption.AutoSize = true;
            lblPayableCaption.Location = new Point(18, 354);
            lblPayableCaption.Name = "lblPayableCaption";
            lblPayableCaption.Size = new Size(160, 24);
            lblPayableCaption.TabIndex = 5;
            lblPayableCaption.Text = "You pay now";
            //
            lblPayableValue.AutoSize = false;
            lblPayableValue.Location = new Point(258, 348);
            lblPayableValue.Name = "lblPayableValue";
            lblPayableValue.Size = new Size(240, 36);
            lblPayableValue.TextAlign = ContentAlignment.MiddleRight;
            lblPayableValue.TabIndex = 6;
            //
            lblHint.AutoSize = false;
            lblHint.Location = new Point(20, 496);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(1016, 92);
            lblHint.TabIndex = 3;
            //
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 596);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(640, 44);
            lblStatus.TabIndex = 4;
            //
            btnConfirm.Location = new Point(680, 596);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(356, 48);
            btnConfirm.TabIndex = 5;
            btnConfirm.Text = "Confirm order";
            btnConfirm.Click += btnConfirm_Click;
            //
            // CheckoutForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1060, 660);
            Controls.Add(btnConfirm);
            Controls.Add(lblStatus);
            Controls.Add(lblHint);
            Controls.Add(grpReview);
            Controls.Add(grpDelivery);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CheckoutForm";
            Text = "PharmaLink - Checkout";
            Load += CheckoutForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            grpDelivery.ResumeLayout(false);
            grpDelivery.PerformLayout();
            grpReview.ResumeLayout(false);
            grpReview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnCancel;
        private GroupBox grpDelivery;
        private Label lblAddress;
        private TextBox txtAddress;
        private Label lblAddressError;
        private Label lblPayment;
        private ComboBox cmbPayment;
        private Label lblPaymentError;
        private Label lblMobile;
        private TextBox txtMobile;
        private Label lblMobileError;
        private Label lblRxWarning;
        private Button btnUploadRx;
        private Label lblRxState;
        private GroupBox grpReview;
        private DataGridView dgvReview;
        private Label lblItemsCaption;
        private Label lblItemsValue;
        private Label lblDeliveryCaption;
        private Label lblDeliveryValue;
        private Label lblPayableCaption;
        private Label lblPayableValue;
        private Label lblHint;
        private Label lblStatus;
        private Button btnConfirm;
    }
}
