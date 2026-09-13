namespace PharmaLinkApp.Forms
{
    partial class VerifyPrescriptionForm
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

            lblStatusFilter = new Label();
            cmbVerifyStatus = new ComboBox();
            btnRefresh = new Button();

            dgvQueue = new DataGridView();

            grpImage = new GroupBox();
            picPrescription = new PictureBox();
            lblImagePath = new Label();
            lblDoctor = new Label();
            btnApprove = new Button();
            btnReject = new Button();

            lblOrderItems = new Label();
            dgvOrderItems = new DataGridView();
            lblNote = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            grpImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvQueue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvOrderItems).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picPrescription).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1240, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(280, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Verify Prescriptions";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "An order carrying a prescription only medicine cannot be confirmed until you have checked the doctor's chit.";
            //
            btnBack.Location = new Point(1104, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            lblStatusFilter.AutoSize = true;
            lblStatusFilter.Location = new Point(20, 92);
            lblStatusFilter.Name = "lblStatusFilter";
            lblStatusFilter.Size = new Size(50, 18);
            lblStatusFilter.TabIndex = 1;
            lblStatusFilter.Text = "Status";
            //
            cmbVerifyStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVerifyStatus.Location = new Point(76, 88);
            cmbVerifyStatus.Name = "cmbVerifyStatus";
            cmbVerifyStatus.Size = new Size(220, 27);
            cmbVerifyStatus.TabIndex = 2;
            cmbVerifyStatus.SelectedIndexChanged += Filter_Changed;
            //
            btnRefresh.Location = new Point(306, 86);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(110, 31);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "Refresh";
            btnRefresh.Click += Filter_Changed;
            //
            dgvQueue.Location = new Point(20, 128);
            dgvQueue.Name = "dgvQueue";
            dgvQueue.Size = new Size(780, 260);
            dgvQueue.TabIndex = 4;
            dgvQueue.SelectionChanged += dgvQueue_SelectionChanged;
            //
            grpImage.Controls.Add(picPrescription);
            grpImage.Controls.Add(lblImagePath);
            grpImage.Controls.Add(lblDoctor);
            grpImage.Controls.Add(btnApprove);
            grpImage.Controls.Add(btnReject);
            grpImage.Location = new Point(816, 100);
            grpImage.Name = "grpImage";
            grpImage.Size = new Size(404, 500);
            grpImage.TabIndex = 5;
            grpImage.TabStop = false;
            grpImage.Text = "  The uploaded prescription  ";
            //
            picPrescription.BackColor = Color.White;
            picPrescription.BorderStyle = BorderStyle.FixedSingle;
            picPrescription.Location = new Point(18, 34);
            picPrescription.Name = "picPrescription";
            picPrescription.Size = new Size(368, 300);
            picPrescription.SizeMode = PictureBoxSizeMode.Zoom;
            picPrescription.TabIndex = 0;
            picPrescription.TabStop = false;
            //
            lblDoctor.AutoSize = false;
            lblDoctor.Location = new Point(18, 342);
            lblDoctor.Name = "lblDoctor";
            lblDoctor.Size = new Size(368, 22);
            lblDoctor.TabIndex = 1;
            //
            lblImagePath.AutoSize = false;
            lblImagePath.Location = new Point(18, 366);
            lblImagePath.Name = "lblImagePath";
            lblImagePath.Size = new Size(368, 40);
            lblImagePath.TabIndex = 2;
            //
            btnApprove.Location = new Point(18, 414);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(178, 44);
            btnApprove.TabIndex = 3;
            btnApprove.Text = "Approve";
            btnApprove.Click += btnApprove_Click;
            //
            btnReject.Location = new Point(208, 414);
            btnReject.Name = "btnReject";
            btnReject.Size = new Size(178, 44);
            btnReject.TabIndex = 4;
            btnReject.Text = "Reject";
            btnReject.Click += btnReject_Click;
            //
            lblOrderItems.AutoSize = true;
            lblOrderItems.Location = new Point(20, 402);
            lblOrderItems.Name = "lblOrderItems";
            lblOrderItems.Size = new Size(300, 22);
            lblOrderItems.TabIndex = 6;
            lblOrderItems.Text = "What is on the selected order";
            //
            dgvOrderItems.Location = new Point(20, 428);
            dgvOrderItems.Name = "dgvOrderItems";
            dgvOrderItems.Size = new Size(780, 172);
            dgvOrderItems.TabIndex = 7;
            //
            lblNote.AutoSize = false;
            lblNote.Location = new Point(20, 610);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(1200, 40);
            lblNote.TabIndex = 8;
            lblNote.Text = "Approve a prescription only when it is readable and covers the medicines on the order. You can confirm the order once its prescription is approved. If you reject it, give a short reason - the customer can upload a new photo while the order is still placed.";
            //
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 656);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1200, 20);
            lblStatus.TabIndex = 9;
            //
            // VerifyPrescriptionForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1240, 686);
            Controls.Add(lblStatus);
            Controls.Add(lblNote);
            Controls.Add(dgvOrderItems);
            Controls.Add(lblOrderItems);
            Controls.Add(grpImage);
            Controls.Add(dgvQueue);
            Controls.Add(btnRefresh);
            Controls.Add(cmbVerifyStatus);
            Controls.Add(lblStatusFilter);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "VerifyPrescriptionForm";
            Text = "PharmaLink - Verify Prescriptions";
            Load += VerifyPrescriptionForm_Load;
            // Anchors are set once every size above is final, so the form can be resized.
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dgvQueue.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            picPrescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblDoctor.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblImagePath.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnApprove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnReject.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblOrderItems.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            dgvOrderItems.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            grpImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvQueue).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvOrderItems).EndInit();
            ((System.ComponentModel.ISupportInitialize)picPrescription).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private Label lblStatusFilter;
        private ComboBox cmbVerifyStatus;
        private Button btnRefresh;
        private DataGridView dgvQueue;
        private GroupBox grpImage;
        private PictureBox picPrescription;
        private Label lblImagePath;
        private Label lblDoctor;
        private Button btnApprove;
        private Button btnReject;
        private Label lblOrderItems;
        private DataGridView dgvOrderItems;
        private Label lblNote;
        private Label lblStatus;
    }
}
