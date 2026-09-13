namespace PharmaLinkApp.Forms
{
    partial class GiveRatingForm
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

            lblMedicine = new Label();
            cmbMedicine = new ComboBox();
            lblMedicineError = new Label();

            lblRatingCaption = new Label();
            btnStar1 = new Button();
            btnStar2 = new Button();
            btnStar3 = new Button();
            btnStar4 = new Button();
            btnStar5 = new Button();
            lblRatingWord = new Label();
            lblRatingError = new Label();

            lblComment = new Label();
            txtComment = new TextBox();
            lblCharCount = new Label();

            lblRuleNote = new Label();
            btnSubmit = new Button();
            btnCancel = new Button();

            panelHeader.SuspendLayout();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(640, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Rate and review";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(500, 16);
            lblSubtitle.TabIndex = 1;
            //
            lblMedicine.AutoSize = true;
            lblMedicine.Location = new Point(20, 88);
            lblMedicine.Name = "lblMedicine";
            lblMedicine.Size = new Size(260, 18);
            lblMedicine.TabIndex = 1;
            lblMedicine.Text = "Which medicine are you reviewing?";
            //
            cmbMedicine.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMedicine.Location = new Point(20, 110);
            cmbMedicine.Name = "cmbMedicine";
            cmbMedicine.Size = new Size(600, 27);
            cmbMedicine.TabIndex = 2;
            cmbMedicine.SelectedIndexChanged += Field_Changed;
            //
            lblMedicineError.AutoSize = false;
            lblMedicineError.Location = new Point(20, 138);
            lblMedicineError.Name = "lblMedicineError";
            lblMedicineError.Size = new Size(600, 18);
            lblMedicineError.TabIndex = 3;
            lblMedicineError.Visible = false;
            //
            lblRatingCaption.AutoSize = true;
            lblRatingCaption.Location = new Point(20, 168);
            lblRatingCaption.Name = "lblRatingCaption";
            lblRatingCaption.Size = new Size(200, 18);
            lblRatingCaption.TabIndex = 4;
            lblRatingCaption.Text = "How many stars?";
            //
            btnStar1.Location = new Point(20, 192);
            btnStar1.Name = "btnStar1";
            btnStar1.Size = new Size(56, 50);
            btnStar1.TabIndex = 5;
            btnStar1.Tag = "1";
            btnStar1.Text = "1";
            btnStar1.Click += Star_Click;
            //
            btnStar2.Location = new Point(82, 192);
            btnStar2.Name = "btnStar2";
            btnStar2.Size = new Size(56, 50);
            btnStar2.TabIndex = 6;
            btnStar2.Tag = "2";
            btnStar2.Text = "2";
            btnStar2.Click += Star_Click;
            //
            btnStar3.Location = new Point(144, 192);
            btnStar3.Name = "btnStar3";
            btnStar3.Size = new Size(56, 50);
            btnStar3.TabIndex = 7;
            btnStar3.Tag = "3";
            btnStar3.Text = "3";
            btnStar3.Click += Star_Click;
            //
            btnStar4.Location = new Point(206, 192);
            btnStar4.Name = "btnStar4";
            btnStar4.Size = new Size(56, 50);
            btnStar4.TabIndex = 8;
            btnStar4.Tag = "4";
            btnStar4.Text = "4";
            btnStar4.Click += Star_Click;
            //
            btnStar5.Location = new Point(268, 192);
            btnStar5.Name = "btnStar5";
            btnStar5.Size = new Size(56, 50);
            btnStar5.TabIndex = 9;
            btnStar5.Tag = "5";
            btnStar5.Text = "5";
            btnStar5.Click += Star_Click;
            //
            lblRatingWord.AutoSize = false;
            lblRatingWord.Location = new Point(340, 192);
            lblRatingWord.Name = "lblRatingWord";
            lblRatingWord.Size = new Size(280, 50);
            lblRatingWord.TextAlign = ContentAlignment.MiddleLeft;
            lblRatingWord.TabIndex = 10;
            //
            lblRatingError.AutoSize = false;
            lblRatingError.Location = new Point(20, 246);
            lblRatingError.Name = "lblRatingError";
            lblRatingError.Size = new Size(600, 18);
            lblRatingError.TabIndex = 11;
            lblRatingError.Visible = false;
            //
            lblComment.AutoSize = true;
            lblComment.Location = new Point(20, 274);
            lblComment.Name = "lblComment";
            lblComment.Size = new Size(300, 18);
            lblComment.TabIndex = 12;
            lblComment.Text = "Tell other patients what happened (optional)";
            //
            txtComment.Location = new Point(20, 296);
            txtComment.MaxLength = 500;
            txtComment.Multiline = true;
            txtComment.Name = "txtComment";
            txtComment.ScrollBars = ScrollBars.Vertical;
            txtComment.Size = new Size(600, 120);
            txtComment.TabIndex = 13;
            txtComment.TextChanged += txtComment_TextChanged;
            //
            lblCharCount.AutoSize = false;
            lblCharCount.Location = new Point(20, 420);
            lblCharCount.Name = "lblCharCount";
            lblCharCount.Size = new Size(600, 18);
            lblCharCount.TextAlign = ContentAlignment.MiddleRight;
            lblCharCount.TabIndex = 14;
            //
            lblRuleNote.AutoSize = false;
            lblRuleNote.Location = new Point(20, 444);
            lblRuleNote.Name = "lblRuleNote";
            lblRuleNote.Size = new Size(600, 66);
            lblRuleNote.TabIndex = 15;
            lblRuleNote.Text = "You can review each medicine from a delivered order once. Your review appears on the medicine's details page for other patients to read.";
            //
            btnSubmit.Location = new Point(330, 520);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(170, 44);
            btnSubmit.TabIndex = 16;
            btnSubmit.Text = "Submit review";
            btnSubmit.Click += btnSubmit_Click;
            //
            btnCancel.Location = new Point(510, 520);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(110, 44);
            btnCancel.TabIndex = 17;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            //
            // GiveRatingForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(640, 582);
            Controls.Add(btnCancel);
            Controls.Add(btnSubmit);
            Controls.Add(lblRuleNote);
            Controls.Add(lblCharCount);
            Controls.Add(txtComment);
            Controls.Add(lblComment);
            Controls.Add(lblRatingError);
            Controls.Add(lblRatingWord);
            Controls.Add(btnStar5);
            Controls.Add(btnStar4);
            Controls.Add(btnStar3);
            Controls.Add(btnStar2);
            Controls.Add(btnStar1);
            Controls.Add(lblRatingCaption);
            Controls.Add(lblMedicineError);
            Controls.Add(cmbMedicine);
            Controls.Add(lblMedicine);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GiveRatingForm";
            ShowInTaskbar = false;
            Text = "PharmaLink - Rate and Review";
            Load += GiveRatingForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblMedicine;
        private ComboBox cmbMedicine;
        private Label lblMedicineError;
        private Label lblRatingCaption;
        private Button btnStar1;
        private Button btnStar2;
        private Button btnStar3;
        private Button btnStar4;
        private Button btnStar5;
        private Label lblRatingWord;
        private Label lblRatingError;
        private Label lblComment;
        private TextBox txtComment;
        private Label lblCharCount;
        private Label lblRuleNote;
        private Button btnSubmit;
        private Button btnCancel;
    }
}
