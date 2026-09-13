namespace PharmaLinkApp.Forms
{
    partial class InvoiceForm
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

            rtbInvoice = new RichTextBox();
            lblFooterNote = new Label();
            btnPrint = new Button();
            btnSaveText = new Button();
            btnClose = new Button();

            panelHeader.SuspendLayout();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(780, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Invoice";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(500, 16);
            lblSubtitle.TabIndex = 1;
            //
            rtbInvoice.BackColor = Color.White;
            rtbInvoice.BorderStyle = BorderStyle.FixedSingle;
            rtbInvoice.Location = new Point(20, 84);
            rtbInvoice.Name = "rtbInvoice";
            rtbInvoice.ReadOnly = true;
            rtbInvoice.Size = new Size(740, 500);
            rtbInvoice.TabIndex = 1;
            rtbInvoice.Text = "";
            rtbInvoice.WordWrap = false;
            //
            lblFooterNote.AutoSize = false;
            lblFooterNote.Location = new Point(20, 590);
            lblFooterNote.Name = "lblFooterNote";
            lblFooterNote.Size = new Size(740, 34);
            lblFooterNote.TabIndex = 2;
            lblFooterNote.Text = "The platform commission is deducted from the pharmacy, not added to your bill. What you see above is exactly what you pay.";
            //
            btnPrint.Location = new Point(320, 630);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(140, 40);
            btnPrint.TabIndex = 3;
            btnPrint.Text = "Print";
            btnPrint.Click += btnPrint_Click;
            //
            btnSaveText.Location = new Point(470, 630);
            btnSaveText.Name = "btnSaveText";
            btnSaveText.Size = new Size(160, 40);
            btnSaveText.TabIndex = 4;
            btnSaveText.Text = "Save as text file";
            btnSaveText.Click += btnSaveText_Click;
            //
            btnClose.Location = new Point(640, 630);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(120, 40);
            btnClose.TabIndex = 5;
            btnClose.Text = "Close";
            btnClose.Click += btnClose_Click;
            //
            // InvoiceForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(780, 686);
            Controls.Add(btnClose);
            Controls.Add(btnSaveText);
            Controls.Add(btnPrint);
            Controls.Add(lblFooterNote);
            Controls.Add(rtbInvoice);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InvoiceForm";
            ShowInTaskbar = false;
            Text = "PharmaLink - Invoice";
            Load += InvoiceForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private RichTextBox rtbInvoice;
        private Label lblFooterNote;
        private Button btnPrint;
        private Button btnSaveText;
        private Button btnClose;
    }
}
