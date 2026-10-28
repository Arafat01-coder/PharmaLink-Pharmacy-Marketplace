using System.Windows.Forms;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// The online payment step of checkout, opened by CheckoutForm for bKash,
    /// Nagad or Visa card. It closes with OK only when the payment gateway has
    /// approved the payment, and then <see cref="Authorization"/> carries the
    /// gateway's reference for OrderService.Checkout to record.
    ///
    ///  - bKash / Nagad follow the steps of the real wallet checkout: account
    ///    number, a six digit code sent to that number, then the PIN.
    ///  - Visa asks for the number, the name on the card, the expiry and the
    ///    CVV. The number, expiry and CVV never leave this form; only the last
    ///    four digits come back in the authorization.
    ///
    /// The gateway is <see cref="PaymentGateways.Current"/>; while it is the
    /// sandbox, a yellow banner says that no real money moves.
    /// Fields show red errors only after the first press of Pay.
    /// </summary>
    public partial class PaymentGatewayForm : Form
    {
        private readonly IPaymentGateway _gateway = PaymentGateways.Current;
        private readonly string _method = "";
        private readonly string _walletNumber = "";
        private readonly decimal _amount;
        private readonly string _pharmacyName = "";

        private bool _submitAttempted;
        private bool _paying;

        /// <summary>Set when the payment was approved; null otherwise.</summary>
        public PaymentAuthorization Authorization { get; private set; }

        public PaymentGatewayForm()
        {
            InitializeComponent();
        }

        /// <param name="method">bKash, Nagad or Card, as stored in Orders.PaymentMethod.</param>
        /// <param name="walletNumber">The wallet number typed on the checkout, for bKash and Nagad.</param>
        public PaymentGatewayForm(string method, string walletNumber, decimal amount, string pharmacyName) : this()
        {
            _method = method ?? "";
            _walletNumber = walletNumber ?? "";
            _amount = amount;
            _pharmacyName = pharmacyName ?? "";
        }

        private bool IsCard => _method == "Card";

        private string MethodName => IsCard ? "Visa card" : _method;

        private void PaymentGatewayForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();

            lblTitle.Text = "Pay with " + MethodName;
            lblSubtitle.Text = _pharmacyName.Length > 0 ? "Order from " + _pharmacyName : "PharmaLink order";
            lblAmount.Text = UiTheme.Money(_amount);
            btnPay.Text = "Pay " + UiTheme.Money(_amount);

            panelWallet.Visible = !IsCard;
            panelCard.Visible = IsCard;

            if (IsCard)
            {
                ActiveControl = txtCardNumber;
            }
            else
            {
                int pinLength = SandboxPaymentGateway.PinLength(_method);
                lblWallet.Text = _method + " account number (11 digits)";
                lblPin.Text = _method + " PIN (" + pinLength + " digits)";
                txtPin.MaxLength = pinLength;

                // The number typed on the checkout is the one the order records,
                // so it is shown here but cannot be changed.
                txtWallet.Text = _walletNumber;
                txtWallet.ReadOnly = Validator.IsMobile(_walletNumber);
                if (txtWallet.ReadOnly) txtWallet.BackColor = UiTheme.ReadOnlyBack;
                ActiveControl = txtWallet.ReadOnly ? (Control)btnSendCode : txtWallet;
            }

            lblSandbox.Visible = !_gateway.IsLive;
            lblCardHint.Visible = IsCard && !_gateway.IsLive;

            FormClosed += (s, args) => ClearSecrets();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Payment");
            StartPosition = FormStartPosition.CenterParent;
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            lblSandbox.Font = UiTheme.FontBodyBold;
            lblSandbox.BackColor = UiTheme.WarningBack;
            lblSandbox.ForeColor = UiTheme.Warning;

            lblAmountCaption.Font = UiTheme.FontSmall;
            lblAmountCaption.ForeColor = UiTheme.TextMuted;
            lblAmount.Font = UiTheme.FontTileValue;
            lblAmount.ForeColor = UiTheme.Primary;

            foreach (Panel panel in new[] { panelWallet, panelCard })
            {
                foreach (Control child in panel.Controls)
                {
                    child.Font = UiTheme.FontBody;
                    child.ForeColor = UiTheme.TextDark;
                    if (child is Label label && label.Name.EndsWith("Error"))
                    {
                        label.Font = UiTheme.FontSmall;
                        label.ForeColor = UiTheme.Danger;
                    }
                }
            }

            lblCodeInfo.Font = UiTheme.FontSmall;
            lblCodeInfo.ForeColor = UiTheme.TextMuted;
            lblCardHint.Font = UiTheme.FontSmall;
            lblCardHint.ForeColor = UiTheme.TextMuted;
            lblFormError.Font = UiTheme.FontSmall;
            lblFormError.ForeColor = UiTheme.Danger;

            UiTheme.StyleAccent(btnSendCode);
            UiTheme.StyleSuccess(btnPay);
            btnPay.Font = UiTheme.FontButtonStrong;
            UiTheme.StyleSecondary(btnCancelPayment);

            AcceptButton = btnPay;
            CancelButton = btnCancelPayment;
        }

        // ---------------------------------------------------------------------
        //  VALIDATION
        // ---------------------------------------------------------------------

        private void Field_Changed(object sender, EventArgs e)
        {
            lblFormError.Visible = false;
            if (_submitAttempted) ValidateFields();
        }

        /// <summary>Checks every field of the visible panel; shows errors only after the first Pay.</summary>
        private bool ValidateFields()
        {
            bool ok = true;

            if (IsCard)
            {
                ok &= Check(Validator.IsVisaCardNumber(txtCardNumber.Text), lblCardNumberError, txtCardNumber,
                            "Enter the 16 digit Visa card number (it starts with 4).");
                ok &= Check(!Validator.IsBlank(txtCardName.Text), lblCardNameError, txtCardName,
                            "Enter the name printed on the card.");

                bool expiryOk = Validator.IsCardExpiry(txtExpiry.Text);
                bool cvvOk = Validator.IsCvv(txtCvv.Text);
                ok &= expiryOk && cvvOk;
                if (!_submitAttempted || (expiryOk && cvvOk))
                {
                    UiTheme.ClearError(lblExpiryError, null);
                }
                else
                {
                    UiTheme.ShowError(lblExpiryError, null, !expiryOk
                        ? "Write the expiry as MM/YY, and it must not have passed."
                        : "Enter the 3 digit CVV from the back of the card.");
                }
                ColourField(txtExpiry, expiryOk);
                ColourField(txtCvv, cvvOk);
            }
            else
            {
                ok &= Check(Validator.IsMobile(txtWallet.Text), lblWalletError, txtWallet,
                            "Enter the 11 digit " + _method + " account number.");
                ok &= Check(Validator.IsOtp(txtCode.Text), lblCodeError, txtCode,
                            "Enter the 6 digit code.");
                int pinLength = SandboxPaymentGateway.PinLength(_method);
                ok &= Check(Validator.IsWalletPin(txtPin.Text, pinLength), lblPinError, txtPin,
                            "Enter your " + pinLength + " digit PIN.");
            }

            return ok;
        }

        private bool Check(bool rulePassed, Label errorLabel, Control field, string message)
        {
            if (rulePassed || !_submitAttempted) UiTheme.ClearError(errorLabel, field);
            else UiTheme.ShowError(errorLabel, field, message);

            // A read-only wallet number keeps its grey background.
            if (field == txtWallet && txtWallet.ReadOnly) txtWallet.BackColor = UiTheme.ReadOnlyBack;
            return rulePassed;
        }

        private void ColourField(Control field, bool ok)
        {
            field.BackColor = ok || !_submitAttempted ? Color.White : UiTheme.ErrorFieldBack;
        }

        // ---------------------------------------------------------------------
        //  ACTIONS
        // ---------------------------------------------------------------------

        private void btnSendCode_Click(object sender, EventArgs e)
        {
            if (!Validator.IsMobile(txtWallet.Text))
            {
                UiTheme.ShowError(lblWalletError, txtWallet, "Enter the 11 digit " + _method + " account number first.");
                txtWallet.Focus();
                return;
            }

            UiTheme.ClearError(lblWalletError, txtWallet);
            if (txtWallet.ReadOnly) txtWallet.BackColor = UiTheme.ReadOnlyBack;

            lblCodeInfo.Text = _gateway.SendWalletCode(_method, txtWallet.Text.Trim());
            lblCodeInfo.ForeColor = UiTheme.Accent;
            btnSendCode.Text = "Send again";
            txtCode.Clear();
            txtCode.Focus();
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            if (_paying) return;

            _submitAttempted = true;
            lblFormError.Visible = false;
            if (!ValidateFields()) return;

            _paying = true;
            btnPay.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                string failure;
                PaymentAuthorization result = IsCard
                    ? _gateway.AuthorizeCard(txtCardNumber.Text, txtExpiry.Text, txtCvv.Text, txtCardName.Text,
                                             _amount, out failure)
                    : _gateway.AuthorizeWallet(_method, txtWallet.Text.Trim(), txtCode.Text, txtPin.Text,
                                               _amount, out failure);

                if (result == null)
                {
                    UiTheme.ShowError(lblFormError, null, failure);
                    txtPin.Clear();
                    txtCvv.Clear();
                    return;
                }

                Authorization = result;
                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                Cursor = Cursors.Default;
                _paying = false;
                if (!IsDisposed) btnPay.Enabled = true;
            }
        }

        private void btnCancelPayment_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>The card number, CVV and PIN do not outlive the window.</summary>
        private void ClearSecrets()
        {
            txtCardNumber.Clear();
            txtCvv.Clear();
            txtExpiry.Clear();
            txtPin.Clear();
            txtCode.Clear();
        }
    }
}
