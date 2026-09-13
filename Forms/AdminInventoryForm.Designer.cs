namespace PharmaLinkApp.Forms
{
    partial class AdminInventoryForm
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

            lblAlertTitle = new Label();
            dgvLowStock = new DataGridView();
            lblRestock = new Label();
            txtRestockUnits = new TextBox();
            btnRestock = new Button();
            btnOpenEditor = new Button();
            lblRestockError = new Label();

            lblInventoryTitle = new Label();
            dgvInventory = new DataGridView();
            lblStatus = new Label();

            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLowStock).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvInventory).BeginInit();
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
            lblTitle.Size = new Size(280, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Stock and Inventory";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "How many units were sold, how many are left, and what needs reordering.";
            //
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(1064, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            lblAlertTitle.AutoSize = true;
            lblAlertTitle.Location = new Point(20, 186);
            lblAlertTitle.Name = "lblAlertTitle";
            lblAlertTitle.Size = new Size(400, 22);
            lblAlertTitle.TabIndex = 1;
            lblAlertTitle.Text = "Low stock alert";
            //
            dgvLowStock.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvLowStock.Location = new Point(20, 212);
            dgvLowStock.Name = "dgvLowStock";
            dgvLowStock.Size = new Size(1154, 150);
            dgvLowStock.TabIndex = 2;
            dgvLowStock.SelectionChanged += dgvLowStock_SelectionChanged;
            //
            lblRestock.AutoSize = true;
            lblRestock.Location = new Point(20, 380);
            lblRestock.Name = "lblRestock";
            lblRestock.Size = new Size(140, 18);
            lblRestock.TabIndex = 3;
            lblRestock.Text = "Units to add to stock";
            //
            txtRestockUnits.Location = new Point(170, 376);
            txtRestockUnits.Name = "txtRestockUnits";
            txtRestockUnits.Size = new Size(100, 27);
            txtRestockUnits.TabIndex = 4;
            txtRestockUnits.TextChanged += txtRestockUnits_TextChanged;
            //
            btnRestock.Location = new Point(284, 372);
            btnRestock.Name = "btnRestock";
            btnRestock.Size = new Size(170, 36);
            btnRestock.TabIndex = 5;
            btnRestock.Text = "Restock selected";
            btnRestock.Click += btnRestock_Click;
            //
            btnOpenEditor.Location = new Point(464, 372);
            btnOpenEditor.Name = "btnOpenEditor";
            btnOpenEditor.Size = new Size(200, 36);
            btnOpenEditor.TabIndex = 6;
            btnOpenEditor.Text = "Open the medicine editor";
            btnOpenEditor.Click += btnOpenEditor_Click;
            //
            lblRestockError.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblRestockError.AutoSize = false;
            lblRestockError.Location = new Point(680, 382);
            lblRestockError.Name = "lblRestockError";
            lblRestockError.Size = new Size(494, 18);
            lblRestockError.TabIndex = 7;
            lblRestockError.Visible = false;
            //
            lblInventoryTitle.AutoSize = true;
            lblInventoryTitle.Location = new Point(20, 420);
            lblInventoryTitle.Name = "lblInventoryTitle";
            lblInventoryTitle.Size = new Size(400, 22);
            lblInventoryTitle.TabIndex = 8;
            lblInventoryTitle.Text = "Full inventory - sold, remaining and stock value";
            //
            dgvInventory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvInventory.Location = new Point(20, 446);
            dgvInventory.Name = "dgvInventory";
            dgvInventory.Size = new Size(1154, 202);
            dgvInventory.TabIndex = 9;
            dgvInventory.CellDoubleClick += dgvInventory_CellDoubleClick;
            //
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 656);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1154, 20);
            lblStatus.TabIndex = 10;
            //
            // AdminInventoryForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 686);
            Controls.Add(lblStatus);
            Controls.Add(dgvInventory);
            Controls.Add(lblInventoryTitle);
            Controls.Add(lblRestockError);
            Controls.Add(btnOpenEditor);
            Controls.Add(btnRestock);
            Controls.Add(txtRestockUnits);
            Controls.Add(lblRestock);
            Controls.Add(dgvLowStock);
            Controls.Add(lblAlertTitle);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "AdminInventoryForm";
            Text = "PharmaLink - Stock and Inventory";
            Load += AdminInventoryForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLowStock).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvInventory).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private Label lblAlertTitle;
        private DataGridView dgvLowStock;
        private Label lblRestock;
        private TextBox txtRestockUnits;
        private Button btnRestock;
        private Button btnOpenEditor;
        private Label lblRestockError;
        private Label lblInventoryTitle;
        private DataGridView dgvInventory;
        private Label lblStatus;
    }
}
