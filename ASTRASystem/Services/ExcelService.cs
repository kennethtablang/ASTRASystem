using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using ClosedXML.Excel;

namespace ASTRASystem.Services
{
    public class ExcelService : IExcelService
    {
        private readonly ILogger<ExcelService> _logger;

        public ExcelService(ILogger<ExcelService> logger)
        {
            _logger = logger;
        }

        public byte[] ExportOrdersToExcel(List<Order> orders)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Orders");

                // Headers
                worksheet.Cell(1, 1).Value = "Order ID";
                worksheet.Cell(1, 2).Value = "Store";
                worksheet.Cell(1, 3).Value = "Barangay";
                worksheet.Cell(1, 4).Value = "City";
                worksheet.Cell(1, 5).Value = "Status";
                worksheet.Cell(1, 6).Value = "Priority";
                worksheet.Cell(1, 7).Value = "Total";
                worksheet.Cell(1, 8).Value = "Items";
                worksheet.Cell(1, 9).Value = "Created";
                worksheet.Cell(1, 10).Value = "Scheduled For";

                // Style headers
                var headerRange = worksheet.Range(1, 1, 1, 10);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data
                int row = 2;
                foreach (var order in orders)
                {
                    worksheet.Cell(row, 1).Value = order.Id;
                    worksheet.Cell(row, 2).Value = order.Store?.Name ?? "";
                    worksheet.Cell(row, 3).Value = order.Store?.Barangay?.Name ?? "";
                    worksheet.Cell(row, 4).Value = order.Store?.City?.Name ?? "";
                    worksheet.Cell(row, 5).Value = order.Status.ToString();
                    worksheet.Cell(row, 6).Value = order.Priority ? "Yes" : "No";
                    worksheet.Cell(row, 7).Value = order.Total;
                    worksheet.Cell(row, 8).Value = order.Items?.Count ?? 0;
                    worksheet.Cell(row, 9).Value = order.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(row, 10).Value = order.ScheduledFor?.ToString("yyyy-MM-dd") ?? "";
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting orders to Excel");
                throw;
            }
        }

        public byte[] ExportPaymentsToExcel(List<Payment> payments)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Payments");

                // Headers
                worksheet.Cell(1, 1).Value = "Payment ID";
                worksheet.Cell(1, 2).Value = "Order ID";
                worksheet.Cell(1, 3).Value = "Amount";
                worksheet.Cell(1, 4).Value = "Method";
                worksheet.Cell(1, 5).Value = "Reference";
                worksheet.Cell(1, 6).Value = "Recorded At";
                worksheet.Cell(1, 7).Value = "Recorded By";

                // Style headers
                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data
                int row = 2;
                foreach (var payment in payments)
                {
                    worksheet.Cell(row, 1).Value = payment.Id;
                    worksheet.Cell(row, 2).Value = payment.OrderId;
                    worksheet.Cell(row, 3).Value = payment.Amount;
                    worksheet.Cell(row, 4).Value = payment.Method.ToString();
                    worksheet.Cell(row, 5).Value = payment.Reference ?? "";
                    worksheet.Cell(row, 6).Value = payment.RecordedAt.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(row, 7).Value = payment.RecordedById ?? "";
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Format currency column
                worksheet.Column(3).Style.NumberFormat.Format = "₱#,##0.00";

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting payments to Excel");
                throw;
            }
        }

        public byte[] ExportStoresToExcel(List<Store> stores)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Stores");

                // Headers
                worksheet.Cell(1, 1).Value = "Store ID";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Owner";
                worksheet.Cell(1, 4).Value = "Phone";
                worksheet.Cell(1, 5).Value = "Barangay";
                worksheet.Cell(1, 6).Value = "City";
                worksheet.Cell(1, 7).Value = "Province";
                worksheet.Cell(1, 8).Value = "Credit Limit";
                worksheet.Cell(1, 9).Value = "Payment Method";
                worksheet.Cell(1, 10).Value = "Created";

                // Style headers
                var headerRange = worksheet.Range(1, 1, 1, 10);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data
                int row = 2;
                foreach (var store in stores)
                {
                    worksheet.Cell(row, 1).Value = store.Id;
                    worksheet.Cell(row, 2).Value = store.Name;
                    worksheet.Cell(row, 3).Value = store.OwnerName ?? "";
                    worksheet.Cell(row, 4).Value = store.Phone ?? "";
                    worksheet.Cell(row, 5).Value = store.Barangay?.Name ?? "";
                    worksheet.Cell(row, 6).Value = store.City?.Name ?? "";
                    worksheet.Cell(row, 7).Value = store.City?.Province ?? "";
                    worksheet.Cell(row, 8).Value = store.CreditLimit;
                    worksheet.Cell(row, 9).Value = store.PreferredPaymentMethod ?? "";
                    worksheet.Cell(row, 10).Value = store.CreatedAt.ToString("yyyy-MM-dd");
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Format currency column
                worksheet.Column(8).Style.NumberFormat.Format = "₱#,##0.00";

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting stores to Excel");
                throw;
            }
        }

        public byte[] ExportProductsToExcel(List<Product> products)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Products");

                // Headers
                worksheet.Cell(1, 1).Value = "Product ID";
                worksheet.Cell(1, 2).Value = "SKU";
                worksheet.Cell(1, 3).Value = "Name";
                worksheet.Cell(1, 4).Value = "Category";
                worksheet.Cell(1, 5).Value = "Price";
                worksheet.Cell(1, 6).Value = "Unit of Measure";
                worksheet.Cell(1, 7).Value = "Perishable";
                worksheet.Cell(1, 8).Value = "Created";

                // Style headers
                var headerRange = worksheet.Range(1, 1, 1, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data
                int row = 2;
                foreach (var product in products)
                {
                    worksheet.Cell(row, 1).Value = product.Id;
                    worksheet.Cell(row, 2).Value = product.Sku;
                    worksheet.Cell(row, 3).Value = product.Name;
                    worksheet.Cell(row, 4).Value = product.Category?.Name ?? "";
                    worksheet.Cell(row, 5).Value = product.Price;
                    worksheet.Cell(row, 6).Value = product.UnitOfMeasure ?? "";
                    worksheet.Cell(row, 7).Value = product.IsPerishable ? "Yes" : "No";
                    worksheet.Cell(row, 8).Value = product.CreatedAt.ToString("yyyy-MM-dd");
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Format currency column
                worksheet.Column(5).Style.NumberFormat.Format = "₱#,##0.00";

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting products to Excel");
                throw;
            }
        }
    }
}