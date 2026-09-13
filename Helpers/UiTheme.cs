using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace PharmaLinkApp.Helpers
{
    /// <summary>
    /// One palette and one set of control styles for the whole application.
    ///
    /// Every form calls into this class instead of picking its own colours, so
    /// the twenty-eight screens look like one product rather than twenty-eight
    /// separate student exercises. Changing the brand colour here changes it
    /// everywhere. Forms should never write Color.FromArgb or new Font(...)
    /// themselves; if a screen needs a shade or a size that is missing, add it
    /// here with a name that says what it is for.
    /// </summary>
    public static class UiTheme
    {
        // -- palette -----------------------------------------------------------
        public static readonly Color Primary = Color.FromArgb(13, 110, 90);    // PharmaLink green
        public static readonly Color PrimaryDark = Color.FromArgb(9, 80, 66);
        public static readonly Color Accent = Color.FromArgb(23, 105, 170);    // links and info
        public static readonly Color Danger = Color.FromArgb(178, 42, 42);
        public static readonly Color DangerHover = Color.FromArgb(205, 60, 60);
        public static readonly Color Warning = Color.FromArgb(190, 120, 20);
        public static readonly Color Success = Color.FromArgb(28, 128, 72);
        public static readonly Color Sidebar = Color.FromArgb(24, 42, 56);
        public static readonly Color SidebarHover = Color.FromArgb(38, 62, 80);
        public static readonly Color SidebarRole = Color.FromArgb(140, 205, 185);
        public static readonly Color SidebarUser = Color.FromArgb(190, 205, 216);
        public static readonly Color PageBack = Color.FromArgb(244, 246, 248);
        public static readonly Color CardBack = Color.White;
        public static readonly Color Border = Color.FromArgb(214, 220, 226);
        public static readonly Color TextDark = Color.FromArgb(28, 36, 44);
        public static readonly Color TextMuted = Color.FromArgb(105, 118, 130);
        public static readonly Color HeaderSubtitle = Color.FromArgb(200, 230, 220);
        public static readonly Color RowAlt = Color.FromArgb(248, 250, 251);
        public static readonly Color GridSelection = Color.FromArgb(214, 234, 228);

        // row and field states
        public static readonly Color LowStockBack = Color.FromArgb(255, 226, 226);
        public static readonly Color DeliveredBack = Color.FromArgb(226, 246, 232);
        public static readonly Color PendingBack = Color.FromArgb(255, 246, 224);   // Placed orders, Pending shops and prescriptions
        public static readonly Color InactiveBack = Color.FromArgb(240, 240, 240); // delisted, cancelled, hidden, paused
        public static readonly Color WarningBack = Color.FromArgb(255, 240, 220);
        public static readonly Color TotalRowBack = Color.FromArgb(228, 240, 236);
        public static readonly Color ReadOnlyBack = Color.FromArgb(240, 242, 244);
        public static readonly Color ErrorFieldBack = Color.FromArgb(255, 240, 240);

        // disabled buttons: a light grey ground keeps the system's grey text readable
        public static readonly Color DisabledBack = Color.FromArgb(226, 230, 234);
        public static readonly Color DisabledText = Color.FromArgb(170, 190, 184);

        // login brand panel
        public static readonly Color BrandMint = Color.FromArgb(150, 220, 200);
        public static readonly Color BrandMintSoft = Color.FromArgb(185, 225, 212);
        public static readonly Color BrandMintPale = Color.FromArgb(210, 235, 226);
        public static readonly Color BrandMintFaint = Color.FromArgb(220, 240, 234);

        // -- fonts -------------------------------------------------------------
        // Created once and shared, so no screen allocates a Font per paint.
        public static readonly Font FontTitle = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
        public static readonly Font FontTitleLarge = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
        public static readonly Font FontHeading = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        public static readonly Font FontSubheading = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
        public static readonly Font FontBody = new Font("Segoe UI", 9.75F);
        public static readonly Font FontBodyBold = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
        public static readonly Font FontValue = new Font("Segoe UI", 10.5F);
        public static readonly Font FontSmall = new Font("Segoe UI", 8.5F);
        public static readonly Font FontCaption = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
        public static readonly Font FontBadge = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        public static readonly Font FontButtonStrong = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        public static readonly Font FontButtonLarge = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        public static readonly Font FontTileValue = new Font("Segoe UI Semibold", 18F, FontStyle.Bold);
        public static readonly Font FontPriceHero = new Font("Segoe UI Semibold", 20F, FontStyle.Bold);
        public static readonly Font FontStrikePrice = new Font("Segoe UI", 11F, FontStyle.Strikeout);
        public static readonly Font FontStars = new Font("Segoe UI Symbol", 16F);
        public static readonly Font FontBrand = new Font("Segoe UI Semibold", 15F, FontStyle.Bold);
        public static readonly Font FontLoginMark = new Font("Segoe UI", 30F, FontStyle.Bold);
        public static readonly Font FontLoginBrand = new Font("Segoe UI Semibold", 28F, FontStyle.Bold);
        public static readonly Font FontTagline = new Font("Segoe UI", 11F);
        public static readonly Font FontMono = new Font("Consolas", 9.5F);
        public static readonly Font FontInvoice = new Font("Consolas", 10F);
        public static readonly Font FontInvoicePrint = new Font("Consolas", 9F);

        // -- form --------------------------------------------------------------
        /// <summary>
        /// Colours, caption and start position for a form.
        ///
        /// The form's Font is deliberately left alone. Every form uses
        /// AutoScaleMode.Font and was laid out in the designer at the project's
        /// default 9pt font; assigning a bigger font here at run time made
        /// WinForms rescale every designer control after load, while anything
        /// placed in code (the dashboard tiles) kept its unscaled position and
        /// ended up overlapping the header and the sidebar.
        /// </summary>
        public static void StyleForm(Form form, string title)
        {
            form.BackColor = PageBack;
            form.ForeColor = TextDark;
            form.StartPosition = form.Modal || form.Owner != null
                ? FormStartPosition.CenterParent
                : FormStartPosition.CenterScreen;
            form.Text = "PharmaLink  -  " + title;
        }

        /// <summary>
        /// Makes a list screen resizable without letting it shrink below the
        /// size it was designed at. Controls must carry Anchor values for this
        /// to be useful.
        /// </summary>
        public static void MakeResizable(Form form, Size minimum)
        {
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MaximizeBox = true;
            form.MinimumSize = minimum;
        }

        /// <summary>The coloured strip across the top of every screen.</summary>
        public static Panel BuildHeader(string title, string subtitle)
        {
            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 68;
            header.BackColor = Primary;

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = FontTitle;
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(18, 10);
            header.Controls.Add(lblTitle);

            Label lblSub = new Label();
            lblSub.Text = subtitle;
            lblSub.Font = FontSmall;
            lblSub.ForeColor = HeaderSubtitle;
            lblSub.AutoSize = true;
            lblSub.Location = new Point(21, 40);
            header.Controls.Add(lblSub);

            return header;
        }

        /// <summary>Applies the header colours to a header that was laid out in the designer.</summary>
        public static void StyleHeader(Panel header, Label title, Label subtitle)
        {
            header.BackColor = Primary;
            if (title != null)
            {
                title.Font = FontTitle;
                title.ForeColor = Color.White;
            }
            if (subtitle != null)
            {
                subtitle.Font = FontSmall;
                subtitle.ForeColor = HeaderSubtitle;
            }
        }

        // -- buttons -----------------------------------------------------------
        public static void StylePrimary(Button button)
        {
            StyleFlat(button, Primary, Color.White);
        }

        public static void StyleSecondary(Button button)
        {
            StyleFlat(button, Color.White, TextDark);
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.BorderSize = 1;
        }

        public static void StyleDanger(Button button)
        {
            StyleFlat(button, Danger, Color.White);
        }

        public static void StyleAccent(Button button)
        {
            StyleFlat(button, Accent, Color.White);
        }

        public static void StyleSuccess(Button button)
        {
            StyleFlat(button, Success, Color.White);
        }

        // The colour each styled button should return to when it is enabled again.
        private static readonly ConditionalWeakTable<Button, ButtonColours> EnabledColours =
            new ConditionalWeakTable<Button, ButtonColours>();

        private sealed class ButtonColours
        {
            public Color Back;
            public Color Fore;
        }

        private static void StyleFlat(Button button, Color back, Color fore)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.ForeColor = fore;
            button.Font = FontBody;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;

            // WinForms always paints a disabled flat button's text in system
            // grey, which is unreadable on the dark green, red and blue buttons.
            // Swapping to a light grey ground while disabled keeps the caption
            // legible and makes "you cannot press this yet" obvious.
            EnabledColours.AddOrUpdate(button, new ButtonColours { Back = back, Fore = fore });
            button.EnabledChanged -= OnStyledButtonEnabledChanged;
            button.EnabledChanged += OnStyledButtonEnabledChanged;
            ApplyEnabledColours(button);
        }

        private static void OnStyledButtonEnabledChanged(object sender, EventArgs e)
        {
            if (sender is Button button) ApplyEnabledColours(button);
        }

        private static void ApplyEnabledColours(Button button)
        {
            if (!EnabledColours.TryGetValue(button, out ButtonColours colours)) return;
            button.BackColor = button.Enabled ? colours.Back : DisabledBack;
            button.Cursor = button.Enabled ? Cursors.Hand : Cursors.Default;
        }

        /// <summary>A left menu button, used on all three dashboards.</summary>
        public static void StyleSidebarButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = SidebarHover;
            button.BackColor = Sidebar;
            button.ForeColor = Color.White;
            button.Font = FontBody;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(16, 0, 0, 0);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        // -- grids -------------------------------------------------------------
        /// <summary>The single grid style used by every DataGridView in the application.</summary>
        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = CardBack;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = Border;
            grid.EnableHeadersVisualStyles = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeight = 36;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Sidebar;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = FontBodyBold;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Sidebar;

            grid.DefaultCellStyle.Font = FontBody;
            grid.DefaultCellStyle.ForeColor = TextDark;
            grid.DefaultCellStyle.SelectionBackColor = GridSelection;
            grid.DefaultCellStyle.SelectionForeColor = TextDark;
            grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            grid.AlternatingRowsDefaultCellStyle.BackColor = RowAlt;
            grid.RowTemplate.Height = 30;
        }

        /// <summary>
        /// Gives a column a share of the grid's width and a floor it will not be
        /// squeezed below, so short codes stay narrow and names stay readable.
        /// Does nothing when the column is not in the grid.
        ///
        /// minimumWidth is written at 100% display scaling. DataGridView column
        /// widths are raw pixels while the text inside them grows with Windows
        /// scaling, so the floor is scaled by the grid's DPI; without that, a
        /// laptop at 125% or 150% truncated "Confirmed" and "500mg".
        /// </summary>
        public static void SizeColumn(DataGridView grid, string columnName, float fillWeight, int minimumWidth)
        {
            if (!grid.Columns.Contains(columnName)) return;
            DataGridViewColumn column = grid.Columns[columnName];
            column.FillWeight = fillWeight;
            column.MinimumWidth = Math.Max(2, (int)Math.Round(minimumWidth * grid.DeviceDpi / 96.0));
        }

        /// <summary>
        /// Draws a centred grey message on an empty grid, for example
        /// "No orders match these filters", so an empty screen never looks broken.
        /// Call once; the message is read from the grid's Tag each time it paints.
        /// </summary>
        public static void EnableEmptyMessage(DataGridView grid, string message)
        {
            grid.Tag = message;
            grid.Paint -= OnGridPaintEmptyMessage;
            grid.Paint += OnGridPaintEmptyMessage;
        }

        private static void OnGridPaintEmptyMessage(object sender, PaintEventArgs e)
        {
            if (!(sender is DataGridView grid) || grid.Rows.Count > 0) return;
            if (!(grid.Tag is string message) || message.Length == 0) return;

            Rectangle area = grid.ClientRectangle;
            area.Y += grid.ColumnHeadersVisible ? grid.ColumnHeadersHeight : 0;
            area.Height -= grid.ColumnHeadersVisible ? grid.ColumnHeadersHeight : 0;
            TextRenderer.DrawText(e.Graphics, message, FontBody, area, TextMuted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        // -- cards and tiles ---------------------------------------------------
        public static Panel BuildCard()
        {
            Panel card = new Panel();
            card.BackColor = CardBack;
            card.Padding = new Padding(14);
            card.BorderStyle = BorderStyle.FixedSingle;
            return card;
        }

        /// <summary>
        /// A summary tile: caption on top, big number underneath.
        /// The Label holding the number is returned in valueLabel so the form can
        /// refresh it without rebuilding the tile.
        /// </summary>
        public static Panel BuildTile(string caption, Color stripe, out Label valueLabel)
        {
            Panel tile = new Panel();
            tile.BackColor = CardBack;
            tile.BorderStyle = BorderStyle.FixedSingle;
            tile.Size = new Size(210, 84);

            Panel bar = new Panel();
            bar.Dock = DockStyle.Left;
            bar.Width = 5;
            bar.BackColor = stripe;
            tile.Controls.Add(bar);

            Label lblCaption = new Label();
            lblCaption.Text = caption;
            lblCaption.Font = FontSmall;
            lblCaption.ForeColor = TextMuted;
            lblCaption.AutoSize = true;
            lblCaption.Location = new Point(16, 12);
            tile.Controls.Add(lblCaption);

            valueLabel = new Label();
            valueLabel.Text = "0";
            valueLabel.Font = FontTileValue;
            valueLabel.ForeColor = TextDark;
            valueLabel.AutoSize = true;
            valueLabel.Location = new Point(14, 34);
            tile.Controls.Add(valueLabel);

            return tile;
        }

        /// <summary>
        /// Lays a row of tiles out from real, already-laid-out controls rather
        /// than from hard-coded pixel positions: the row starts just right of
        /// <paramref name="leftOf"/> (the sidebar) and just below
        /// <paramref name="below"/> (the header), and the tiles share the width
        /// that is left. Call it from Load and again from Resize.
        /// </summary>
        public static void LayoutTileRow(Control host, Control leftOf, Control below, int rightMargin, params Panel[] tiles)
        {
            if (tiles == null || tiles.Length == 0) return;

            const int gap = 14;
            const int margin = 24;
            int left = (leftOf != null ? leftOf.Right : 0) + margin;
            int top = (below != null ? below.Bottom : 0) + 16;
            int available = host.ClientSize.Width - left - rightMargin;
            int width = Math.Max(160, (available - gap * (tiles.Length - 1)) / tiles.Length);

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i].Parent != host) host.Controls.Add(tiles[i]);
                tiles[i].Bounds = new Rectangle(left + i * (width + gap), top, width, 84);
                tiles[i].BringToFront();
            }
        }

        // -- validation labels -------------------------------------------------
        /// <summary>Shows a red message under a field and outlines the field in red.</summary>
        public static void ShowError(Label errorLabel, Control field, string message)
        {
            errorLabel.Text = message;
            errorLabel.ForeColor = Danger;
            errorLabel.Visible = true;
            if (field != null)
            {
                field.BackColor = ErrorFieldBack;
            }
        }

        public static void ClearError(Label errorLabel, Control field)
        {
            errorLabel.Text = string.Empty;
            errorLabel.Visible = false;
            if (field != null)
            {
                field.BackColor = Color.White;
            }
        }

        public static Label BuildErrorLabel(Point location, int width)
        {
            Label label = new Label();
            label.AutoSize = false;
            label.Size = new Size(width, 18);
            label.Location = location;
            label.Font = FontSmall;
            label.ForeColor = Danger;
            label.Visible = false;
            return label;
        }

        // -- money -------------------------------------------------------------
        public static string Money(decimal amount)
        {
            return "Tk " + amount.ToString("N2");
        }
    }
}
