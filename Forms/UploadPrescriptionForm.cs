using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Helpers;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 26. The modal that collects a prescription photograph: at the
    /// Confirm Order step when the basket contains a medicine whose RequiresRx
    /// flag is set, and again from My Orders when a prescription was rejected or
    /// never attached.
    ///
    /// It accepts only JPG and PNG files under 2 MB, and it cannot be dismissed
    /// with Attach until a valid image has been chosen. The form only collects
    /// the file; the row is written by the caller - inside the checkout
    /// transaction, or by PrescriptionService.Upload for a re-upload.
    /// </summary>
    public partial class UploadPrescriptionForm : Form
    {
        private const long MaxBytes = 2 * 1024 * 1024;

        private readonly string _pharmacyName;

        /// <summary>The chosen file, read by the caller after the dialog closes with OK.</summary>
        public string SelectedImagePath { get; private set; } = "";

        public string DoctorName { get; private set; } = "";

        public UploadPrescriptionForm(string pharmacyName)
        {
            InitializeComponent();
            _pharmacyName = pharmacyName;
        }

        private void UploadPrescriptionForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();

            // The preview bitmap is a private copy; release it however the form closes.
            FormClosed += (s, args) => ClearPreview();

            lblSubtitle.Text = _pharmacyName + " will check this before confirming your order.";
            UpdateAttachButton();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Upload Prescription");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);
            panelHeader.BackColor = UiTheme.Warning;
            lblSubtitle.ForeColor = UiTheme.WarningBack;

            lblFileError.Font = UiTheme.FontSmall;
            lblFileError.ForeColor = UiTheme.Danger;
            lblFileInfo.Font = UiTheme.FontSmall;
            lblFileInfo.ForeColor = UiTheme.TextMuted;
            lblRules.Font = UiTheme.FontSmall;
            lblRules.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleAccent(btnChooseFile);
            UiTheme.StyleSuccess(btnAttach);
            btnAttach.Font = UiTheme.FontButtonStrong;
            UiTheme.StyleSecondary(btnCancel);
        }

        // ---------------------------------------------------------------------

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Choose a photograph of your prescription";
                dialog.Filter = "Prescription image (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                TryAcceptFile(dialog.FileName);
            }
        }

        private void TryAcceptFile(string path)
        {
            ClearPreview();
            SelectedImagePath = "";

            FileInfo file = new FileInfo(path);

            if (!file.Exists)
            {
                UiTheme.ShowError(lblFileError, null, "That file no longer exists.");
                UpdateAttachButton();
                return;
            }

            string extension = file.Extension.ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                UiTheme.ShowError(lblFileError, null,
                    "Only JPG and PNG images are accepted. '" + extension + "' is not one of them.");
                UpdateAttachButton();
                return;
            }

            if (file.Length > MaxBytes)
            {
                UiTheme.ShowError(lblFileError, null,
                    "The image is " + (file.Length / 1024 / 1024.0).ToString("N1") +
                    " MB. Please use a photograph under 2 MB.");
                UpdateAttachButton();
                return;
            }

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (Image original = Image.FromStream(stream))
                {
                    picPreview.Image = new Bitmap(original);
                }
            }
            catch
            {
                UiTheme.ShowError(lblFileError, null, "That file could not be read as an image.");
                UpdateAttachButton();
                return;
            }

            SelectedImagePath = path;
            UiTheme.ClearError(lblFileError, null);
            lblFileInfo.Text = file.Name + "   -   " + (file.Length / 1024.0).ToString("N0") + " KB";
            UpdateAttachButton();
        }

        private void ClearPreview()
        {
            if (picPreview.Image != null)
            {
                picPreview.Image.Dispose();
                picPreview.Image = null;
            }
            lblFileInfo.Text = "";
        }

        /// <summary>A disabled styled button greys itself, so only Enabled is set.</summary>
        private void UpdateAttachButton()
        {
            btnAttach.Enabled = !string.IsNullOrEmpty(SelectedImagePath);
        }

        // ---------------------------------------------------------------------

        private void btnAttach_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedImagePath)) return;

            DoctorName = txtDoctor.Text.Trim();
            ClearPreview();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show(
                "Without a prescription the pharmacy cannot confirm this order.\r\n\r\n" +
                "Close without attaching one?",
                "No prescription attached", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (answer != DialogResult.OK) return;

            SelectedImagePath = "";
            ClearPreview();
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
