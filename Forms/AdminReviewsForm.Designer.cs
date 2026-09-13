namespace PharmaLinkApp.Forms
{
    partial class AdminReviewsForm
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

            lblAverage = new Label();
            lblRatingFilter = new Label();
            cmbRating = new ComboBox();
            btnRefresh = new Button();

            dgvReviews = new DataGridView();
            lblComment = new Label();
            txtComment = new TextBox();
            btnReport = new Button();
            lblReadOnlyNote = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReviews).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1120, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(260, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Customer Reviews";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "What your customers said. This screen is read only by design - there is no Delete button anywhere on it.";
            //
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(984, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            lblAverage.AutoSize = true;
            lblAverage.Location = new Point(20, 86);
            lblAverage.Name = "lblAverage";
            lblAverage.Size = new Size(400, 28);
            lblAverage.TabIndex = 1;
            //
            lblRatingFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRatingFilter.AutoSize = true;
            lblRatingFilter.Location = new Point(640, 92);
            lblRatingFilter.Name = "lblRatingFilter";
            lblRatingFilter.Size = new Size(90, 18);
            lblRatingFilter.TabIndex = 2;
            lblRatingFilter.Text = "Show ratings";
            //
            cmbRating.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmbRating.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRating.Location = new Point(736, 88);
            cmbRating.Name = "cmbRating";
            cmbRating.Size = new Size(240, 27);
            cmbRating.TabIndex = 3;
            cmbRating.SelectedIndexChanged += Filter_Changed;
            //
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(986, 86);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(108, 31);
            btnRefresh.TabIndex = 4;
            btnRefresh.Text = "Refresh";
            btnRefresh.Click += Filter_Changed;
            //
            dgvReviews.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvReviews.Location = new Point(20, 128);
            dgvReviews.Name = "dgvReviews";
            dgvReviews.Size = new Size(1074, 320);
            dgvReviews.TabIndex = 5;
            dgvReviews.SelectionChanged += dgvReviews_SelectionChanged;
            //
            lblComment.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblComment.AutoSize = true;
            lblComment.Location = new Point(20, 460);
            lblComment.Name = "lblComment";
            lblComment.Size = new Size(220, 18);
            lblComment.TabIndex = 6;
            lblComment.Text = "Full text of the selected review";
            //
            txtComment.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtComment.Location = new Point(20, 482);
            txtComment.Multiline = true;
            txtComment.Name = "txtComment";
            txtComment.ReadOnly = true;
            txtComment.ScrollBars = ScrollBars.Vertical;
            txtComment.Size = new Size(700, 80);
            txtComment.TabIndex = 7;
            //
            btnReport.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnReport.Location = new Point(740, 482);
            btnReport.Name = "btnReport";
            btnReport.Size = new Size(354, 40);
            btnReport.TabIndex = 8;
            btnReport.Text = "Report this review to the Super Admin";
            btnReport.Click += btnReport_Click;
            //
            lblReadOnlyNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblReadOnlyNote.AutoSize = false;
            lblReadOnlyNote.Location = new Point(740, 528);
            lblReadOnlyNote.Name = "lblReadOnlyNote";
            lblReadOnlyNote.Size = new Size(354, 60);
            lblReadOnlyNote.TabIndex = 9;
            lblReadOnlyNote.Text = "A pharmacy owner cannot edit or delete a review about his own shop. If you believe a review is abusive, report it and the Super Admin decides. That is what keeps the ratings on this platform worth reading.";
            //
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 574);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(700, 40);
            lblStatus.TabIndex = 10;
            //
            // AdminReviewsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 620);
            Controls.Add(lblStatus);
            Controls.Add(lblReadOnlyNote);
            Controls.Add(btnReport);
            Controls.Add(txtComment);
            Controls.Add(lblComment);
            Controls.Add(dgvReviews);
            Controls.Add(btnRefresh);
            Controls.Add(cmbRating);
            Controls.Add(lblRatingFilter);
            Controls.Add(lblAverage);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "AdminReviewsForm";
            Text = "PharmaLink - Customer Reviews";
            Load += AdminReviewsForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReviews).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private Label lblAverage;
        private Label lblRatingFilter;
        private ComboBox cmbRating;
        private Button btnRefresh;
        private DataGridView dgvReviews;
        private Label lblComment;
        private TextBox txtComment;
        private Button btnReport;
        private Label lblReadOnlyNote;
        private Label lblStatus;
    }
}
