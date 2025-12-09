using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using System.Text;

namespace ASTRASystem.Services
{
    public class ThermalReceiptService : IThermalReceiptService
    {
        private readonly ILogger<ThermalReceiptService> _logger;

        // ESC/POS Commands
        private const string ESC = "\x1B";
        private const string GS = "\x1D";

        // Text Formatting
        private const string INIT = ESC + "@";                    // Initialize printer
        private const string BOLD_ON = ESC + "E" + "\x01";       // Bold ON
        private const string BOLD_OFF = ESC + "E" + "\x00";      // Bold OFF
        private const string ALIGN_LEFT = ESC + "a" + "\x00";    // Left align
        private const string ALIGN_CENTER = ESC + "a" + "\x01";  // Center align
        private const string ALIGN_RIGHT = ESC + "a" + "\x02";   // Right align
        private const string SIZE_NORMAL = GS + "!" + "\x00";    // Normal size
        private const string SIZE_DOUBLE = GS + "!" + "\x11";    // Double size
        private const string SIZE_LARGE = GS + "!" + "\x22";     // Large size

        // Paper Control
        private const string CUT_PAPER = GS + "V" + "\x42" + "\x00"; // Cut paper
        private const string FEED_LINE = "\n";
        private const string FEED_LINES_3 = ESC + "d" + "\x03";

