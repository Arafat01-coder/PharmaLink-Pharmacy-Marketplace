namespace PharmaLinkApp.Forms
{
    partial class SuperAdminManageShopsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            btnBack = new Button();

            lblSearch = new Label();
            txtSearch = new TextBox();
            lblStatusFilter = new Label();
            cmbStatus = new ComboBox();
            lblAreaFilter = new Label();
            cmbArea = new ComboBox();
            btnSearch = new Button();
            btnClear = new Button();

            dgvPharmacies = new DataGridView();

            btnApprove = new Button();
            btnSuspend = new Button();
            btnReinstate = new Button();
            btnDelete = new Button();
            lblCommission = new Label();
            txtCommission = new TextBox();
            btnSetCommission = new Button();
            lblCommissionError = new Label();

            lblNote = new Label();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPharmacies).BeginInit();
            SuspendLayout();
            //
            // panelHeader
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
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(240, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Manage Pharmacies";
            //
            // lblSubtitle
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(600, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Approve a licence, suspend a shop, delete a registration or change one pharmacy's commission rate.";
            //
            // btnBack
            //
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(1064, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            // lblSearch
            //
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(20, 90);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(50, 18);
            lblSearch.TabIndex = 1;
            lblSearch.Text = "Search";
            //
            // txtSearch
            //
            txtSearch.Location = new Point(76, 86);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "shop name, owner or licence number";
            txtSearch.Size = new Size(280, 27);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += txtSearch_TextChanged;
            //
            // lblStatusFilter
            //
            lblStatusFilter.AutoSize = true;
            lblStatusFilter.Location = new Point(374, 90);
            lblStatusFilter.Name = "lblStatusFilter";
            lblStatusFilter.Size = new Size(50, 18);
            lblStatusFilter.TabIndex = 3;
            lblStatusFilter.Text = "Status";
            //
            // cmbStatus
            //
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Location = new Point(430, 86);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(150, 27);
            cmbStatus.TabIndex = 4;
            cmbStatus.SelectedIndexChanged += Filter_Changed;
            //
            // lblAreaFilter
            //
            lblAreaFilter.AutoSize = true;
            lblAreaFilter.Location = new Point(598, 90);
            lblAreaFilter.Name = "lblAreaFilter";
            lblAreaFilter.Size = new Size(40, 18);
            lblAreaFilter.TabIndex = 5;
            lblAreaFilter.Text = "Area";
            //
            // cmbArea
            //
            cmbArea.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbArea.Location = new Point(644, 86);
            cmbArea.Name = "cmbArea";
            cmbArea.Size = new Size(170, 27);
            cmbArea.TabIndex = 6;
            cmbArea.SelectedIndexChanged += Filter_Changed;
            //
            // btnSearch
            //
            btnSearch.Location = new Point(834, 84);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(100, 31);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "Search";
            btnSearch.Click += btnSearch_Click;
            //
            // btnClear
            //
            btnClear.Location = new Point(944, 84);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(100, 31);
            btnClear.TabIndex = 8;
            btnClear.Text = "Clear filters";
            btnClear.Click += btnClear_Click;
            //
            // dgvPharmacies
            //
            dgvPharmacies.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvPharmacies.Location = new Point(20, 128);
            dgvPharmacies.Name = "dgvPharmacies";
            dgvPharmacies.Size = new Size(1154, 396);
            dgvPharmacies.TabIndex = 9;
            dgvPharmacies.SelectionChanged += dgvPharmacies_SelectionChanged;
            //
            // btnApprove
            //
            btnApprove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnApprove.Location = new Point(20, 538);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(140, 36);
            btnApprove.TabIndex = 10;
            btnApprove.Text = "Approve";
            btnApprove.Click += btnApprove_Click;
            //
            // btnSuspend
            //
            btnSuspend.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSuspend.Location = new Point(170, 538);
            btnSuspend.Name = "btnSuspend";
            btnSuspend.Size = new Size(140, 36);
            btnSuspend.TabIndex = 11;
            btnSuspend.Text = "Suspend";
            btnSuspend.Click += btnSuspend_Click;
            //
            // btnReinstate
            //
            btnReinstate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnReinstate.Location = new Point(320, 538);
            btnReinstate.Name = "btnReinstate";
            btnReinstate.Size = new Size(140, 36);
            btnReinstate.TabIndex = 12;
            btnReinstate.Text = "Reinstate";
            btnReinstate.Click += btnReinstate_Click;
            //
            // btnDelete
            //
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(470, 538);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(140, 36);
            btnDelete.TabIndex = 13;
            btnDelete.Text = "Delete";
            btnDelete.Click += btnDelete_Click;
            //
            // lblCommission
            //
            lblCommission.AutoSize = true;
            lblCommission.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblCommission.Location = new Point(672, 546);
            lblCommission.Name = "lblCommission";
            lblCommission.Size = new Size(170, 18);
            lblCommission.TabIndex = 14;
            lblCommission.Text = "Commission rate (0 - 30 %)";
            //
            // txtCommission
            //
            txtCommission.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtCommission.Location = new Point(848, 542);
            txtCommission.Name = "txtCommission";
            txtCommission.Size = new Size(70, 27);
            txtCommission.TabIndex = 15;
            txtCommission.TextChanged += txtCommission_TextChanged;
            //
            // btnSetCommission
            //
            btnSetCommission.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSetCommission.Location = new Point(928, 538);
            btnSetCommission.Name = "btnSetCommission";
            btnSetCommission.Size = new Size(150, 36);
            btnSetCommission.TabIndex = 16;
            btnSetCommission.Text = "Save rate";
            btnSetCommission.Click += btnSetCommission_Click;
            //
            // lblCommissionError
            //
            lblCommissionError.AutoSize = false;
            lblCommissionError.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblCommissionError.Location = new Point(672, 572);
            lblCommissionError.Name = "lblCommissionError";
            lblCommissionError.Size = new Size(410, 18);
            lblCommissionError.TabIndex = 17;
            lblCommissionError.Visible = false;
            //
            // lblNote
            //
            lblNote.AutoSize = false;
            lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblNote.Location = new Point(20, 588);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(640, 56);
            lblNote.TabIndex = 18;
            lblNote.Text = "Suspending a pharmacy signs its owner out and hides the shop from customers. Nothing is deleted, so past orders and invoices stay valid and the shop's medicine list is kept for if it is reinstated. A rejected registration can still be approved later. Delete is only available for a shop that has never taken an order.";
            //
            // lblStatus
            //
            lblStatus.AutoSize = false;
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Location = new Point(20, 648);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1154, 20);
            lblStatus.TabIndex = 19;
            //
            // SuperAdminManageShopsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 676);
            Controls.Add(lblStatus);
            Controls.Add(lblNote);
            Controls.Add(lblCommissionError);
            Controls.Add(btnSetCommission);
            Controls.Add(txtCommission);
            Controls.Add(lblCommission);
            Controls.Add(btnDelete);
            Controls.Add(btnReinstate);
            Controls.Add(btnSuspend);
            Controls.Add(btnApprove);
            Controls.Add(dgvPharmacies);
            Controls.Add(btnClear);
            Controls.Add(btnSearch);
            Controls.Add(cmbArea);
            Controls.Add(lblAreaFilter);
            Controls.Add(cmbStatus);
            Controls.Add(lblStatusFilter);
            Controls.Add(txtSearch);
            Controls.Add(lblSearch);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SuperAdminManageShopsForm";
            Text = "PharmaLink - Manage Pharmacies";
            Load += SuperAdminManageShopsForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPharmacies).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private Label lblSearch;
        private TextBox txtSearch;
        private Label lblStatusFilter;
        private ComboBox cmbStatus;
        private Label lblAreaFilter;
        private ComboBox cmbArea;
        private Button btnSearch;
        private Button btnClear;
        private DataGridView dgvPharmacies;
        private Button btnApprove;
        private Button btnSuspend;
        private Button btnReinstate;
        private Button btnDelete;
        private Label lblCommission;
        private TextBox txtCommission;
        private Button btnSetCommission;
        private Label lblCommissionError;
        private Label lblNote;
        private Label lblStatus;
    }
}
