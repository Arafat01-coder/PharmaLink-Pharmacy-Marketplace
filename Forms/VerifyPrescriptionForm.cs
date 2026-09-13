using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 16. The pharmacy's prescription verification queue.
    ///
    /// When an order contains a medicine whose RequiresRx flag is set, it
    /// appears here with the uploaded image and the doctor's name. Approve or
    /// Reject sets Prescriptions.VerifyStatus on the order's current
    /// prescription, and only an order whose current prescription is Approved
    /// can be moved to Confirmed.
    ///
    /// A decision is only offered - and only accepted by the database - while
    /// the prescription is Pending and its order is still Placed. A rejection
    /// carries a short reason the customer sees in My Orders, where they can
    /// upload a new photograph.
    /// </summary>
    public partial class VerifyPrescriptionForm : Form
    {
        private readonly PrescriptionService _prescriptions = new PrescriptionService();
        private readonly OrderService _orders = new OrderService();
        private bool _loading = true;

        /// <summary>True while the grid is being rebound, so SelectionChanged does not query per row.</summary>
        private bool _binding;

        public VerifyPrescriptionForm()
        {
            InitializeComponent();
        }

        private void VerifyPrescriptionForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            UiTheme.MakeResizable(this, Size);

            // The preview is a private copy of the file; release it with the form.
            FormClosed += (s, args) => ClearImage();

            cmbVerifyStatus.Items.AddRange(new object[] { "Pending", "Approved", "Rejected", "All" });
            cmbVerifyStatus.SelectedIndex = 0;

            _loading = false;
            LoadQueue();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Verify Prescriptions");

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            grpImage.Font = UiTheme.FontHeading;
            grpImage.ForeColor = UiTheme.Primary;
            grpImage.BackColor = UiTheme.CardBack;

            lblOrderItems.Font = UiTheme.FontHeading;
            lblOrderItems.ForeColor = UiTheme.TextDark;
            lblDoctor.Font = UiTheme.FontBody;
            lblDoctor.ForeColor = UiTheme.TextDark;
            lblImagePath.Font = UiTheme.FontSmall;
            lblImagePath.ForeColor = UiTheme.TextMuted;
            lblNote.Font = UiTheme.FontSmall;
            lblNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSecondary(btnRefresh);
            UiTheme.StyleSuccess(btnApprove);
            UiTheme.StyleDanger(btnReject);
            UiTheme.StyleGrid(dgvQueue);
            UiTheme.StyleGrid(dgvOrderItems);
            UiTheme.EnableEmptyMessage(dgvQueue, "No prescriptions match this filter.");
            dgvQueue.DataBindingComplete += dgvQueue_DataBindingComplete;
        }

        private int SelectedPrescriptionId()
        {
            DataGridViewRow row = dgvQueue.CurrentRow;
            if (row == null || row.Cells["PrescriptionId"].Value == null || row.Cells["PrescriptionId"].Value == DBNull.Value)
                return 0;
            return Convert.ToInt32(row.Cells["PrescriptionId"].Value);
        }

        /// <summary>Rebinds the queue, keeps the same row selected, then shows <paramref name="statusMessage"/> if given.</summary>
        private void LoadQueue(string statusMessage = null)
        {
            if (_loading) return;

            try
            {
                string status = cmbVerifyStatus.SelectedItem.ToString();
                if (status == "All") status = "";

                int keepId = dgvQueue.Columns.Count > 0 ? SelectedPrescriptionId() : 0;
                DataTable table = _prescriptions.GetQueueForPharmacy(UserSession.PharmacyId, status);

                _binding = true;
                try
                {
                    dgvQueue.DataSource = table;

                    if (dgvQueue.Columns.Count > 0)
                    {
                        // The prescription id is only needed to act on the row, so it is
                        // hidden; short headers and DPI-scaled floors let the queue fit
                        // beside the image preview without a horizontal scrollbar.
                        dgvQueue.Columns["PrescriptionId"].Visible = false;
                        dgvQueue.Columns["OrderId"].HeaderText = "Order";
                        dgvQueue.Columns["Customer"].HeaderText = "Customer";
                        dgvQueue.Columns["DoctorName"].HeaderText = "Doctor";
                        dgvQueue.Columns["ImagePath"].Visible = false;
                        dgvQueue.Columns["UploadedAt"].HeaderText = "Uploaded";
                        dgvQueue.Columns["UploadedAt"].DefaultCellStyle.Format = "dd MMM, HH:mm";
                        dgvQueue.Columns["VerifyStatus"].HeaderText = "Rx status";
                        dgvQueue.Columns["RejectReason"].HeaderText = "Reason";
                        dgvQueue.Columns["OrderStatus"].HeaderText = "Order";
                        dgvQueue.Columns["TotalAmount"].HeaderText = "Total (Tk)";
                        dgvQueue.Columns["IsCurrent"].Visible = false;

                        UiTheme.SizeColumn(dgvQueue, "OrderId", 36, 48);
                        UiTheme.SizeColumn(dgvQueue, "Customer", 80, 80);
                        UiTheme.SizeColumn(dgvQueue, "DoctorName", 90, 80);
                        UiTheme.SizeColumn(dgvQueue, "UploadedAt", 70, 84);
                        UiTheme.SizeColumn(dgvQueue, "VerifyStatus", 50, 62);
                        UiTheme.SizeColumn(dgvQueue, "RejectReason", 70, 54);
                        UiTheme.SizeColumn(dgvQueue, "OrderStatus", 50, 60);
                        UiTheme.SizeColumn(dgvQueue, "TotalAmount", 50, 60);
                    }

                    SelectRow(keepId);

                    // Hiding PrescriptionId above cleared the current cell; put it
                    // back so the first prescription's image and items show on open.
                    UiTheme.EnsureCurrentCell(dgvQueue, "OrderId");
                }
                finally
                {
                    _binding = false;
                }

                UpdateSelection();

                int pending = _prescriptions.CountPending(UserSession.PharmacyId);
                lblStatus.Text = statusMessage ??
                    (pending == 0
                        ? "Nothing is waiting for verification right now."
                        : pending + " prescription(s) are waiting for your decision. Those orders cannot be confirmed until you decide.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("The prescription queue could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectRow(int prescriptionId)
        {
            if (prescriptionId <= 0) return;
            foreach (DataGridViewRow row in dgvQueue.Rows)
            {
                if (Convert.ToInt32(row.Cells["PrescriptionId"].Value) == prescriptionId)
                {
                    dgvQueue.CurrentCell = row.Cells["OrderId"];
                    return;
                }
            }
        }

        /// <summary>
        /// Row colours are applied once per binding rather than in
        /// CellFormatting, which fires for every cell on every repaint and made
        /// the grid flicker.
        /// </summary>
        private void dgvQueue_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvQueue.Columns.Count == 0) return;

            foreach (DataGridViewRow row in dgvQueue.Rows)
            {
                string status = Convert.ToString(row.Cells["VerifyStatus"].Value);
                string orderStatus = Convert.ToString(row.Cells["OrderStatus"].Value);

                Color back;
                switch (status)
                {
                    case "Pending": back = orderStatus == "Placed" ? UiTheme.PendingBack : UiTheme.InactiveBack; break;
                    case "Approved": back = UiTheme.DeliveredBack; break;
                    case "Rejected": back = UiTheme.LowStockBack; break;
                    default: back = Color.Empty; break;
                }
                row.DefaultCellStyle.BackColor = back;
            }
        }

        private void dgvQueue_SelectionChanged(object sender, EventArgs e)
        {
            if (_binding) return;
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            DataGridViewRow row = dgvQueue.CurrentRow;

            ClearImage();

            if (row == null || SelectedPrescriptionId() == 0)
            {
                dgvOrderItems.DataSource = null;
                lblDoctor.Text = "";
                lblImagePath.Text = "";
                btnApprove.Enabled = false;
                btnReject.Enabled = false;
                return;
            }

            int orderId = Convert.ToInt32(row.Cells["OrderId"].Value);
            try
            {
                dgvOrderItems.DataSource = _orders.GetOrderItems(orderId);

                if (dgvOrderItems.Columns.Count > 0)
                {
                    dgvOrderItems.Columns["MedicineName"].HeaderText = "Medicine";
                    dgvOrderItems.Columns["Strength"].HeaderText = "Strength";
                    dgvOrderItems.Columns["Quantity"].HeaderText = "Qty";
                    dgvOrderItems.Columns["UnitPrice"].HeaderText = "Unit price (Tk)";
                    dgvOrderItems.Columns["Subtotal"].HeaderText = "Line total (Tk)";
                }
            }
            catch (Exception ex)
            {
                dgvOrderItems.DataSource = null;
                lblStatus.Text = "The items on order " + orderId + " could not be loaded: " + DbHelper.Describe(ex);
            }

            object doctor = row.Cells["DoctorName"].Value;
            string status = Convert.ToString(row.Cells["VerifyStatus"].Value);
            string reason = Convert.ToString(row.Cells["RejectReason"].Value);

            lblDoctor.Text = "Doctor: " + (doctor == null || doctor == DBNull.Value ? "(not given)" : doctor.ToString()) +
                             (status == "Rejected" && reason.Length > 0 ? "   |   Rejected: " + reason : "");

            string storedPath = Convert.ToString(row.Cells["ImagePath"].Value);
            ShowImage(storedPath);

            bool decidable = IsDecidable(row);
            btnApprove.Enabled = decidable;
            btnReject.Enabled = decidable;
        }

        /// <summary>The same rule the UPDATE enforces: current, Pending, on a Placed order.</summary>
        private static bool IsDecidable(DataGridViewRow row)
        {
            return Convert.ToString(row.Cells["VerifyStatus"].Value) == "Pending" &&
                   Convert.ToString(row.Cells["OrderStatus"].Value) == "Placed" &&
                   Convert.ToInt32(row.Cells["IsCurrent"].Value) == 1;
        }

        private void ClearImage()
        {
            if (picPrescription.Image != null)
            {
                picPrescription.Image.Dispose();
                picPrescription.Image = null;
            }
        }

        private void ShowImage(string storedPath)
        {
            // ResolveImagePath refuses anything outside the upload folder and
            // returns "" for it, exactly as for a file that is simply missing.
            string fullPath = PrescriptionService.ResolveImagePath(storedPath);

            if (string.IsNullOrWhiteSpace(fullPath))
            {
                lblImagePath.Text = "The prescription image is not available on this computer.";
                return;
            }

            lblImagePath.Text = Path.GetFileName(fullPath);

            try
            {
                // Loaded through a stream and copied, so the file is not locked
                // and can still be replaced by a fresh upload.
                using (FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                using (Image original = Image.FromStream(stream))
                {
                    picPrescription.Image = new Bitmap(original);
                }
            }
            catch
            {
                lblImagePath.Text = "The prescription image could not be opened. It may be damaged.";
            }
        }

        // ---------------------------------------------------------------------

        private void SetStatus(string newStatus)
        {
            DataGridViewRow row = dgvQueue.CurrentRow;
            if (row == null || SelectedPrescriptionId() == 0 || !IsDecidable(row)) return;

            int prescriptionId = Convert.ToInt32(row.Cells["PrescriptionId"].Value);
            int orderId = Convert.ToInt32(row.Cells["OrderId"].Value);
            string reason = null;

            if (newStatus == "Rejected")
            {
                reason = AskRejectReason(orderId);
                if (reason == null) return;
            }
            else
            {
                DialogResult answer = MessageBox.Show(
                    "Approve the prescription on order " + orderId + "?\r\n\r\n" +
                    "Once approved, you will be able to confirm the order.",
                    "Approve prescription", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (answer != DialogResult.Yes) return;
            }

            bool changed;
            try
            {
                changed = _prescriptions.SetVerifyStatus(prescriptionId, UserSession.PharmacyId, newStatus, reason);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The decision could not be saved.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadQueue(changed
                ? "The prescription on order " + orderId + " is now " + newStatus + "."
                : "Nothing changed: this prescription has already been decided, or order " + orderId +
                  " is no longer waiting.");
        }

        /// <summary>
        /// A small modal asking for the reason the customer will see. Returns
        /// null when the owner cancels. Built in code because it is only a
        /// text box and two buttons.
        /// </summary>
        private string AskRejectReason(int orderId)
        {
            using (Form dialog = new Form())
            using (Label prompt = new Label())
            using (TextBox txtReason = new TextBox())
            using (Label lblError = new Label())
            using (Button btnOk = new Button())
            using (Button btnCancelReason = new Button())
            {
                UiTheme.StyleForm(dialog, "Reject prescription");
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(460, 180);

                prompt.Text = "Why are you rejecting the prescription on order " + orderId + "? " +
                              "The customer will see this, for example \"photo is blurry\".";
                prompt.Font = UiTheme.FontBody;
                prompt.Location = new Point(16, 14);
                prompt.Size = new Size(428, 40);

                txtReason.MaxLength = 200;
                txtReason.Font = UiTheme.FontBody;
                txtReason.Location = new Point(16, 60);
                txtReason.Size = new Size(428, 27);

                lblError.Font = UiTheme.FontSmall;
                lblError.ForeColor = UiTheme.Danger;
                lblError.Location = new Point(16, 92);
                lblError.Size = new Size(428, 18);

                btnOk.Text = "Reject";
                btnOk.Location = new Point(222, 124);
                btnOk.Size = new Size(106, 38);
                UiTheme.StyleDanger(btnOk);
                btnOk.Click += (s, args) =>
                {
                    if (Validator.IsBlank(txtReason.Text))
                    {
                        lblError.Text = "Please enter a short reason.";
                        txtReason.Focus();
                        return;
                    }
                    dialog.DialogResult = DialogResult.OK;
                };

                btnCancelReason.Text = "Cancel";
                btnCancelReason.Location = new Point(338, 124);
                btnCancelReason.Size = new Size(106, 38);
                btnCancelReason.DialogResult = DialogResult.Cancel;
                UiTheme.StyleSecondary(btnCancelReason);

                dialog.Controls.AddRange(new Control[] { prompt, txtReason, lblError, btnOk, btnCancelReason });
                dialog.AcceptButton = btnOk;
                dialog.CancelButton = btnCancelReason;

                return dialog.ShowDialog(this) == DialogResult.OK ? txtReason.Text.Trim() : null;
            }
        }

        private void btnApprove_Click(object sender, EventArgs e) => SetStatus("Approved");
        private void btnReject_Click(object sender, EventArgs e) => SetStatus("Rejected");
        private void Filter_Changed(object sender, EventArgs e) => LoadQueue();

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