        public ThermalReceiptService(ILogger<ThermalReceiptService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generate ESC/POS receipt for thermal printer (58mm or 80mm)
        /// </summary>
        public string GenerateThermalReceipt(Order order, int paperWidth = 58)
        {
            try
            {
                var receipt = new StringBuilder();
                int maxChars = paperWidth == 58 ? 32 : 48; // Characters per line

                // Initialize printer
                receipt.Append(INIT);

                // Header - Company Name
                receipt.Append(ALIGN_CENTER);
                receipt.Append(SIZE_DOUBLE);
                receipt.Append(BOLD_ON);
                receipt.Append("ASTRA SYSTEM\n");
                receipt.Append(BOLD_OFF);
                receipt.Append(SIZE_NORMAL);

                receipt.Append("Distribution Management\n");
                receipt.Append("------------------------\n");

                // Store Information
                receipt.Append(ALIGN_LEFT);
                receipt.Append(BOLD_ON);
                receipt.Append($"STORE: {TruncateText(order.Store.Name, maxChars)}\n");
                receipt.Append(BOLD_OFF);

                if (!string.IsNullOrEmpty(order.Store.OwnerName))
                    receipt.Append($"Owner: {TruncateText(order.Store.OwnerName, maxChars)}\n");

                // Barangay and City
                var barangayName = order.Store.Barangay?.Name ?? "";
                var cityName = order.Store.City?.Name ?? "";

                if (!string.IsNullOrEmpty(barangayName))
                    receipt.Append($"{TruncateText(barangayName, maxChars)}\n");

                if (!string.IsNullOrEmpty(cityName))
                {
                    var cityProvince = order.Store.City?.Province;
                    var cityDisplay = !string.IsNullOrEmpty(cityProvince)
                        ? $"{cityName}, {cityProvince}"
                        : cityName;
                    receipt.Append($"{TruncateText(cityDisplay, maxChars)}\n");
                }

                if (!string.IsNullOrEmpty(order.Store.Phone))
                    receipt.Append($"Tel: {order.Store.Phone}\n");

                receipt.Append(GetDashedLine(maxChars));

                // Order Details
                receipt.Append(BOLD_ON);
                receipt.Append($"ORDER #{order.Id}\n");
                receipt.Append(BOLD_OFF);
                receipt.Append($"Date: {order.CreatedAt:yyyy-MM-dd HH:mm}\n");

                if (order.ScheduledFor.HasValue)
                    receipt.Append($"Scheduled: {order.ScheduledFor.Value:yyyy-MM-dd}\n");

                receipt.Append(GetDashedLine(maxChars));

                // Items Header
                receipt.Append(BOLD_ON);
                receipt.Append(FormatReceiptLine("ITEM", "QTY", "PRICE", "TOTAL", maxChars));
                receipt.Append(BOLD_OFF);
                receipt.Append(GetDashedLine(maxChars));

                // Order Items
                if (order.Items != null && order.Items.Any())
                {
                    foreach (var item in order.Items)
                    {
                        // Product name (can wrap to multiple lines)
                        var productName = TruncateText(item.Product.Name, maxChars - 2);
                        receipt.Append($"  {productName}\n");

                        // Quantity, Unit Price, Line Total
                        var qty = item.Quantity.ToString();
                        var price = item.UnitPrice.ToString("N2");
                        var total = (item.Quantity * item.UnitPrice).ToString("N2");

                        receipt.Append(FormatReceiptLine("", qty, price, total, maxChars));
                    }
                }

                receipt.Append(GetDashedLine(maxChars));

                // Totals
                receipt.Append(ALIGN_RIGHT);
                receipt.Append($"Subtotal: {FormatCurrency(order.SubTotal)}\n");
                receipt.Append($"Tax (12%): {FormatCurrency(order.Tax)}\n");

                receipt.Append(BOLD_ON);
                receipt.Append(SIZE_DOUBLE);
                receipt.Append($"TOTAL: {FormatCurrency(order.Total)}\n");
                receipt.Append(SIZE_NORMAL);
                receipt.Append(BOLD_OFF);

                receipt.Append(ALIGN_LEFT);
                receipt.Append(GetDashedLine(maxChars));

                // Payment Information (if available)
                if (order.Payments != null && order.Payments.Any())
                {
                    receipt.Append(BOLD_ON);
                    receipt.Append("PAYMENT DETAILS:\n");
                    receipt.Append(BOLD_OFF);

                    foreach (var payment in order.Payments)
                    {
                        receipt.Append($"{payment.Method}: {FormatCurrency(payment.Amount)}\n");
                        if (!string.IsNullOrEmpty(payment.Reference))
                            receipt.Append($"  Ref: {payment.Reference}\n");
                    }

                    var totalPaid = order.Payments.Sum(p => p.Amount);
                    var balance = order.Total - totalPaid;

                    if (balance > 0)
                    {
                        receipt.Append(BOLD_ON);
                        receipt.Append($"BALANCE DUE: {FormatCurrency(balance)}\n");
                        receipt.Append(BOLD_OFF);
                    }
                    else if (balance == 0)
                    {
                        receipt.Append(BOLD_ON);
                        receipt.Append("*** FULLY PAID ***\n");
                        receipt.Append(BOLD_OFF);
                    }

                    receipt.Append(GetDashedLine(maxChars));
                }

                // Footer
                receipt.Append(ALIGN_CENTER);
                receipt.Append("\n");
                receipt.Append("Thank you for your business!\n");
                receipt.Append("------------------------\n");
                receipt.Append($"Printed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

                // QR Code for order tracking (optional)
                // receipt.Append(GenerateQRCode($"ORDER-{order.Id}"));

                // Feed and cut
                receipt.Append(FEED_LINES_3);
                receipt.Append(CUT_PAPER);

                return receipt.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thermal receipt for order {OrderId}", order.Id);
                throw;
            }
        }

        /// <summary>
        /// Generate simplified receipt for 58mm printer (mobile)
        /// </summary>
        public string GenerateSimplifiedReceipt(Order order)
        {
            return GenerateThermalReceipt(order, paperWidth: 58);
        }

        /// <summary>
        /// Generate full receipt for 80mm printer (desktop)
        /// </summary>
        public string GenerateFullReceipt(Order order)
        {
            return GenerateThermalReceipt(order, paperWidth: 80);
        }

        /// <summary>
        /// Generate Base64 encoded receipt for mobile apps
        /// </summary>
        public string GenerateBase64Receipt(Order order, int paperWidth = 58)
        {
            var receipt = GenerateThermalReceipt(order, paperWidth);
            var bytes = Encoding.GetEncoding("Windows-1252").GetBytes(receipt);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Generate receipt as byte array for direct printing
        /// </summary>
        public byte[] GenerateReceiptBytes(Order order, int paperWidth = 58)
        {
            var receipt = GenerateThermalReceipt(order, paperWidth);
            return Encoding.GetEncoding("Windows-1252").GetBytes(receipt);
        }

        #region Helper Methods

        private string FormatReceiptLine(string col1, string col2, string col3, string col4, int maxChars)
        {
            // Calculate column widths
            int col1Width = maxChars / 2;
            int col2Width = 6;
            int col3Width = 8;
            int col4Width = maxChars - col1Width - col2Width - col3Width;

            var line = new StringBuilder();
            line.Append(PadRight(col1, col1Width));
            line.Append(PadLeft(col2, col2Width));
            line.Append(PadLeft(col3, col3Width));
            line.Append(PadLeft(col4, col4Width));
            line.Append("\n");

            return line.ToString();
        }

        private string GetDashedLine(int maxChars)
        {
            return new string('-', maxChars) + "\n";
        }

        private string FormatCurrency(decimal amount)
        {
            return $"P{amount:N2}";
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }

        private string PadRight(string text, int width)
        {
            if (string.IsNullOrEmpty(text))
                return new string(' ', width);

            return text.Length >= width
                ? text.Substring(0, width)
                : text + new string(' ', width - text.Length);
        }

        private string PadLeft(string text, int width)
        {
            if (string.IsNullOrEmpty(text))
                return new string(' ', width);

            return text.Length >= width
                ? text.Substring(0, width)
                : new string(' ', width - text.Length) + text;
        }

        /// <summary>
        /// Generate ESC/POS QR Code command (optional)
        /// </summary>
        private string GenerateQRCode(string data)
        {
            var qr = new StringBuilder();

            // QR Code commands for ESC/POS
            // Model
            qr.Append(GS + "(k" + "\x04\x00\x31\x41\x32\x00");

            // Size
            qr.Append(GS + "(k" + "\x03\x00\x31\x43\x03");

            // Error correction
            qr.Append(GS + "(k" + "\x03\x00\x31\x45\x31");

            // Store data
            var dataLength = data.Length + 3;
            var pL = (char)(dataLength % 256);
            var pH = (char)(dataLength / 256);
            qr.Append(GS + "(k" + pL + pH + "\x31\x50\x30" + data);

            // Print
            qr.Append(GS + "(k" + "\x03\x00\x31\x51\x30");

            qr.Append("\n");

            return qr.ToString();
        }

        #endregion
    }
}