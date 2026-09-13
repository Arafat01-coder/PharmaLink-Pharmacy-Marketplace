using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using PharmaLinkApp.Database;
using PharmaLinkApp.Helpers;
using PharmaLinkApp.Models;
using PharmaLinkApp.Services;

namespace PharmaLinkApp.Forms
{
    /// <summary>
    /// The printable bill produced after checkout, and reopened later from the
    /// customer's order history or from the pharmacy owner's order list.
    ///
    /// It carries the order number, both addresses, the pharmacy's DGDA licence
    /// number, the line items at the price actually charged and the grand total.
    /// TotalAmount comes from the computed column in the database, so the bill
    /// total can never disagree with its own parts.
    ///
    /// The form is opened with nothing but an order number, so it checks who is
    /// asking before showing anything: a customer sees only their own orders and
    /// a pharmacy owner only orders placed with their pharmacy. The wallet
    /// number is masked to its last three digits.
    /// </summary>
    public partial class InvoiceForm : Form
    {
        private readonly OrderService _orders = new OrderService();
        private readonly int _orderId;

        private Order _order;
        private decimal _savings;
        private string _printText = "";
        private int _printCharsPrinted;

        public InvoiceForm(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
        }

        private void InvoiceForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            BuildInvoice();
        }

        private void ApplyTheme()
        {
            UiTheme.StyleForm(this, "Invoice");
            StartPosition = FormStartPosition.CenterParent;

            UiTheme.StyleHeader(panelHeader, lblTitle, lblSubtitle);

            rtbInvoice.Font = UiTheme.FontInvoice;
            rtbInvoice.ForeColor = UiTheme.TextDark;

            lblFooterNote.Font = UiTheme.FontSmall;
            lblFooterNote.ForeColor = UiTheme.TextMuted;

            UiTheme.StylePrimary(btnPrint);
            UiTheme.StyleAccent(btnSavePdf);
            UiTheme.StyleSecondary(btnSaveText);
            UiTheme.StyleSecondary(btnClose);
        }

        // ---------------------------------------------------------------------

