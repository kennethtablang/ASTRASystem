using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using System.Text;

namespace ASTRASystem.Services
{
    public class ThermalReceiptService : IThermalReceiptService
    {
        private readonly ILogger<ThermalReceiptService> _logger;

        public ThermalReceiptService(ILogger<ThermalReceiptService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generate ESC/POS receipt for thermal printer using ESCPOS_NET (58mm or 80mm)
        /// </summary>
        public string GenerateThermalReceipt(Order order, int paperWidth = 58)
        {
            var bytes = GenerateReceiptBytes(order, paperWidth);
            // Use Latin1 to strictly map byte values to char values 1:1 to preserve binary data in string
            return Encoding.Latin1.GetString(bytes);
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
            var bytes = GenerateReceiptBytes(order, paperWidth);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Generate receipt as byte array for direct printing
        /// </summary>
        public byte[] GenerateReceiptBytes(Order order, int paperWidth = 58)
        {
            try
            {
                var e = new EPSON();
                var cmds = new List<byte[]>();
                int maxChars = paperWidth == 58 ? 32 : 48;

                // Header - Company Name (Centered, Bold, Double Height)
                cmds.Add(e.PrintLine("ASTRA SYSTEM"));
                
                cmds.Add(e.PrintLine("Distribution Management"));
                cmds.Add(e.PrintLine(new string('-', maxChars)));

                // Store Information
                cmds.Add(e.PrintLine($"STORE: {TruncateText(order.Store.Name, maxChars)}"));

                if (!string.IsNullOrEmpty(order.Store.OwnerName))
                    cmds.Add(e.PrintLine($"Owner: {TruncateText(order.Store.OwnerName, maxChars)}"));

                // Barangay and City
                var barangayName = order.Store.Barangay?.Name ?? "";
                var cityName = order.Store.City?.Name ?? "";

                if (!string.IsNullOrEmpty(barangayName))
                    cmds.Add(e.PrintLine(TruncateText(barangayName, maxChars)));

                if (!string.IsNullOrEmpty(cityName))
                {
                    var cityProvince = order.Store.City?.Province;
                    var cityDisplay = !string.IsNullOrEmpty(cityProvince)
                        ? $"{cityName}, {cityProvince}"
                        : cityName;
                    cmds.Add(e.PrintLine(TruncateText(cityDisplay, maxChars)));
                }

                if (!string.IsNullOrEmpty(order.Store.Phone))
                    cmds.Add(e.PrintLine($"Tel: {order.Store.Phone}"));

                cmds.Add(e.PrintLine(new string('-', maxChars)));

                // Order Details
                cmds.Add(e.PrintLine($"ORDER #{order.Id}"));
                cmds.Add(e.PrintLine($"Date: {order.CreatedAt:yyyy-MM-dd HH:mm}"));

                if (order.ScheduledFor.HasValue)
                    cmds.Add(e.PrintLine($"Scheduled: {order.ScheduledFor.Value:yyyy-MM-dd}"));

                cmds.Add(e.PrintLine(new string('-', maxChars)));

                // Items Header
                cmds.Add(e.PrintLine(FormatReceiptLine("ITEM", "QTY", "PRICE", "TOTAL", maxChars)));
                cmds.Add(e.PrintLine(new string('-', maxChars)));

                // Order Items
                if (order.Items != null && order.Items.Any())
                {
                    foreach (var item in order.Items)
                    {
                        var productName = TruncateText(item.Product.Name, maxChars - 2);
                        cmds.Add(e.PrintLine($" {productName}"));

                        var qty = item.Quantity.ToString();
                        var price = item.UnitPrice.ToString("N2");
                        var total = (item.Quantity * item.UnitPrice).ToString("N2");

                        cmds.Add(e.PrintLine(FormatReceiptLine("", qty, price, total, maxChars)));
                    }
                }

                cmds.Add(e.PrintLine(new string('-', maxChars)));

                // Totals
                cmds.Add(e.PrintLine($"Subtotal: {FormatCurrency(order.SubTotal)}"));
                cmds.Add(e.PrintLine($"TOTAL: {FormatCurrency(order.Total)}"));

                cmds.Add(e.PrintLine(new string('-', maxChars)));

                // Payment Information
                if (order.Payments != null && order.Payments.Any())
                {
                    cmds.Add(e.PrintLine("PAYMENT DETAILS:"));

                    foreach (var payment in order.Payments)
                    {
                        cmds.Add(e.PrintLine($"{payment.Method}: {FormatCurrency(payment.Amount)}"));
                        if (!string.IsNullOrEmpty(payment.Reference))
                            cmds.Add(e.PrintLine($"  Ref: {payment.Reference}"));
                    }

                    var totalPaid = order.Payments.Sum(p => p.Amount);
                    var balance = order.Total - totalPaid;

                    if (balance > 0)
                    {
                        cmds.Add(e.PrintLine($"BALANCE DUE: {FormatCurrency(balance)}"));
                    }
                    else if (balance == 0)
                    {
                        cmds.Add(e.PrintLine("*** FULLY PAID ***"));
                    }

                    cmds.Add(e.PrintLine(new string('-', maxChars)));
                }

                // Footer
                cmds.Add(e.PrintLine("Thank you!"));
                cmds.Add(e.PrintLine("Order with us again!      "));
                cmds.Add(e.PrintLine(new string('-', maxChars)));
                cmds.Add(e.PrintLine($"Printed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"));

                cmds.Add(e.PrintLine(new string('-', maxChars)));

                // Combine all byte arrays
                return ByteSplicer.Combine(cmds.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating receipt bytes for order {OrderId}", order.Id);
                throw;
            }
        }

        #region Helper Methods

        private string FormatReceiptLine(string col1, string col2, string col3, string col4, int maxChars)
        {
            // Calculate column widths
            int col1Width = 4;
            int col2Width = 5;
            int col3Width = 6;
            int col4Width = maxChars - col1Width - col2Width - col3Width - 1;

            var line = new StringBuilder();
            line.Append(PadRight(col1, col1Width));
            line.Append(PadLeft(col2, col2Width));
            line.Append(PadLeft(col3, col3Width));
            line.Append(PadLeft(col4, col4Width));

            return line.ToString();
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

        #endregion
    }
}