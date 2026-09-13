namespace PharmaLinkApp.Forms
{
    partial class UploadPrescriptionForm
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

            btnChooseFile = new Button();
            lblFileError = new Label();
            picPreview = new PictureBox();
            lblFileInfo = new Label();

            lblDoctor = new Label();
            txtDoctor = new TextBox();

            lblRules = new Label();
            btnAttach = new Button();
            btnCancel = new Button();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(620, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Upload your prescription";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(500, 16);
            lblSubtitle.TabIndex = 1;
            //
            btnChooseFile.Location = new Point(20, 86);
            btnChooseFile.Name = "btnChooseFile";
            btnChooseFile.Size = new Size(580, 40);
            btnChooseFile.TabIndex = 1;
            btnChooseFile.Text = "Choose a photograph of the prescription...";
            btnChooseFile.Click += btnChooseFile_Click;
            //
            lblFileError.AutoSize = false;
            lblFileError.Location = new Point(20, 130);
            lblFileError.Name = "lblFileError";
            lblFileError.Size = new Size(580, 18);
            lblFileError.TabIndex = 2;
            lblFileError.Visible = false;
            //
            picPreview.BackColor = Color.White;
            picPreview.BorderStyle = BorderStyle.FixedSingle;
            picPreview.Location = new Point(20, 152);
            picPreview.Name = "picPreview";
            picPreview.Size = new Size(580, 260);
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.TabIndex = 3;
            picPreview.TabStop = false;
            //
            lblFileInfo.AutoSize = false;
            lblFileInfo.Location = new Point(20, 418);
            lblFileInfo.Name = "lblFileInfo";
            lblFileInfo.Size = new Size(580, 20);
            lblFileInfo.TabIndex = 4;
            //
            lblDoctor.AutoSize = true;
            lblDoctor.Location = new Point(20, 446);
            lblDoctor.Name = "lblDoctor";
            lblDoctor.Size = new Size(280, 18);
            lblDoctor.TabIndex = 5;
            lblDoctor.Text = "Prescribing doctor (optional)";
            //
            txtDoctor.Location = new Point(20, 468);
            txtDoctor.MaxLength = 100;
            txtDoctor.Name = "txtDoctor";
            txtDoctor.PlaceholderText = "for example  Dr. Anisur Rahman, MBBS";
            txtDoctor.Size = new Size(580, 27);
            txtDoctor.TabIndex = 6;
            //
            lblRules.AutoSize = false;
            lblRules.Location = new Point(20, 502);
            lblRules.Name = "lblRules";
            lblRules.Size = new Size(580, 56);
            lblRules.TabIndex = 7;
            lblRules.Text = "JPG or PNG only, under 2 MB. Make sure the doctor's name, the medicines and the date are easy to read. The pharmacy checks the photo before confirming your order.";
            //
            btnAttach.Location = new Point(320, 568);
            btnAttach.Name = "btnAttach";
            btnAttach.Size = new Size(160, 42);
            btnAttach.TabIndex = 8;
            btnAttach.Text = "Attach";
            btnAttach.Click += btnAttach_Click;
            //
            btnCancel.Location = new Point(490, 568);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(110, 42);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            //
            // UploadPrescriptionForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(620, 626);
            Controls.Add(btnCancel);
            Controls.Add(btnAttach);
            Controls.Add(lblRules);
            Controls.Add(txtDoctor);
            Controls.Add(lblDoctor);
            Controls.Add(lblFileInfo);
            Controls.Add(picPreview);
            Controls.Add(lblFileError);
            Controls.Add(btnChooseFile);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UploadPrescriptionForm";
            ShowInTaskbar = false;
            Text = "PharmaLink - Upload Prescription";
            Load += UploadPrescriptionForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnChooseFile;
        private Label lblFileError;
        private PictureBox picPreview;
        private Label lblFileInfo;
        private Label lblDoctor;
        private TextBox txtDoctor;
        private Label lblRules;
        private Button btnAttach;
        private Button btnCancel;
    }
}
