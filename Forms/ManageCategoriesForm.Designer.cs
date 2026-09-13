namespace PharmaLinkApp.Forms
{
    partial class ManageCategoriesForm
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

            dgvCategories = new DataGridView();
            chkShowInactive = new CheckBox();

            grpEditor = new GroupBox();
            lblName = new Label();
            txtName = new TextBox();
            lblNameError = new Label();
            lblDescription = new Label();
            txtDescription = new TextBox();
            btnAdd = new Button();
            btnUpdate = new Button();
            btnDeactivate = new Button();
            btnActivate = new Button();
            btnNew = new Button();
            lblEditorNote = new Label();

            lblStatus = new Label();

            panelHeader.SuspendLayout();
            grpEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCategories).BeginInit();
            SuspendLayout();
            //
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(btnBack);
            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1040, 68);
            panelHeader.TabIndex = 0;
            //
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(240, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Medicine Categories";
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(23, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(600, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "The master list every pharmacy classifies its medicines against.";
            //
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Location = new Point(904, 16);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(110, 36);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.Click += btnBack_Click;
            //
            dgvCategories.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvCategories.Location = new Point(20, 116);
            dgvCategories.Name = "dgvCategories";
            dgvCategories.Size = new Size(620, 460);
            dgvCategories.TabIndex = 1;
            dgvCategories.SelectionChanged += dgvCategories_SelectionChanged;
            //
            chkShowInactive.AutoSize = true;
            chkShowInactive.Location = new Point(22, 88);
            chkShowInactive.Name = "chkShowInactive";
            chkShowInactive.Size = new Size(220, 22);
            chkShowInactive.TabIndex = 2;
            chkShowInactive.Text = "Show deactivated categories too";
            chkShowInactive.Checked = true;
            chkShowInactive.CheckedChanged += chkShowInactive_CheckedChanged;
            //
            grpEditor.Controls.Add(lblName);
            grpEditor.Controls.Add(txtName);
            grpEditor.Controls.Add(lblNameError);
            grpEditor.Controls.Add(lblDescription);
            grpEditor.Controls.Add(txtDescription);
            grpEditor.Controls.Add(btnAdd);
            grpEditor.Controls.Add(btnUpdate);
            grpEditor.Controls.Add(btnDeactivate);
            grpEditor.Controls.Add(btnActivate);
            grpEditor.Controls.Add(btnNew);
            grpEditor.Controls.Add(lblEditorNote);
            grpEditor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpEditor.Location = new Point(660, 116);
            grpEditor.Name = "grpEditor";
            grpEditor.Size = new Size(354, 460);
            grpEditor.TabIndex = 3;
            grpEditor.TabStop = false;
            grpEditor.Text = "  Category details  ";
            //
            lblName.AutoSize = true;
            lblName.Location = new Point(18, 34);
            lblName.Name = "lblName";
            lblName.Size = new Size(110, 18);
            lblName.TabIndex = 0;
            lblName.Text = "Category name";
            //
            txtName.Location = new Point(18, 56);
            txtName.MaxLength = 60;
            txtName.Name = "txtName";
            txtName.Size = new Size(316, 27);
            txtName.TabIndex = 1;
            txtName.TextChanged += txtName_TextChanged;
            //
            lblNameError.AutoSize = false;
            lblNameError.Location = new Point(18, 86);
            lblNameError.Name = "lblNameError";
            lblNameError.Size = new Size(316, 32);
            lblNameError.TabIndex = 2;
            lblNameError.Visible = false;
            //
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(18, 122);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(90, 18);
            lblDescription.TabIndex = 3;
            lblDescription.Text = "Description";
            //
            txtDescription.Location = new Point(18, 144);
            txtDescription.MaxLength = 200;
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(316, 70);
            txtDescription.TabIndex = 4;
            //
            btnAdd.Location = new Point(18, 234);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(152, 38);
            btnAdd.TabIndex = 5;
            btnAdd.Text = "Add new";
            btnAdd.Click += btnAdd_Click;
            //
            btnUpdate.Location = new Point(182, 234);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(152, 38);
            btnUpdate.TabIndex = 6;
            btnUpdate.Text = "Save changes";
            btnUpdate.Click += btnUpdate_Click;
            //
            btnDeactivate.Location = new Point(18, 280);
            btnDeactivate.Name = "btnDeactivate";
            btnDeactivate.Size = new Size(152, 38);
            btnDeactivate.TabIndex = 7;
            btnDeactivate.Text = "Deactivate";
            btnDeactivate.Click += btnDeactivate_Click;
            //
            btnActivate.Location = new Point(182, 280);
            btnActivate.Name = "btnActivate";
            btnActivate.Size = new Size(152, 38);
            btnActivate.TabIndex = 8;
            btnActivate.Text = "Reactivate";
            btnActivate.Click += btnActivate_Click;
            //
            btnNew.Location = new Point(18, 326);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(316, 32);
            btnNew.TabIndex = 9;
            btnNew.Text = "Clear the form";
            btnNew.Click += btnNew_Click;
            //
            lblEditorNote.AutoSize = false;
            lblEditorNote.Location = new Point(18, 368);
            lblEditorNote.Name = "lblEditorNote";
            lblEditorNote.Size = new Size(316, 80);
            lblEditorNote.TabIndex = 10;
            lblEditorNote.Text = "A category that medicines already use is never deleted, only deactivated: it disappears from the dropdowns while existing medicines keep it. Category names must be unique, so a second 'Antibiotic' is refused with a red message under the name.";
            //
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(20, 588);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(994, 20);
            lblStatus.TabIndex = 4;
            //
            // ManageCategoriesForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1040, 622);
            Controls.Add(lblStatus);
            Controls.Add(grpEditor);
            Controls.Add(chkShowInactive);
            Controls.Add(dgvCategories);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "ManageCategoriesForm";
            Text = "PharmaLink - Medicine Categories";
            Load += ManageCategoriesForm_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            grpEditor.ResumeLayout(false);
            grpEditor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCategories).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnBack;
        private DataGridView dgvCategories;
        private CheckBox chkShowInactive;
        private GroupBox grpEditor;
        private Label lblName;
        private TextBox txtName;
        private Label lblNameError;
        private Label lblDescription;
        private TextBox txtDescription;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDeactivate;
        private Button btnActivate;
        private Button btnNew;
        private Label lblEditorNote;
        private Label lblStatus;
    }
}
