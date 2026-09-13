namespace PharmaLinkApp.Forms
{
    partial class OrderHistoryForm
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
            cmbStatus = new ComboBox();
            lblPharmacyFilter = new Label();
            cmbPharmacy = new ComboBox();
            lblFrom = new Label();
            dtpFrom = new DateTimePicker();
            lblTo = new Label();
            dtpTo = new DateTimePicker();
            btnRefresh = new Button();

            dgvOrders = new DataGridView();
            lblItemsTitle = new Label();
            dgvOrderItems = new DataGridView();

            btnViewInvoice = new Button();
            btnRateReview = new Button();
            btnUploadRx = new Button();
            btnCancelOrder = new Button();
            lblNote = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOrders).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvOrderItems).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1180, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "My Orders";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Every order you have placed, with its invoice, its prescription and its Rate and Review action.";
            //
            btnBack.Location = new Point(1044, 16);
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
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Location = new Point(76, 88);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(150, 27);
            cmbStatus.TabIndex = 2;
            cmbStatus.SelectedIndexChanged += Filter_Changed;
            //
            lblPharmacyFilter.AutoSize = true;
            lblPharmacyFilter.Location = new Point(242, 92);
            lblPharmacyFilter.Name = "lblPharmacyFilter";
            lblPharmacyFilter.Size = new Size(70, 18);
            lblPharmacyFilter.TabIndex = 3;
            lblPharmacyFilter.Text = "Pharmacy";
            //
            cmbPharmacy.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPharmacy.Location = new Point(318, 88);
            cmbPharmacy.Name = "cmbPharmacy";
            cmbPharmacy.Size = new Size(220, 27);
            cmbPharmacy.TabIndex = 4;
            cmbPharmacy.SelectedIndexChanged += Filter_Changed;
            //
            lblFrom.AutoSize = true;
            lblFrom.Location = new Point(554, 92);
            lblFrom.Name = "lblFrom";
            lblFrom.Size = new Size(40, 18);
            lblFrom.TabIndex = 5;
            lblFrom.Text = "From";
            //
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.Location = new Point(600, 88);
            dtpFrom.Name = "dtpFrom";
            dtpFrom.Size = new Size(140, 27);
            dtpFrom.TabIndex = 6;
            dtpFrom.ValueChanged += Filter_Changed;
            //
            lblTo.AutoSize = true;
            lblTo.Location = new Point(752, 92);
            lblTo.Name = "lblTo";
            lblTo.Size = new Size(24, 18);
            lblTo.TabIndex = 7;
            lblTo.Text = "To";
            //
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.Location = new Point(782, 88);
            dtpTo.Name = "dtpTo";
            dtpTo.Size = new Size(140, 27);
            dtpTo.TabIndex = 8;
            dtpTo.ValueChanged += Filter_Changed;
            //
            btnRefresh.Location = new Point(940, 86);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(214, 31);
            btnRefresh.TabIndex = 9;
            btnRefresh.Text = "Refresh / reset filters";
            btnRefresh.Click += btnRefresh_Click;
            //
            dgvOrders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvOrders.Location = new Point(20, 130);
            dgvOrders.Name = "dgvOrders";
            dgvOrders.Size = new Size(1134, 260);
            dgvOrders.TabIndex = 10;
            dgvOrders.SelectionChanged += dgvOrders_SelectionChanged;
            dgvOrders.CellDoubleClick += dgvOrders_CellDoubleClick;
            //
            lblItemsTitle.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblItemsTitle.AutoSize = true;
            lblItemsTitle.Location = new Point(20, 402);
            lblItemsTitle.Name = "lblItemsTitle";
            lblItemsTitle.Size = new Size(300, 22);
            lblItemsTitle.TabIndex = 11;
            lblItemsTitle.Text = "What was on the selected order";
            //
            dgvOrderItems.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvOrderItems.Location = new Point(20, 428);
            dgvOrderItems.Name = "dgvOrderItems";
            dgvOrderItems.Size = new Size(1134, 130);
            dgvOrderItems.TabIndex = 12;
            //
            btnViewInvoice.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnViewInvoice.Location = new Point(20, 574);
            btnViewInvoice.Name = "btnViewInvoice";
            btnViewInvoice.Size = new Size(180, 40);
            btnViewInvoice.TabIndex = 13;
            btnViewInvoice.Text = "View invoice";
            btnViewInvoice.Click += btnViewInvoice_Click;
            //
            btnRateReview.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRateReview.Location = new Point(210, 574);
            btnRateReview.Name = "btnRateReview";
            btnRateReview.Size = new Size(180, 40);
            btnRateReview.TabIndex = 14;
            btnRateReview.Text = "Rate and review";
            btnRateReview.Click += btnRateReview_Click;
            //
            btnUploadRx.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUploadRx.Location = new Point(400, 574);
            btnUploadRx.Name = "btnUploadRx";
            btnUploadRx.Size = new Size(200, 40);
            btnUploadRx.TabIndex = 15;
            btnUploadRx.Text = "Upload prescription";
            btnUploadRx.Click += btnUploadRx_Click;
            //
            btnCancelOrder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCancelOrder.Location = new Point(610, 574);
            btnCancelOrder.Name = "btnCancelOrder";
            btnCancelOrder.Size = new Size(150, 40);
            btnCancelOrder.TabIndex = 16;
            btnCancelOrder.Text = "Cancel order";
            btnCancelOrder.Click += btnCancelOrder_Click;
            //
            lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblNote.AutoSize = false;
            lblNote.Location = new Point(776, 570);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(378, 48);
            lblNote.TabIndex = 17;
            lblNote.Text = "Rate and review opens once an order is delivered. You can cancel an order until the pharmacy confirms it.";
            //
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 620);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1134, 20);
            lblStatus.TabIndex = 18;
            //
            // OrderHistoryForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1180, 650);
            Controls.Add(lblStatus);
            Controls.Add(lblNote);
            Controls.Add(btnCancelOrder);
            Controls.Add(btnUploadRx);
            Controls.Add(btnRateReview);
            Controls.Add(btnViewInvoice);
            Controls.Add(dgvOrderItems);
            Controls.Add(lblItemsTitle);
            Controls.Add(dgvOrders);
            Controls.Add(btnRefresh);
            Controls.Add(dtpTo);
            Controls.Add(lblTo);
            Controls.Add(dtpFrom);
            Controls.Add(lblFrom);
            Controls.Add(cmbPharmacy);
            Controls.Add(lblPharmacyFilter);
            Controls.Add(cmbStatus);
            Controls.Add(lblStatusFilter);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "OrderHistoryForm";
            Text = "PharmaLink - My Orders";
            Load += OrderHistoryForm_Load;
            // Anchors for the header are set once every size above is final.
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOrders).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvOrderItems).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private Label lblStatusFilter;
        private ComboBox cmbStatus;
        private Label lblPharmacyFilter;
        private ComboBox cmbPharmacy;
        private Label lblFrom;
        private DateTimePicker dtpFrom;
        private Label lblTo;
        private DateTimePicker dtpTo;
        private Button btnRefresh;
        private DataGridView dgvOrders;
        private Label lblItemsTitle;
        private DataGridView dgvOrderItems;
        private Button btnViewInvoice;
        private Button btnRateReview;
        private Button btnUploadRx;
        private Button btnCancelOrder;
        private Label lblNote;
        private Label lblStatus;
    }
}
