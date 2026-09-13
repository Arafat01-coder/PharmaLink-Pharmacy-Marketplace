using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 11, the shop half of the profile.
    ///
    /// Every UPDATE on this form carries WHERE PharmacyId = @PharmacyId, so an
    /// owner cannot edit another shop. The licence number is displayed read only
    /// because changing it would mean a new licence and a fresh approval, and
    /// the commission rate, status and rating are shown but belong to the Super
    /// Admin.
    ///
    /// The logo is copied into PharmaLink's own Uploads\Logos folder when the
    /// profile is saved, under a new unique name, and the database stores that
    /// relative path. Storing the original path pointed at a file on the
    /// owner's desktop that the next move, rename or other computer would lose.
    /// </summary>
    public partial class PharmacyProfileForm : Form
    {
        private const long MaxLogoBytes = 2 * 1024 * 1024;
        private static readonly string[] LogoExtensions = { ".jpg", ".jpeg", ".png" };

        private readonly PharmacyService _pharmacies = new PharmacyService();
        private readonly ReviewService _reviews = new ReviewService();
        private bool _loading = true;

        // The stored (relative) logo path, and a newly chosen file not yet copied in.
        private string _logoPath = "";
        private string _pendingLogoFile;

        public PharmacyProfileForm()
        {
            InitializeComponent();
            FormClosed += (s, e) => SetPreview(null);
        }

        private void PharmacyProfileForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            if (!LoadPharmacy()) return;   // the form is already closing
            _loading = false;
            ValidateAll();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "My Pharmacy Profile");
            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            foreach (GroupBox group in new[] { grpShop, grpFacts })
            {
                group.Font = UiTheme.FontHeading;
                group.ForeColor = UiTheme.Primary;
                group.BackColor = UiTheme.CardBack;

                foreach (Control child in group.Controls)
                {
                    child.Font = UiTheme.FontBody;
                    child.ForeColor = UiTheme.TextDark;
                    if (child is Label label && label.Name.EndsWith("Error"))
                    {
                        label.Font = UiTheme.FontSmall;
                        label.ForeColor = UiTheme.Danger;
                    }
                }
            }

            foreach (Label caption in new[] { lblStatusCaption, lblCommissionCaption, lblRatingCaption, lblRegisteredCaption })
            {
                caption.Font = UiTheme.FontCaption;
                caption.ForeColor = UiTheme.TextMuted;
            }

            foreach (Label value in new[] { lblStatusValue, lblCommissionValue, lblRatingValue, lblRegisteredValue })
            {
                value.Font = UiTheme.FontSubheading;
                value.ForeColor = UiTheme.TextDark;
            }

            txtLicense.BackColor = UiTheme.ReadOnlyBack;
            txtLogoPath.BackColor = UiTheme.ReadOnlyBack;
            picLogo.BackColor = UiTheme.ReadOnlyBack;
            lblLicenseNote.Font = UiTheme.FontSmall;
            lblLicenseNote.ForeColor = UiTheme.TextMuted;
            lblLogoHint.Font = UiTheme.FontSmall;
            lblLogoHint.ForeColor = UiTheme.TextMuted;
            lblFactsNote.Font = UiTheme.FontSmall;
            lblFactsNote.ForeColor = UiTheme.TextMuted;
            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;

            UiTheme.StyleSecondary(btnBack);
            UiTheme.StyleSecondary(btnBrowseLogo);
            UiTheme.StyleSecondary(btnClearLogo);
            UiTheme.StylePrimary(btnSave);
            btnSave.Font = UiTheme.FontButtonStrong;
        }

        /// <summary>Returns false when the form had to close instead.</summary>
        private bool LoadPharmacy()
        {
            Pharmacy pharmacy;
            List<string> areas;
            decimal rating;
            int reviewCount;

            try
            {
                pharmacy = _pharmacies.GetById(UserSession.PharmacyId);
                areas = pharmacy == null ? new List<string>() : _pharmacies.GetAreas(false);
                rating = _reviews.GetAverageForPharmacy(UserSession.PharmacyId);
                reviewCount = _reviews.CountForPharmacy(UserSession.PharmacyId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your pharmacy record could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return false;
            }

            if (pharmacy == null)
            {
                MessageBox.Show("Your pharmacy record could not be loaded.", "PharmaLink",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return false;
            }

            txtShopName.Text = pharmacy.PharmacyName;
            txtLicense.Text = pharmacy.LicenseNo;
            txtAddress.Text = pharmacy.Address;
            txtContact.Text = pharmacy.ContactPhone;

            // Existing areas are offered so the same area is not typed three ways
            // ("Dhanmondi", "dhanmondi ", "Dhanmondi R/A"); a new one can still be typed.
            cmbArea.Items.Clear();
            foreach (string area in areas)
            {
                if (!string.IsNullOrWhiteSpace(area)) cmbArea.Items.Add(area.Trim());
            }
            cmbArea.Text = pharmacy.Area;

            _logoPath = pharmacy.LogoPath ?? "";
            _pendingLogoFile = null;
            txtLogoPath.Text = _logoPath.Length == 0 ? "(no logo)" : Path.GetFileName(_logoPath);
            SetPreview(ResolveLogo(_logoPath));

            lblStatusValue.Text = pharmacy.Status;
            lblStatusValue.ForeColor = pharmacy.Status == "Approved" ? UiTheme.Success : UiTheme.Danger;

            lblCommissionValue.Text = pharmacy.CommissionRate.ToString("N2") + " %";
            lblRegisteredValue.Text = pharmacy.RegisteredAt.ToString("dd MMM yyyy");

            lblRatingValue.Text = reviewCount == 0 ? "no reviews yet" : rating.ToString("N2") + " / 5";
            lblRatingValue.ForeColor = reviewCount > 0 && rating < 2.5m ? UiTheme.Danger : UiTheme.TextDark;

            lblStatus.Text = "Your shop name, area and address appear on the customer catalogue and are printed on every invoice.";
            return true;
        }

        // ---------------------------------------------------------------------
        //  VALIDATION
        // ---------------------------------------------------------------------

        private void Field_Changed(object sender, EventArgs e)
        {
            if (_loading) return;
            ValidateAll();
        }

        private bool ValidateAll()
        {
            bool ok = true;

            ok &= Check(!Validator.IsBlank(txtShopName.Text), lblShopNameError, txtShopName,
                        "The shop name cannot be empty - it is what customers search for.");

            string area = cmbArea.Text.Trim();
            if (area.Length == 0)
                ok &= Check(false, lblAreaError, cmbArea, "The area is what customers filter by, so it is required.");
            else
                ok &= Check(area.Length <= 60, lblAreaError, cmbArea, "The area can be at most 60 characters.");

            ok &= Check(!Validator.IsBlank(txtAddress.Text), lblAddressError, txtAddress,
                        "The address is printed on every invoice, so it cannot be empty.");

            if (Validator.IsBlank(txtContact.Text))
                ok &= Check(false, lblContactError, txtContact, "Enter the shop's contact number.");
            else
                ok &= Check(Validator.IsContactPhone(txtContact.Text.Trim()), lblContactError, txtContact,
                            "Use 11 digits starting 01 or 02.");

            // StylePrimary greys the button itself while it is disabled.
            btnSave.Enabled = ok;
            return ok;
        }

        private bool Check(bool rulePassed, Label errorLabel, Control field, string message)
        {
            if (rulePassed) UiTheme.ClearError(errorLabel, field);
            else UiTheme.ShowError(errorLabel, field, message);
            return rulePassed;
        }

        // ---------------------------------------------------------------------
        //  LOGO
        // ---------------------------------------------------------------------

        /// <summary>Stored paths are relative to the application folder; old rows may hold a full path.</summary>
        private static string ResolveLogo(string storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath)) return null;
            return Path.IsPathRooted(storedPath) ? storedPath : Path.Combine(AppContext.BaseDirectory, storedPath);
        }

        /// <summary>
        /// Shows an image file in the preview, or clears it. The file is read
        /// into memory and copied, so the preview never keeps the file locked,
        /// and the previous image is disposed rather than left for the GC.
        /// </summary>
        private void SetPreview(string fullPath)
        {
            Image old = picLogo.Image;
            picLogo.Image = null;
            if (old != null) old.Dispose();

            if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath)) return;

            try
            {
                using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(fullPath)))
                using (Image image = Image.FromStream(stream))
                {
                    picLogo.Image = new Bitmap(image);
                }
            }
            catch (Exception)
            {
                // An unreadable stored logo just shows no preview.
            }
        }

        private void btnBrowseLogo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Logo images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                string file = dialog.FileName;
                string extension = Path.GetExtension(file).ToLowerInvariant();

                if (Array.IndexOf(LogoExtensions, extension) < 0)
                {
                    MessageBox.Show("The logo must be a JPG or PNG image.", "Choose another file",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (new FileInfo(file).Length >= MaxLogoBytes)
                {
                    MessageBox.Show("The logo must be smaller than 2 MB. Save a smaller copy and choose that.",
                        "Choose another file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(file)))
                    using (Image.FromStream(stream)) { }
                }
                catch (Exception)
                {
                    MessageBox.Show("That file could not be opened as an image.", "Choose another file",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _pendingLogoFile = file;
                txtLogoPath.Text = Path.GetFileName(file) + "  (saved when you press Save)";
                SetPreview(file);
            }
        }

        private void btnClearLogo_Click(object sender, EventArgs e)
        {
            _pendingLogoFile = null;
            _logoPath = "";
            txtLogoPath.Text = "(no logo - saved when you press Save)";
            SetPreview(null);
        }

        /// <summary>
        /// Copies the chosen file into Uploads\Logos under the application
        /// folder with a unique name and returns the relative path to store.
        /// </summary>
        private static string CopyLogoIntoApp(string sourceFile)
        {
            string relativeFolder = Path.Combine("Uploads", "Logos");
            string folder = Path.Combine(AppContext.BaseDirectory, relativeFolder);
            Directory.CreateDirectory(folder);

            string fileName = "pharmacy-" + UserSession.PharmacyId + "-" + Guid.NewGuid().ToString("N") +
                              Path.GetExtension(sourceFile).ToLowerInvariant();
            File.Copy(sourceFile, Path.Combine(folder, fileName));
            return Path.Combine(relativeFolder, fileName);
        }

        // ---------------------------------------------------------------------

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateAll()) return;

            string copiedLogo = null;
            try
            {
                string logoToStore = _logoPath;
                if (_pendingLogoFile != null)
                {
                    copiedLogo = CopyLogoIntoApp(_pendingLogoFile);
                    logoToStore = copiedLogo;
                }

                if (_pharmacies.UpdateProfile(UserSession.PharmacyId, txtShopName.Text, cmbArea.Text.Trim(),
                                              txtAddress.Text, txtContact.Text.Trim(), logoToStore))
                {
                    UserSession.PharmacyName = txtShopName.Text.Trim();
                    _logoPath = logoToStore ?? "";
                    _pendingLogoFile = null;
                    txtLogoPath.Text = _logoPath.Length == 0 ? "(no logo)" : Path.GetFileName(_logoPath);
                    lblStatus.Text = "Shop profile saved. Customers see the new details immediately.";
                    MessageBox.Show("Your pharmacy profile has been updated.", "PharmaLink",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DeleteQuietly(copiedLogo);
                    lblStatus.Text = "Nothing was saved.";
                    MessageBox.Show("Nothing was saved because your pharmacy record could not be found.",
                        "Not saved", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                DeleteQuietly(copiedLogo);
                MessageBox.Show("The profile could not be saved.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Removes a logo copy that ended up not being used by any saved profile.</summary>
        private static void DeleteQuietly(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            try
            {
                File.Delete(Path.Combine(AppContext.BaseDirectory, relativePath));
            }
            catch (Exception)
            {
                // an orphaned copy is harmless
            }
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
