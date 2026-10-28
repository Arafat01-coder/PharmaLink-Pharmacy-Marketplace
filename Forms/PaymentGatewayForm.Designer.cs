namespace PharmaLinkApp.Forms
{
    partial class PaymentGatewayForm
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
            lblSandbox = new Label();
            lblAmountCaption = new Label();
            lblAmount = new Label();

            panelWallet = new Panel();
            lblWallet = new Label();
            txtWallet = new TextBox();
            btnSendCode = new Button();
            lblWalletError = new Label();
            lblCodeInfo = new Label();
            lblCode = new Label();
            txtCode = new TextBox();
            lblCodeError = new Label();
            lblPin = new Label();
            txtPin = new TextBox();
            lblPinError = new Label();

            panelCard = new Panel();
            lblCardNumber = new Label();
            txtCardNumber = new TextBox();
            lblCardNumberError = new Label();
            lblCardName = new Label();
            txtCardName = new TextBox();
            lblCardNameError = new Label();
            lblExpiry = new Label();
            txtExpiry = new TextBox();
            lblCvv = new Label();
            txtCvv = new TextBox();
            lblExpiryError = new Label();
            lblCardHint = new Label();

            lblFormError = new Label();
            btnPay = new Button();
            btnCancelPayment = new Button();

            panelHeader.SuspendLayout();
            panelWallet.SuspendLayout();
            panelCard.SuspendLayout();
            SuspendLayout();
            //
            // header
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(480, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Pay";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(300, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Pharmacy";
            //
            lblSandbox.AutoSize = false;
            lblSandbox.Location = new Point(24, 80);
            lblSandbox.Name = "lblSandbox";
            lblSandbox.Padding = new Padding(8, 0, 8, 0);
            lblSandbox.Size = new Size(432, 34);
            lblSandbox.TabIndex = 1;
            lblSandbox.Text = "SANDBOX - test payments only. No real money moves.";
            lblSandbox.TextAlign = ContentAlignment.MiddleLeft;
            //
            lblAmountCaption.AutoSize = true;
            lblAmountCaption.Location = new Point(24, 124);
            lblAmountCaption.Name = "lblAmountCaption";
            lblAmountCaption.Size = new Size(100, 18);
            lblAmountCaption.TabIndex = 2;
            lblAmountCaption.Text = "Amount to pay";
            //
            lblAmount.AutoSize = true;
            lblAmount.Location = new Point(22, 142);
            lblAmount.Name = "lblAmount";
            lblAmount.Size = new Size(150, 32);
            lblAmount.TabIndex = 3;
            lblAmount.Text = "Tk 0.00";
            //
            // wallet panel (bKash / Nagad)
            //
            panelWallet.Controls.Add(lblWallet);
            panelWallet.Controls.Add(txtWallet);
            panelWallet.Controls.Add(btnSendCode);
            panelWallet.Controls.Add(lblWalletError);
            panelWallet.Controls.Add(lblCodeInfo);
            panelWallet.Controls.Add(lblCode);
            panelWallet.Controls.Add(txtCode);
            panelWallet.Controls.Add(lblCodeError);
            panelWallet.Controls.Add(lblPin);
            panelWallet.Controls.Add(txtPin);
            panelWallet.Controls.Add(lblPinError);
            panelWallet.Location = new Point(24, 186);
            panelWallet.Name = "panelWallet";
            panelWallet.Size = new Size(432, 236);
            panelWallet.TabIndex = 4;
            //
            lblWallet.AutoSize = true;
            lblWallet.Location = new Point(0, 0);
            lblWallet.Name = "lblWallet";
            lblWallet.Size = new Size(150, 18);
            lblWallet.TabIndex = 0;
            lblWallet.Text = "Account number (11 digits)";
            //
            txtWallet.Location = new Point(0, 22);
            txtWallet.MaxLength = 11;
            txtWallet.Name = "txtWallet";
            txtWallet.Size = new Size(290, 27);
            txtWallet.TabIndex = 1;
            txtWallet.TextChanged += Field_Changed;
            //
            btnSendCode.Location = new Point(300, 20);
            btnSendCode.Name = "btnSendCode";
            btnSendCode.Size = new Size(132, 31);
            btnSendCode.TabIndex = 2;
            btnSendCode.Text = "Send code";
            btnSendCode.Click += btnSendCode_Click;
            //
            lblWalletError.AutoSize = false;
            lblWalletError.Location = new Point(0, 52);
            lblWalletError.Name = "lblWalletError";
            lblWalletError.Size = new Size(432, 18);
            lblWalletError.TabIndex = 3;
            lblWalletError.Visible = false;
            //
            lblCodeInfo.AutoSize = false;
            lblCodeInfo.Location = new Point(0, 72);
            lblCodeInfo.Name = "lblCodeInfo";
            lblCodeInfo.Size = new Size(432, 36);
            lblCodeInfo.TabIndex = 4;
            lblCodeInfo.Text = "Press 'Send code' to receive a six digit verification code.";
            //
            lblCode.AutoSize = true;
            lblCode.Location = new Point(0, 112);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(150, 18);
            lblCode.TabIndex = 5;
            lblCode.Text = "Verification code (6 digits)";
            //
            txtCode.Location = new Point(0, 134);
            txtCode.MaxLength = 6;
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(200, 27);
            txtCode.TabIndex = 6;
            txtCode.TextChanged += Field_Changed;
            //
            lblCodeError.AutoSize = false;
            lblCodeError.Location = new Point(0, 162);
            lblCodeError.Name = "lblCodeError";
            lblCodeError.Size = new Size(210, 18);
            lblCodeError.TabIndex = 7;
            lblCodeError.Visible = false;
            //
            lblPin.AutoSize = true;
            lblPin.Location = new Point(232, 112);
            lblPin.Name = "lblPin";
            lblPin.Size = new Size(100, 18);
            lblPin.TabIndex = 8;
            lblPin.Text = "PIN";
            //
            txtPin.Location = new Point(232, 134);
            txtPin.MaxLength = 5;
            txtPin.Name = "txtPin";
            txtPin.PasswordChar = '*';
            txtPin.Size = new Size(200, 27);
            txtPin.TabIndex = 9;
            txtPin.TextChanged += Field_Changed;
            //
            lblPinError.AutoSize = false;
            lblPinError.Location = new Point(232, 162);
            lblPinError.Name = "lblPinError";
            lblPinError.Size = new Size(200, 18);
            lblPinError.TabIndex = 10;
            lblPinError.Visible = false;
            //
            // card panel (Visa)
            //
            panelCard.Controls.Add(lblCardNumber);
            panelCard.Controls.Add(txtCardNumber);
            panelCard.Controls.Add(lblCardNumberError);
            panelCard.Controls.Add(lblCardName);
            panelCard.Controls.Add(txtCardName);
            panelCard.Controls.Add(lblCardNameError);
            panelCard.Controls.Add(lblExpiry);
            panelCard.Controls.Add(txtExpiry);
            panelCard.Controls.Add(lblCvv);
            panelCard.Controls.Add(txtCvv);
            panelCard.Controls.Add(lblExpiryError);
            panelCard.Controls.Add(lblCardHint);
            panelCard.Location = new Point(24, 186);
            panelCard.Name = "panelCard";
            panelCard.Size = new Size(432, 236);
            panelCard.TabIndex = 5;
            panelCard.Visible = false;
            //
            lblCardNumber.AutoSize = true;
            lblCardNumber.Location = new Point(0, 0);
            lblCardNumber.Name = "lblCardNumber";
            lblCardNumber.Size = new Size(120, 18);
            lblCardNumber.TabIndex = 0;
            lblCardNumber.Text = "Visa card number";
            //
            txtCardNumber.Location = new Point(0, 22);
            txtCardNumber.MaxLength = 19;
            txtCardNumber.Name = "txtCardNumber";
            txtCardNumber.Size = new Size(432, 27);
            txtCardNumber.TabIndex = 1;
            txtCardNumber.TextChanged += Field_Changed;
            //
            lblCardNumberError.AutoSize = false;
            lblCardNumberError.Location = new Point(0, 50);
            lblCardNumberError.Name = "lblCardNumberError";
            lblCardNumberError.Size = new Size(432, 18);
            lblCardNumberError.TabIndex = 2;
            lblCardNumberError.Visible = false;
            //
            lblCardName.AutoSize = true;
            lblCardName.Location = new Point(0, 70);
            lblCardName.Name = "lblCardName";
            lblCardName.Size = new Size(120, 18);
            lblCardName.TabIndex = 3;
            lblCardName.Text = "Name on card";
            //
            txtCardName.Location = new Point(0, 92);
            txtCardName.MaxLength = 60;
            txtCardName.Name = "txtCardName";
            txtCardName.Size = new Size(432, 27);
            txtCardName.TabIndex = 4;
            txtCardName.TextChanged += Field_Changed;
            //
            lblCardNameError.AutoSize = false;
            lblCardNameError.Location = new Point(0, 120);
            lblCardNameError.Name = "lblCardNameError";
            lblCardNameError.Size = new Size(432, 18);
            lblCardNameError.TabIndex = 5;
            lblCardNameError.Visible = false;
            //
            lblExpiry.AutoSize = true;
            lblExpiry.Location = new Point(0, 140);
            lblExpiry.Name = "lblExpiry";
            lblExpiry.Size = new Size(100, 18);
            lblExpiry.TabIndex = 6;
            lblExpiry.Text = "Expiry (MM/YY)";
            //
            txtExpiry.Location = new Point(0, 162);
            txtExpiry.MaxLength = 5;
            txtExpiry.Name = "txtExpiry";
            txtExpiry.Size = new Size(200, 27);
            txtExpiry.TabIndex = 7;
            txtExpiry.TextChanged += Field_Changed;
            //
            lblCvv.AutoSize = true;
            lblCvv.Location = new Point(232, 140);
            lblCvv.Name = "lblCvv";
            lblCvv.Size = new Size(100, 18);
            lblCvv.TabIndex = 8;
            lblCvv.Text = "CVV (3 digits)";
            //
            txtCvv.Location = new Point(232, 162);
            txtCvv.MaxLength = 3;
            txtCvv.Name = "txtCvv";
            txtCvv.PasswordChar = '*';
            txtCvv.Size = new Size(200, 27);
            txtCvv.TabIndex = 9;
            txtCvv.TextChanged += Field_Changed;
            //
            lblExpiryError.AutoSize = false;
            lblExpiryError.Location = new Point(0, 190);
            lblExpiryError.Name = "lblExpiryError";
            lblExpiryError.Size = new Size(432, 18);
            lblExpiryError.TabIndex = 10;
            lblExpiryError.Visible = false;
            //
            lblCardHint.AutoSize = false;
            lblCardHint.Location = new Point(0, 210);
            lblCardHint.Name = "lblCardHint";
            lblCardHint.Size = new Size(432, 26);
            lblCardHint.TabIndex = 11;
            lblCardHint.Text = "Test card: 4242 4242 4242 4242. 4000 0000 0000 0002 is always declined.";
            //
            lblFormError.AutoSize = false;
            lblFormError.Location = new Point(24, 428);
            lblFormError.Name = "lblFormError";
            lblFormError.Size = new Size(432, 36);
            lblFormError.TabIndex = 6;
            lblFormError.Visible = false;
            //
            btnPay.Location = new Point(176, 472);
            btnPay.Name = "btnPay";
            btnPay.Size = new Size(170, 42);
            btnPay.TabIndex = 7;
            btnPay.Text = "Pay";
            btnPay.Click += btnPay_Click;
            //
            btnCancelPayment.Location = new Point(356, 472);
            btnCancelPayment.Name = "btnCancelPayment";
            btnCancelPayment.Size = new Size(100, 42);
            btnCancelPayment.TabIndex = 8;
            btnCancelPayment.Text = "Cancel";
            btnCancelPayment.Click += btnCancelPayment_Click;
            //
            // PaymentGatewayForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 532);
            Controls.Add(btnCancelPayment);
            Controls.Add(btnPay);
            Controls.Add(lblFormError);
            Controls.Add(panelCard);
            Controls.Add(panelWallet);
            Controls.Add(lblAmount);
            Controls.Add(lblAmountCaption);
            Controls.Add(lblSandbox);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PaymentGatewayForm";
            ShowInTaskbar = false;
            Text = "PharmaLink  -  Payment";
            Load += PaymentGatewayForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelWallet.ResumeLayout(false);
            panelWallet.PerformLayout();
            panelCard.ResumeLayout(false);
            panelCard.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblSandbox;
        private Label lblAmountCaption;
        private Label lblAmount;

        private Panel panelWallet;
        private Label lblWallet;
        private TextBox txtWallet;
        private Button btnSendCode;
        private Label lblWalletError;
        private Label lblCodeInfo;
        private Label lblCode;
        private TextBox txtCode;
        private Label lblCodeError;
        private Label lblPin;
        private TextBox txtPin;
        private Label lblPinError;

        private Panel panelCard;
        private Label lblCardNumber;
        private TextBox txtCardNumber;
        private Label lblCardNumberError;
        private Label lblCardName;
        private TextBox txtCardName;
        private Label lblCardNameError;
        private Label lblExpiry;
        private TextBox txtExpiry;
        private Label lblCvv;
        private TextBox txtCvv;
        private Label lblExpiryError;
        private Label lblCardHint;

        private Label lblFormError;
        private Button btnPay;
        private Button btnCancelPayment;
    }
}
