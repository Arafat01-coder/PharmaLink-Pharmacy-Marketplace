using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// Requirement 28. The modal opened by Rate and Review on the order history.
    ///
    /// Only the medicines on this delivered order that have not been reviewed
    /// yet appear in the dropdown, a rating must be chosen before Submit is
    /// enabled, and the comment is capped at 500 characters to match the
    /// NVARCHAR(500) column. An empty comment is stored as NULL, not as "".
    ///
    /// After a review is posted the form stays open while other medicines on
    /// the order are still waiting for one, and closes with OK once anything
    /// was posted, however it is closed.
    /// </summary>
    public partial class GiveRatingForm : Form
    {
        private const string FilledStar = "★";
        private const string EmptyStar = "☆";

        private readonly ReviewService _reviews = new ReviewService();
        private readonly int _orderId;
        private int _rating;

        /// <summary>True while the form fills its own controls, so that is not mistaken for the user's input.</summary>
        private bool _loading = true;

        /// <summary>Validation messages wait until the customer has actually done something.</summary>
        private bool _interacted;

        private int _posted;

        public GiveRatingForm(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
        }

        private void GiveRatingForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            FormClosing += (s, args) => { if (_posted > 0) DialogResult = DialogResult.OK; };

            try
            {
                LoadReviewableItems();
                UpdateCharCount();
                PaintStars();
                _loading = false;
                ValidateAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The medicines on this order could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Rate and Review");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);
            lblSubtitle.Text = "Order #" + _orderId + "  -  only medicines you actually received can be rated.";

            lblRatingCaption.Font = UiTheme.FontHeading;
            lblRatingCaption.ForeColor = UiTheme.TextDark;
            lblRatingWord.Font = UiTheme.FontHeading;
            lblRatingWord.ForeColor = UiTheme.TextMuted;

            foreach (Label label in new[] { lblMedicineError, lblRatingError })
            {
                label.Font = UiTheme.FontSmall;
                label.ForeColor = UiTheme.Danger;
            }

            lblCharCount.Font = UiTheme.FontSmall;
            lblCharCount.ForeColor = UiTheme.TextMuted;
            lblRuleNote.Font = UiTheme.FontSmall;
            lblRuleNote.ForeColor = UiTheme.TextMuted;

            foreach (Button star in StarButtons())
            {
                UiTheme.StyleSecondary(star);
                star.Font = UiTheme.FontStars;
            }

            UiTheme.StyleSuccess(btnSubmit);
            btnSubmit.Font = UiTheme.FontButtonStrong;
            UiTheme.StyleSecondary(btnCancel);
        }

        private Button[] StarButtons()
        {
            return new[] { btnStar1, btnStar2, btnStar3, btnStar4, btnStar5 };
        }

        /// <summary>Fills the dropdown and returns how many medicines are still waiting for a review.</summary>
        private int LoadReviewableItems()
        {
            DataTable table = _reviews.GetReviewableItems(_orderId, UserSession.UserId);

            cmbMedicine.Items.Clear();
            cmbMedicine.Items.Add("- choose a medicine from this order -");

            foreach (DataRow row in table.Rows)
            {
                cmbMedicine.Items.Add(row["MedicineId"] + " - " + row["MedicineName"] + " " + row["Strength"]);
            }

            cmbMedicine.SelectedIndex = 0;

            if (table.Rows.Count == 0)
            {
                // Not an error the customer made, so it is shown as information.
                lblMedicineError.Text = "Everything on this order has already been reviewed, or the order has not been delivered yet.";
                lblMedicineError.ForeColor = UiTheme.TextMuted;
                lblMedicineError.Visible = true;
                cmbMedicine.Enabled = false;
            }
            else
            {
                lblMedicineError.ForeColor = UiTheme.Danger;
                cmbMedicine.Enabled = true;
                if (table.Rows.Count == 1) cmbMedicine.SelectedIndex = 1;   // only one thing to review, so pick it
            }

            return table.Rows.Count;
        }

        private int SelectedMedicineId()
        {
            if (cmbMedicine.SelectedIndex <= 0) return 0;
            string text = cmbMedicine.SelectedItem.ToString();
            return int.Parse(text.Substring(0, text.IndexOf(' ')));
        }

        // ---------------------------------------------------------------------
        //  STAR SELECTOR
        // ---------------------------------------------------------------------

        private void Star_Click(object sender, EventArgs e)
        {
            Button clicked = (Button)sender;
            _rating = int.Parse(clicked.Tag.ToString());
            _interacted = true;
            PaintStars();
            ValidateAll();
        }

        /// <summary>Filled and empty star glyphs, coloured by how good the rating is. Also run on load.</summary>
        private void PaintStars()
        {
            foreach (Button star in StarButtons())
            {
                int value = int.Parse(star.Tag.ToString());

                if (value <= _rating)
                {
                    star.Text = FilledStar;
                    star.BackColor = value <= 2 ? UiTheme.Danger
                                   : value == 3 ? UiTheme.Warning
                                                : UiTheme.Success;
                    star.ForeColor = Color.White;
                }
                else
                {
                    star.Text = EmptyStar;
                    star.BackColor = Color.White;
                    star.ForeColor = UiTheme.TextMuted;
                }
            }

            switch (_rating)
            {
                case 1: lblRatingWord.Text = "1 star  -  very poor"; lblRatingWord.ForeColor = UiTheme.Danger; break;
                case 2: lblRatingWord.Text = "2 stars  -  poor"; lblRatingWord.ForeColor = UiTheme.Danger; break;
                case 3: lblRatingWord.Text = "3 stars  -  acceptable"; lblRatingWord.ForeColor = UiTheme.Warning; break;
                case 4: lblRatingWord.Text = "4 stars  -  good"; lblRatingWord.ForeColor = UiTheme.Success; break;
                case 5: lblRatingWord.Text = "5 stars  -  excellent"; lblRatingWord.ForeColor = UiTheme.Success; break;
                default: lblRatingWord.Text = "Choose a rating"; lblRatingWord.ForeColor = UiTheme.TextMuted; break;
            }
        }

        // ---------------------------------------------------------------------

        private void txtComment_TextChanged(object sender, EventArgs e)
        {
            if (!_loading) _interacted = true;
            UpdateCharCount();
        }

        private void UpdateCharCount()
        {
            int remaining = 500 - txtComment.Text.Length;
            lblCharCount.Text = remaining + " character(s) left";
            lblCharCount.ForeColor = remaining < 40 ? UiTheme.Warning : UiTheme.TextMuted;
        }

        private void Field_Changed(object sender, EventArgs e)
        {
            if (_loading) return;
            _interacted = true;
            ValidateAll();
        }

        private bool ValidateAll()
        {
            bool medicineChosen = SelectedMedicineId() > 0;
            bool ratingChosen = _rating >= 1 && _rating <= 5;

            if (medicineChosen) UiTheme.ClearError(lblMedicineError, cmbMedicine);

            if (!ratingChosen && medicineChosen && _interacted)
                UiTheme.ShowError(lblRatingError, null, "Choose a rating from 1 to 5 before submitting.");
            else
                UiTheme.ClearError(lblRatingError, null);

            bool ok = medicineChosen && ratingChosen;
            btnSubmit.Enabled = ok;
            return ok;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!ValidateAll()) return;

            string comment = string.IsNullOrWhiteSpace(txtComment.Text) ? null : txtComment.Text.Trim();

            string message;
            bool posted;
            try
            {
                posted = _reviews.AddReview(UserSession.UserId, SelectedMedicineId(), _orderId,
                                            _rating, comment, out message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The review could not be saved.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int remaining;
            _loading = true;
            try
            {
                if (posted)
                {
                    _posted++;
                    _rating = 0;
                    txtComment.Clear();
                }
                remaining = LoadReviewableItems();
            }
            catch (Exception ex)
            {
                remaining = 0;
                if (posted)
                    MessageBox.Show(message, "Review posted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MessageBox.Show("The remaining medicines could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }
            finally
            {
                _loading = false;
            }

            if (!posted)
            {
                MessageBox.Show(message, "Review not posted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ValidateAll();
                return;
            }

            if (remaining > 0)
            {
                _interacted = false;
                PaintStars();
                UpdateCharCount();
                ValidateAll();
                MessageBox.Show(message + "\r\n\r\nYou can now review the other medicine(s) on this order, or close this window.",
                    "Review posted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show(message, "Review posted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;   // becomes OK in FormClosing if a review was posted
            Close();
        }
    }
}
