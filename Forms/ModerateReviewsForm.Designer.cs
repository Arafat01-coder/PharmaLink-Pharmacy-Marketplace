namespace PharmaLinkApp.Forms
{
    partial class ModerateReviewsForm
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

            lblRatingFilter = new Label();
            cmbRating = new ComboBox();
            chkIncludeHidden = new CheckBox();
            btnRefresh = new Button();

            dgvReviews = new DataGridView();
            lblComment = new Label();
            txtComment = new TextBox();

            btnHide = new Button();
            btnUnhide = new Button();
            btnDismissReport = new Button();
            lblNote = new Label();
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
            panelHeader.Size = new Size(1200, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(240, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Moderate Reviews";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Every row carries the order number that proves the purchase, so a review always traces back to a real delivery.";
            //
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(1064, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            lblRatingFilter.AutoSize = true;
            lblRatingFilter.Location = new Point(20, 92);
            lblRatingFilter.Name = "lblRatingFilter";
            lblRatingFilter.Size = new Size(90, 18);
            lblRatingFilter.TabIndex = 1;
            lblRatingFilter.Text = "Show ratings";
            //
            cmbRating.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRating.Location = new Point(116, 88);
            cmbRating.Name = "cmbRating";
            cmbRating.Size = new Size(220, 27);
            cmbRating.TabIndex = 2;
            cmbRating.SelectedIndexChanged += Filter_Changed;
            //
            chkIncludeHidden.AutoSize = true;
            chkIncludeHidden.Location = new Point(356, 91);
            chkIncludeHidden.Name = "chkIncludeHidden";
            chkIncludeHidden.Size = new Size(230, 22);
            chkIncludeHidden.TabIndex = 3;
            chkIncludeHidden.Text = "Include reviews already hidden";
            chkIncludeHidden.CheckedChanged += Filter_Changed;
            //
            btnRefresh.Location = new Point(604, 86);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(110, 31);
            btnRefresh.TabIndex = 4;
            btnRefresh.Text = "Refresh";
            btnRefresh.Click += Filter_Changed;
            //
            dgvReviews.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvReviews.Location = new Point(20, 130);
            dgvReviews.Name = "dgvReviews";
            dgvReviews.Size = new Size(1154, 330);
            dgvReviews.TabIndex = 5;
            dgvReviews.SelectionChanged += dgvReviews_SelectionChanged;
            //
            lblComment.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblComment.AutoSize = true;
            lblComment.Location = new Point(20, 472);
            lblComment.Name = "lblComment";
            lblComment.Size = new Size(200, 18);
            lblComment.TabIndex = 6;
            lblComment.Text = "Full text of the selected review";
            //
            txtComment.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtComment.Location = new Point(20, 494);
            txtComment.Multiline = true;
            txtComment.Name = "txtComment";
            txtComment.ReadOnly = true;
            txtComment.ScrollBars = ScrollBars.Vertical;
            txtComment.Size = new Size(720, 84);
            txtComment.TabIndex = 7;
            //
            btnHide.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnHide.Location = new Point(760, 494);
            btnHide.Name = "btnHide";
            btnHide.Size = new Size(200, 38);
            btnHide.TabIndex = 8;
            btnHide.Text = "Hide review";
            btnHide.Click += btnHide_Click;
            //
            btnUnhide.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnUnhide.Location = new Point(974, 494);
            btnUnhide.Name = "btnUnhide";
            btnUnhide.Size = new Size(200, 38);
            btnUnhide.TabIndex = 9;
            btnUnhide.Text = "Restore review";
            btnUnhide.Click += btnUnhide_Click;
            //
            btnDismissReport.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDismissReport.Location = new Point(760, 538);
            btnDismissReport.Name = "btnDismissReport";
            btnDismissReport.Size = new Size(414, 34);
            btnDismissReport.TabIndex = 10;
            btnDismissReport.Text = "Dismiss report (keep the review visible)";
            btnDismissReport.Click += btnDismissReport_Click;
            //
            lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblNote.AutoSize = false;
            lblNote.Location = new Point(760, 578);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(414, 56);
            lblNote.TabIndex = 11;
            lblNote.Text = "Hiding removes the review from the customer screens and from every average rating, but keeps it so it can be restored. Hiding a reported review also closes the report.";
            //
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 592);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(720, 40);
            lblStatus.TabIndex = 12;
            //
            // ModerateReviewsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 640);
            Controls.Add(lblStatus);
            Controls.Add(lblNote);
            Controls.Add(btnDismissReport);
            Controls.Add(btnUnhide);
            Controls.Add(btnHide);
            Controls.Add(txtComment);
            Controls.Add(lblComment);
            Controls.Add(dgvReviews);
            Controls.Add(btnRefresh);
            Controls.Add(chkIncludeHidden);
            Controls.Add(cmbRating);
            Controls.Add(lblRatingFilter);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "ModerateReviewsForm";
            Text = "PharmaLink - Moderate Reviews";
            Load += ModerateReviewsForm_Load;
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
        private Label lblRatingFilter;
        private ComboBox cmbRating;
        private CheckBox chkIncludeHidden;
        private Button btnRefresh;
        private DataGridView dgvReviews;
        private Label lblComment;
        private TextBox txtComment;
        private Button btnHide;
        private Button btnUnhide;
        private Button btnDismissReport;
        private Label lblNote;
        private Label lblStatus;
    }
}