        private void BuildInvoice()
        {
            try
            {
                _order = _orders.GetOrderWithItems(_orderId);

                if (_order == null)
                {
                    MessageBox.Show("That order could not be found.", "PharmaLink",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                if (!CurrentUserMayView(_order))
                {
                    _order = null;
                    MessageBox.Show("This invoice belongs to another account, so it cannot be shown.", "PharmaLink",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                _savings = _orders.GetDiscountSavings(_orderId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The invoice could not be loaded.\r\n\r\n" + DbHelper.Describe(ex), "PharmaLink",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            lblTitle.Text = "Invoice  #" + _order.OrderId;
            lblSubtitle.Text = _order.PharmacyName + "   -   " + _order.OrderDate.ToString("dd MMM yyyy, h:mm tt") +
                               "   -   status: " + _order.Status;

            _printText = ComposeInvoiceText(_order, _savings);
            rtbInvoice.Text = _printText;
        }

        /// <summary>
        /// Ownership rule for an invoice: the customer who placed it, the owner
        /// of the pharmacy it was placed with, or the platform's Super Admin.
        /// Anyone else - including a signed-out session - is refused.
        /// </summary>
        private static bool CurrentUserMayView(Order order)
        {
            if (UserSession.IsCustomer) return order.CustomerId == UserSession.UserId;
            if (UserSession.IsAdmin) return UserSession.PharmacyId != 0 && order.PharmacyId == UserSession.PharmacyId;
            return UserSession.IsSuperAdmin;
        }

        /// <summary>Lays the bill out as fixed width text so it prints exactly as it looks.</summary>
        private static string ComposeInvoiceText(Order order, decimal savings)
        {
            const int width = 78;
            StringBuilder bill = new StringBuilder();

            bill.AppendLine(Centre("P H A R M A L I N K", width));
            bill.AppendLine(Centre("Pharmacy Marketplace", width));
            bill.AppendLine(new string('=', width));
            bill.AppendLine();

            bill.AppendLine("INVOICE  #" + order.OrderId);
            bill.AppendLine("Date      " + order.OrderDate.ToString("dd MMM yyyy, h:mm tt"));
            bill.AppendLine("Status    " + order.Status);
            bill.AppendLine("Payment   " + FriendlyPayment(order.PaymentMethod) +
                            (string.IsNullOrWhiteSpace(order.PaymentMobile)
                                ? "" : "   (wallet " + MaskWallet(order.PaymentMobile) + ")"));
            bill.AppendLine();
            bill.AppendLine(new string('-', width));

            bill.AppendLine("SOLD BY");
            bill.AppendLine("  " + order.PharmacyName);
            bill.AppendLine("  " + order.PharmacyAddress);
            bill.AppendLine("  DGDA drug licence: " + order.PharmacyLicense);
            bill.AppendLine();

            bill.AppendLine("DELIVERED TO");
            bill.AppendLine("  " + order.CustomerName + "   (" + order.CustomerPhone + ")");
            bill.AppendLine("  " + order.DeliveryAddress);
            bill.AppendLine(new string('-', width));
            bill.AppendLine();

            bill.AppendLine(
                "MEDICINE".PadRight(34) +
                "QTY".PadLeft(5) +
                "UNIT PRICE".PadLeft(15) +
                "LINE TOTAL".PadLeft(16));
            bill.AppendLine(new string('-', width));

            foreach (OrderItem item in order.Items)
            {
                string name = item.MedicineName;
                if (!string.IsNullOrWhiteSpace(item.Strength)) name += " " + item.Strength;
                if (name.Length > 33) name = name.Substring(0, 30) + "...";

                bill.AppendLine(
                    name.PadRight(34) +
                    item.Quantity.ToString().PadLeft(5) +
                    item.UnitPrice.ToString("N2").PadLeft(15) +
                    item.Subtotal.ToString("N2").PadLeft(16));
            }

            bill.AppendLine(new string('-', width));
            if (savings > 0m)
                bill.AppendLine("Offer savings (already taken off the prices above)".PadRight(54) +
                                ("Tk " + savings.ToString("N2")).PadLeft(24));
            bill.AppendLine("Items total".PadRight(54) + ("Tk " + order.ItemsTotal.ToString("N2")).PadLeft(24));
            bill.AppendLine("Delivery charge".PadRight(54) + ("Tk " + order.DeliveryCharge.ToString("N2")).PadLeft(24));
            bill.AppendLine(new string('=', width));
            bill.AppendLine("GRAND TOTAL".PadRight(54) + ("Tk " + order.TotalAmount.ToString("N2")).PadLeft(24));
            bill.AppendLine(new string('=', width));
            bill.AppendLine();

            bill.AppendLine("Thank you for using PharmaLink.");
            bill.AppendLine("The platform commission is deducted from the pharmacy, not added to this bill.");
            bill.AppendLine();
            bill.AppendLine(Centre("This is a computer generated invoice.", width));

            return bill.ToString();
        }

        /// <summary>Shows only the last three digits of a wallet number, e.g. ********006.</summary>
        private static string MaskWallet(string number)
        {
            string trimmed = number.Trim();
            if (trimmed.Length <= 3) return new string('*', trimmed.Length);
            return new string('*', trimmed.Length - 3) + trimmed.Substring(trimmed.Length - 3);
        }

        private static string Centre(string text, int width)
        {
            if (text.Length >= width) return text;
            int padding = (width - text.Length) / 2;
            return new string(' ', padding) + text;
        }

        private static string FriendlyPayment(string stored)
        {
            switch (stored)
            {
                case "CashOnDelivery": return "Cash on delivery";
                case "bKash": return "bKash";
                case "Nagad": return "Nagad";
                case "Card": return "Card";
                default: return stored;
            }
        }

        // ---------------------------------------------------------------------
        //  PRINT AND SAVE
        // ---------------------------------------------------------------------

        /// <summary>The PDF printer that ships with Windows 10 and 11.</summary>
        private const string PdfPrinterName = "Microsoft Print to PDF";

        /// <summary>
        /// The one print setup shared by Print and Save as PDF, so the PDF is
        /// the paper invoice page for page. The character counter is reset in
        /// BeginPrint rather than by the caller: every print job (a second
        /// Print, or a PDF after a Print) then starts again from the first
        /// character, and pagination in Document_PrintPage keeps working.
        /// The caller owns and disposes the document.
        /// </summary>
        private PrintDocument CreateInvoiceDocument()
        {
            PrintDocument document = new PrintDocument();
            document.DocumentName = "PharmaLink invoice " + _orderId;
            document.BeginPrint += (s, e) => _printCharsPrinted = 0;
            document.PrintPage += Document_PrintPage;
            return document;
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (_order == null) return;

            try
            {
                using (PrintDocument document = CreateInvoiceDocument())
                using (PrintDialog dialog = new PrintDialog())
                {
                    dialog.Document = document;
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                        document.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The invoice could not be printed.\r\n\r\n" + DbHelper.Describe(ex) +
                                "\r\n\r\nYou can still save it as a text file.",
                    "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Saves the invoice as a PDF by printing the same document to the
        /// "Microsoft Print to PDF" printer with PrintToFile, so no PDF library
        /// is needed. StandardPrintController suppresses the "Printing page 1"
        /// progress window. The printer can be removed from Windows, so its
        /// presence is checked first and the user is pointed at Print instead.
        /// </summary>
        private void btnSavePdf_Click(object sender, EventArgs e)
        {
            if (_order == null) return;

            string path;
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PDF document (*.pdf)|*.pdf";
                dialog.DefaultExt = "pdf";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;
                dialog.FileName = "PharmaLink-Invoice-" + _orderId + ".pdf";

                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                path = dialog.FileName;
            }

            string message;
            bool saved;
            Cursor = Cursors.WaitCursor;
            try
            {
                saved = SaveAsPdf(path, out message);
            }
            catch (Exception ex)
            {
                saved = false;
                message = "The PDF could not be saved.\r\n\r\n" + DbHelper.Describe(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (saved)
                MessageBox.Show(message, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(message, "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Prints the invoice to <paramref name="path"/> through the PDF printer
        /// and checks the file really appeared. Returns false with a plain
        /// message when the printer is missing or no file was written.
        ///
        /// The old file is removed first (the save dialog already asked before
        /// overwriting), otherwise a failed print would leave the previous PDF
        /// in place and "the file exists" would prove nothing. The spooler can
        /// finish writing a moment after Print returns, so the check waits a
        /// few seconds for a non-empty file.
        /// </summary>
        private bool SaveAsPdf(string path, out string message)
        {
            using (PrintDocument document = CreateInvoiceDocument())
            {
                document.PrinterSettings.PrinterName = PdfPrinterName;
                if (!document.PrinterSettings.IsValid)
                {
                    message = "The \"" + PdfPrinterName + "\" printer is not installed on this computer, " +
                              "so the invoice cannot be saved as a PDF directly.\r\n\r\n" +
                              "Use Print instead and choose a PDF printer from the list, " +
                              "or save the invoice as a text file.";
                    return false;
                }

                try
                {
                    if (File.Exists(path)) File.Delete(path);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    message = "The existing file could not be replaced - it may be open in another program.\r\n\r\n" +
                              "Close it, or choose a different file name, and try again.";
                    return false;
                }

                document.PrinterSettings.PrintToFile = true;
                document.PrinterSettings.PrintFileName = path;
                document.PrintController = new StandardPrintController();
                document.Print();
            }

            DateTime deadline = DateTime.Now.AddSeconds(10);
            while (DateTime.Now < deadline)
            {
                FileInfo file = new FileInfo(path);
                if (file.Exists && file.Length > 0)
                {
                    message = "Invoice #" + _orderId + " saved as a PDF to:\r\n\r\n" + path;
                    return true;
                }
                Thread.Sleep(200);
            }

            message = "Windows did not write the PDF file.\r\n\r\n" +
                      "Try again, or use Print and choose a PDF printer from the list.";
            return false;
        }

        private void Document_PrintPage(object sender, PrintPageEventArgs e)
        {
            // The print font is the shared theme font, so it is not disposed here.
            Font font = UiTheme.FontInvoicePrint;
            RectangleF area = e.MarginBounds;

            using (StringFormat format = new StringFormat(StringFormatFlags.LineLimit))
            {
                int charactersFitted, linesFilled;
                e.Graphics.MeasureString(_printText.Substring(_printCharsPrinted), font, area.Size, format,
                                         out charactersFitted, out linesFilled);

                e.Graphics.DrawString(_printText.Substring(_printCharsPrinted), font, Brushes.Black, area, format);

                _printCharsPrinted += charactersFitted;
                e.HasMorePages = _printCharsPrinted < _printText.Length;
            }
        }

        private void btnSaveText_Click(object sender, EventArgs e)
        {
            if (_order == null) return;

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text file (*.txt)|*.txt";
                dialog.FileName = "PharmaLink-invoice-" + _orderId + ".txt";

                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    File.WriteAllText(dialog.FileName, _printText, Encoding.UTF8);
                    MessageBox.Show("Invoice saved to " + dialog.FileName, "Saved",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The invoice could not be saved.\r\n\r\n" + DbHelper.Describe(ex), "PharmaLink",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
