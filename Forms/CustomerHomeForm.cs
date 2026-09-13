using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// The customer's home screen (requirements 20, 21 and 22).
    ///
    /// One search box over three columns - brand name, generic name and
    /// manufacturer - so a patient holding a doctor's chit that says
    /// "paracetamol" finds Napa and Ace Plus even though neither brand contains
    /// that word. Five ComboBox filters narrow the result: category, price
    /// range, area, pharmacy and availability. They are combined in a single
    /// query, and only medicines belonging to an Approved pharmacy are ever
    /// returned.
    ///
    /// Typing in the search box waits 300 ms after the last key before querying,
    /// so a word typed quickly runs one search instead of one per letter.
    /// Before any child screen opens, the session is re-checked, so a customer
    /// suspended mid-session is signed out at the next click.
    /// </summary>
    public partial class CustomerHomeForm : Form
    {
        /// <summary>"In stock" is the load default and what Clear filters returns to.</summary>
        private const int DefaultAvailabilityIndex = 1;

        private readonly MedicineService _medicines = new MedicineService();
        private readonly CategoryService _categories = new CategoryService();
        private readonly PharmacyService _pharmacies = new PharmacyService();
        private readonly CartService _cart = new CartService();
        private readonly AuthService _auth = new AuthService();

        private readonly System.Windows.Forms.Timer _searchTimer = new System.Windows.Forms.Timer { Interval = 300 };

        private bool _loading = true;

        /// <summary>True while the grid is being rebound, so SelectionChanged is ignored.</summary>
        private bool _binding;

        public CustomerHomeForm()
        {
            InitializeComponent();
        }

        private void CustomerHomeForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            UiTheme.MakeResizable(this, Size);

            _searchTimer.Tick += SearchTimer_Tick;
            FormClosed += (s, args) => _searchTimer.Dispose();

            try
            {
                LoadFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The filters could not be loaded.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _loading = false;
            LoadGrid();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Browse Medicines");

            panelSide.BackColor = UiTheme.Sidebar;
            lblBrand.Font = UiTheme.FontBrand;
            lblBrand.ForeColor = Color.White;
            lblRole.Font = UiTheme.FontCaption;
            lblRole.ForeColor = UiTheme.SidebarRole;
            lblUserName.Font = UiTheme.FontSmall;
            lblUserName.ForeColor = UiTheme.SidebarUser;
            lblUserName.Text = UserSession.FullName;

            foreach (Button button in new[] { btnBrowse, btnCart, btnOffers, btnOrders, btnMyAccount })
                UiTheme.StyleSidebarButton(button);

            btnBrowse.BackColor = UiTheme.SidebarHover;   // the screen we are on

            UiTheme.StyleSidebarButton(btnLogout);
            btnLogout.BackColor = UiTheme.Danger;
            btnLogout.FlatAppearance.MouseOverBackColor = UiTheme.DangerHover;

            UiTheme.StyleHeader(panelHeader, lblHeaderTitle, lblHeaderSub);
            lblCartSummary.Font = UiTheme.FontButtonStrong;
            lblCartSummary.ForeColor = Color.White;

            UiTheme.StyleSecondary(btnSearch);
            UiTheme.StyleSecondary(btnClearFilters);
            UiTheme.StyleAccent(btnDetails);
            UiTheme.StylePrimary(btnAddToCart);
            UiTheme.StyleSecondary(btnOpenCart);
            btnAddToCart.Font = UiTheme.FontButtonStrong;

            UiTheme.StyleGrid(dgvMedicines);
            UiTheme.EnableEmptyMessage(dgvMedicines, "No medicines match your search. Try Clear filters.");
            dgvMedicines.DataBindingComplete += dgvMedicines_DataBindingComplete;

            lblStatus.Font = UiTheme.FontSmall;
            lblStatus.ForeColor = UiTheme.TextMuted;
        }

        private void LoadFilters()
        {
            cmbCategory.Items.Add("All categories");
            foreach (Category category in _categories.GetActiveList())
                cmbCategory.Items.Add(category.CategoryId + " - " + category.CategoryName);

            // The bands meet but never overlap: each minimum is inclusive and each
            // maximum exclusive, applied to the price after today's discount.
            cmbPriceRange.Items.AddRange(new object[]
            {
                "Any price",
                "Under Tk 10",
                "Tk 10 to under 50",
                "Tk 50 to under 200",
                "Tk 200 to under 1000",
                "Tk 1000 and over"
            });

            cmbArea.Items.Add("All areas");
            foreach (string area in _pharmacies.GetAreas(true)) cmbArea.Items.Add(area);

            cmbPharmacy.Items.Add("All pharmacies");
            foreach (Pharmacy pharmacy in _pharmacies.GetApprovedList())
                cmbPharmacy.Items.Add(pharmacy.PharmacyId + " - " + pharmacy.PharmacyName);

            cmbAvailability.Items.AddRange(new object[] { "Any", "In stock" });

            ResetFilters();
        }

        /// <summary>The one definition of "no filters": used on load and by Clear filters.</summary>
        private void ResetFilters()
        {
            txtSearch.Clear();
            foreach (ComboBox combo in new[] { cmbCategory, cmbPriceRange, cmbArea, cmbPharmacy })
                if (combo.Items.Count > 0) combo.SelectedIndex = 0;
            if (cmbAvailability.Items.Count > DefaultAvailabilityIndex)
                cmbAvailability.SelectedIndex = DefaultAvailabilityIndex;   // in stock only is the sensible default
        }

        // ---------------------------------------------------------------------
        //  READING THE FILTERS
        // ---------------------------------------------------------------------

        private int SelectedCategoryId()
        {
            if (cmbCategory.SelectedIndex <= 0) return 0;
            string text = cmbCategory.SelectedItem.ToString();
            return int.Parse(text.Substring(0, text.IndexOf(' ')));
        }

        private int SelectedPharmacyId()
        {
            if (cmbPharmacy.SelectedIndex <= 0) return 0;
            string text = cmbPharmacy.SelectedItem.ToString();
            return int.Parse(text.Substring(0, text.IndexOf(' ')));
        }

        /// <summary>
        /// Minimum inclusive, maximum exclusive, on the discounted price.
        /// A maximum of 0 means "no upper limit" to SearchForCustomer, which is
        /// how the open-ended top band is expressed; "Any price" is 0 and 0.
        /// </summary>
        private void SelectedPriceRange(out decimal minPrice, out decimal maxPrice)
        {
            switch (cmbPriceRange.SelectedIndex)
            {
                case 1: minPrice = 0m; maxPrice = 10m; break;
                case 2: minPrice = 10m; maxPrice = 50m; break;
                case 3: minPrice = 50m; maxPrice = 200m; break;
                case 4: minPrice = 200m; maxPrice = 1000m; break;
                case 5: minPrice = 1000m; maxPrice = 0m; break;
                default: minPrice = 0m; maxPrice = 0m; break;
            }
        }

        /// <summary>Counts only filters that differ from the defaults, so a fresh screen says 0.</summary>
        private int ActiveFilterCount()
        {
            int count = 0;
            if (!string.IsNullOrWhiteSpace(txtSearch.Text)) count++;
            if (cmbCategory.SelectedIndex > 0) count++;
            if (cmbPriceRange.SelectedIndex > 0) count++;
            if (cmbArea.SelectedIndex > 0) count++;
            if (cmbPharmacy.SelectedIndex > 0) count++;
            if (cmbAvailability.SelectedIndex != DefaultAvailabilityIndex) count++;
            return count;
        }

        // ---------------------------------------------------------------------

        /// <summary>
        /// Runs the search and rebinds the grid. The selected medicine (or
        /// <paramref name="selectMedicineId"/>) stays selected, and
        /// <paramref name="statusMessage"/> - for example "added to your cart" -
        /// is written after the reload so the reload cannot wipe it.
        /// </summary>
        private void LoadGrid(string statusMessage = null, int selectMedicineId = 0)
        {
            if (_loading) return;
            _searchTimer.Stop();

            try
            {
                if (selectMedicineId == 0) selectMedicineId = SelectedMedicineId();

                decimal minPrice, maxPrice;
                SelectedPriceRange(out minPrice, out maxPrice);

                string area = cmbArea.SelectedIndex <= 0 ? "" : cmbArea.SelectedItem.ToString();

                DataTable table = _medicines.SearchForCustomer(
                    txtSearch.Text.Trim(),
                    SelectedCategoryId(),
                    minPrice, maxPrice,
                    area,
                    SelectedPharmacyId(),
                    cmbAvailability.SelectedIndex == 1);

                _binding = true;
                try
                {
                    dgvMedicines.DataSource = table;
                    LabelColumns();
                    SelectMedicine(selectMedicineId);
                }
                finally
                {
                    _binding = false;
                }

                int cartLines = _cart.CountLines(UserSession.UserId);
                lblCartSummary.Text = cartLines == 0 ? "Cart is empty" : "Cart:  " + cartLines + " item(s)";

                string counts = table.Rows.Count + " medicine(s) found   |   " + ActiveFilterCount() + " filter(s) active";
                const string help = "Rx = a prescription is needed at checkout.   Off % = today's discount, already included in 'You pay'.";

                lblStatus.ForeColor = statusMessage != null ? UiTheme.Success : UiTheme.TextMuted;
                lblStatus.Text = statusMessage != null
                    ? statusMessage + Environment.NewLine + counts
                    : counts + Environment.NewLine + help;

                UpdateButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load the catalogue.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Short codes (ID, Rx, Off %, Stock) get narrow columns and the names a
        /// customer reads (Brand, Generic, Sold by) get the width, each with a
        /// floor so nothing is squeezed to "Mit...".
        /// </summary>
        private void LabelColumns()
        {
            if (dgvMedicines.Columns.Count == 0) return;

            dgvMedicines.Columns["MedicineId"].HeaderText = "ID";
            dgvMedicines.Columns["MedicineName"].HeaderText = "Brand";
            dgvMedicines.Columns["GenericName"].HeaderText = "Generic";
            dgvMedicines.Columns["Strength"].HeaderText = "Strength";
            dgvMedicines.Columns["Manufacturer"].HeaderText = "Made by";
            dgvMedicines.Columns["CategoryName"].HeaderText = "Category";
            dgvMedicines.Columns["PharmacyName"].HeaderText = "Sold by";
            dgvMedicines.Columns["Area"].HeaderText = "Area";
            dgvMedicines.Columns["UnitPrice"].HeaderText = "List (Tk)";
            dgvMedicines.Columns["DiscountPercent"].HeaderText = "Off %";
            dgvMedicines.Columns["DiscountPercent"].DefaultCellStyle.Format = "0.##";
            dgvMedicines.Columns["PriceYouPay"].HeaderText = "You pay (Tk)";
            dgvMedicines.Columns["Stock"].HeaderText = "Stock";
            dgvMedicines.Columns["RequiresRx"].HeaderText = "Rx";
            dgvMedicines.Columns["ExpiryDate"].HeaderText = "Expires";
            dgvMedicines.Columns["ExpiryDate"].DefaultCellStyle.Format = "MMM yy";

            // The id is only needed to open details and add to the cart, which
            // read it from the row, and the Area is already filtered on and
            // implied by "Sold by"; hiding both gives the names room to breathe.
            dgvMedicines.Columns["MedicineId"].Visible = false;
            dgvMedicines.Columns["Area"].Visible = false;

            UiTheme.SizeColumn(dgvMedicines, "MedicineName", 100, 90);
            UiTheme.SizeColumn(dgvMedicines, "GenericName", 110, 100);
            UiTheme.SizeColumn(dgvMedicines, "Strength", 45, 64);
            UiTheme.SizeColumn(dgvMedicines, "Manufacturer", 80, 90);
            UiTheme.SizeColumn(dgvMedicines, "CategoryName", 70, 80);
            UiTheme.SizeColumn(dgvMedicines, "PharmacyName", 100, 110);
            UiTheme.SizeColumn(dgvMedicines, "UnitPrice", 40, 60);
            UiTheme.SizeColumn(dgvMedicines, "DiscountPercent", 28, 48);
            UiTheme.SizeColumn(dgvMedicines, "PriceYouPay", 48, 76);
            UiTheme.SizeColumn(dgvMedicines, "Stock", 30, 50);
            UiTheme.SizeColumn(dgvMedicines, "RequiresRx", 20, 34);
            UiTheme.SizeColumn(dgvMedicines, "ExpiryDate", 50, 64);
        }

        private void SelectMedicine(int medicineId)
        {
            if (medicineId <= 0) return;
            foreach (DataGridViewRow row in dgvMedicines.Rows)
            {
                if (Convert.ToInt32(row.Cells["MedicineId"].Value) == medicineId)
                {
                    dgvMedicines.CurrentCell = row.Cells["MedicineName"];
                    return;
                }
            }
        }

        /// <summary>
        /// Discounted rows are tinted green and prescription only rows amber.
        /// Applied once per binding: doing it in CellFormatting reassigned the
        /// row style on every cell paint and caused constant repaints.
        /// </summary>
        private void dgvMedicines_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvMedicines.Columns.Count == 0) return;

            foreach (DataGridViewRow row in dgvMedicines.Rows)
            {
                object discount = row.Cells["DiscountPercent"].Value;
                object rx = row.Cells["RequiresRx"].Value;

                bool discounted = discount != null && discount != DBNull.Value && Convert.ToDecimal(discount) > 0m;
                bool needsRx = rx != null && rx != DBNull.Value && Convert.ToBoolean(rx);

                if (discounted) row.DefaultCellStyle.BackColor = UiTheme.DeliveredBack;
                else if (needsRx) row.DefaultCellStyle.BackColor = UiTheme.PendingBack;
                else row.DefaultCellStyle.BackColor = Color.Empty;
            }
        }

        private void dgvMedicines_SelectionChanged(object sender, EventArgs e)
        {
            if (_binding) return;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            int medicineId = SelectedMedicineId();
            bool hasRow = medicineId > 0;

            object stock = hasRow ? dgvMedicines.CurrentRow.Cells["Stock"].Value : null;
            bool inStock = hasRow && stock != null && stock != DBNull.Value && Convert.ToInt32(stock) > 0;

            btnDetails.Enabled = hasRow;
            btnAddToCart.Enabled = hasRow && inStock;
            btnAddToCart.Text = hasRow && !inStock ? "Out of stock" : "Add to cart";
        }

        private int SelectedMedicineId()
        {
            if (dgvMedicines.CurrentRow == null || dgvMedicines.Columns.Count == 0) return 0;
            object value = dgvMedicines.CurrentRow.Cells["MedicineId"].Value;
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        // ---------------------------------------------------------------------
        //  ACTIONS
        // ---------------------------------------------------------------------

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            int medicineId = SelectedMedicineId();
            if (medicineId == 0) return;

            int quantity;
            if (!Validator.IsPositiveInt(txtQuantity.Text, out quantity))
            {
                MessageBox.Show("Enter a whole quantity of one or more.", "Check the quantity",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQuantity.Focus();
                txtQuantity.SelectAll();
                return;
            }

            string name = Convert.ToString(dgvMedicines.CurrentRow.Cells["MedicineName"].Value);

            string message;
            bool added;
            try
            {
                added = _cart.AddOrIncrease(UserSession.UserId, medicineId, quantity, out message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("That could not be added to your cart.\r\n\r\n" + DbHelper.Describe(ex),
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (added)
            {
                txtQuantity.Text = "1";
                LoadGrid(quantity + " x " + name + " added to your cart. Adding it again increases the quantity.",
                         medicineId);
            }
            else
            {
                MessageBox.Show(message, "Cannot add to cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            int medicineId = SelectedMedicineId();
            if (medicineId == 0) return;
            if (!SessionStillValid()) return;

            using (MedicineDetailsForm details = new MedicineDetailsForm(medicineId))
            {
                details.ShowDialog(this);
            }
            LoadGrid(null, medicineId);
        }

        private void dgvMedicines_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnDetails_Click(sender, EventArgs.Empty);
        }

        // ---------------------------------------------------------------------
        //  NAVIGATION
        // ---------------------------------------------------------------------

        /// <summary>
        /// Re-reads the account status. When the account has been suspended the
        /// customer is told why and signed out, exactly as Log out does.
        /// </summary>
        private bool SessionStillValid()
        {
            try
            {
                string message;
                if (_auth.CheckSessionStillValid(out message)) return true;

                MessageBox.Show(message, "Signed out", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UserSession.Clear();
                Close();
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(DbHelper.Describe(ex), "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>The child is only created once the session has been re-checked.</summary>
        private void OpenChild(Func<Form> createChild)
        {
            if (!SessionStillValid()) return;

            using (Form child = createChild())
            {
                child.ShowDialog(this);
            }

            // My Account may have changed the name shown in the sidebar.
            lblUserName.Text = UserSession.FullName;
            LoadGrid();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (!SessionStillValid()) return;
            LoadGrid();
        }

        private void btnCart_Click(object sender, EventArgs e) => OpenChild(() => new CartForm());
        private void btnOffers_Click(object sender, EventArgs e) => OpenChild(() => new CustomerOffersForm());
        private void btnOrders_Click(object sender, EventArgs e) => OpenChild(() => new OrderHistoryForm());
        private void btnMyAccount_Click(object sender, EventArgs e) => OpenChild(() => new MyProfileForm());
        private void btnSearch_Click(object sender, EventArgs e) => LoadGrid();
        private void Filter_Changed(object sender, EventArgs e) => LoadGrid();

        /// <summary>Restarts the 300 ms wait on every key; the search runs when typing pauses.</summary>
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            LoadGrid();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            _loading = true;
            ResetFilters();
            _loading = false;
            LoadGrid();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show("Log out of PharmaLink?", "Log out",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer == DialogResult.Yes)
            {
                UserSession.Clear();
                Close();
            }
        }
    }
}
