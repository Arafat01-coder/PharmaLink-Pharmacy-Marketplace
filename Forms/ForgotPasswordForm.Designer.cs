namespace PharmaLinkApp.Forms
{
    partial class ForgotPasswordForm
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

            lblIntro = new Label();
            lblEmail = new Label();
            txtResetEmail = new TextBox();
            lblEmailError = new Label();
            lblMobile = new Label();
            txtResetMobile = new TextBox();
            lblMobileError = new Label();
            lblResult = new Label();
            btnSubmitReset = new Button();
            btnCancelReset = new Button();

            panelHeader.SuspendLayout();
            SuspendLayout();
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
            lblTitle.Size = new Size(260, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Forgot your password?";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(400, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "The PharmaLink administrator resets it for you by phone.";
            //
            lblIntro.AutoSize = false;
            lblIntro.Location = new Point(24, 82);
            lblIntro.Name = "lblIntro";
            lblIntro.Size = new Size(432, 54);
            lblIntro.TabIndex = 1;
            lblIntro.Text = "PharmaLink cannot send email or text messages. Enter the email address and the mobile number registered on your account. The administrator checks them and phones that number with a temporary password.";
            //
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(24, 146);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(90, 18);
            lblEmail.TabIndex = 2;
            lblEmail.Text = "Email address";
            //
            txtResetEmail.Location = new Point(24, 168);
            txtResetEmail.MaxLength = 120;
            txtResetEmail.Name = "txtResetEmail";
            txtResetEmail.Size = new Size(432, 27);
            txtResetEmail.TabIndex = 3;
            txtResetEmail.TextChanged += Field_Changed;
            txtResetEmail.Leave += txtResetEmail_Leave;
            //
            lblEmailError.AutoSize = false;
            lblEmailError.Location = new Point(24, 196);
            lblEmailError.Name = "lblEmailError";
            lblEmailError.Size = new Size(432, 18);
            lblEmailError.TabIndex = 4;
            lblEmailError.Visible = false;
            //
            lblMobile.AutoSize = true;
            lblMobile.Location = new Point(24, 220);
            lblMobile.Name = "lblMobile";
            lblMobile.Size = new Size(170, 18);
            lblMobile.TabIndex = 5;
            lblMobile.Text = "Registered mobile number";
            //
            txtResetMobile.Location = new Point(24, 242);
            txtResetMobile.MaxLength = 11;
            txtResetMobile.Name = "txtResetMobile";
            txtResetMobile.PlaceholderText = "11 digits, starting with 01";
            txtResetMobile.Size = new Size(432, 27);
            txtResetMobile.TabIndex = 6;
            txtResetMobile.TextChanged += Field_Changed;
            txtResetMobile.Leave += txtResetMobile_Leave;
            //
            lblMobileError.AutoSize = false;
            lblMobileError.Location = new Point(24, 270);
            lblMobileError.Name = "lblMobileError";
            lblMobileError.Size = new Size(432, 18);
            lblMobileError.TabIndex = 7;
            lblMobileError.Visible = false;
            //
            lblResult.AutoSize = false;
            lblResult.Location = new Point(24, 296);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(432, 58);
            lblResult.TabIndex = 8;
            lblResult.Visible = false;
            //
            btnSubmitReset.Location = new Point(196, 364);
            btnSubmitReset.Name = "btnSubmitReset";
            btnSubmitReset.Size = new Size(150, 40);
            btnSubmitReset.TabIndex = 9;
            btnSubmitReset.Text = "Send request";
            btnSubmitReset.Click += btnSubmitReset_Click;
            //
            btnCancelReset.Location = new Point(356, 364);
            btnCancelReset.Name = "btnCancelReset";
            btnCancelReset.Size = new Size(100, 40);
            btnCancelReset.TabIndex = 10;
            btnCancelReset.Text = "Cancel";
            btnCancelReset.Click += btnCancelReset_Click;
            //
            // ForgotPasswordForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 424);
            Controls.Add(btnCancelReset);
            Controls.Add(btnSubmitReset);
            Controls.Add(lblResult);
            Controls.Add(lblMobileError);
            Controls.Add(txtResetMobile);
            Controls.Add(lblMobile);
            Controls.Add(lblEmailError);
            Controls.Add(txtResetEmail);
            Controls.Add(lblEmail);
            Controls.Add(lblIntro);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ForgotPasswordForm";
            ShowInTaskbar = false;
            Text = "PharmaLink - Forgot password";
            Load += ForgotPasswordForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblIntro;
        private Label lblEmail;
        private TextBox txtResetEmail;
        private Label lblEmailError;
        private Label lblMobile;
        private TextBox txtResetMobile;
        private Label lblMobileError;
        private Label lblResult;
        private Button btnSubmitReset;
        private Button btnCancelReset;
    }
}
