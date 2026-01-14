using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ASTRASystem.Services
{
    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        public byte[] GenerateInvoicePdf(Invoice invoice)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(50);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("INVOICE").FontSize(20).Bold();
                                column.Item().Text($"Invoice #: {invoice.Id}").FontSize(12);
                                column.Item().Text($"Date: {invoice.IssuedAt:yyyy-MM-dd}").FontSize(10);
                            });

                            row.RelativeItem().Column(column =>
                            {
                                column.Item().AlignRight().Text("ASTRA System").FontSize(14).Bold();
                                column.Item().AlignRight().Text("Distribution Management").FontSize(10);
                            });
                        });

                        page.Content().PaddingVertical(20).Column(column =>
                        {
                            // Bill To Section
                            column.Item().PaddingVertical(10).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Bill To:").Bold();
                                    if (invoice.Order?.Store != null)
                                    {
                                        col.Item().Text(invoice.Order.Store.Name);
                                        if (!string.IsNullOrEmpty(invoice.Order.Store.OwnerName))
                                        {
                                            col.Item().Text($"Owner: {invoice.Order.Store.OwnerName}");
                                        }
                                        col.Item().Text($"{invoice.Order.Store.Barangay}, {invoice.Order.Store.City}");
                                        if (!string.IsNullOrEmpty(invoice.Order.Store.Phone))
                                        {
                                            col.Item().Text($"Phone: {invoice.Order.Store.Phone}");
                                        }
                                    }
                                });
                            });

                            // Line Items Table
                            column.Item().PaddingVertical(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(50);  // No.
                                    columns.RelativeColumn(4);   // Item
                                    columns.RelativeColumn(2);   // SKU
                                    columns.RelativeColumn(1);   // Qty
                                    columns.RelativeColumn(2);   // Unit Price
                                    columns.RelativeColumn(2);   // Total
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#").Bold();
                                    header.Cell().Element(CellStyle).Text("Item").Bold();
                                    header.Cell().Element(CellStyle).Text("SKU").Bold();
                                    header.Cell().Element(CellStyle).Text("Qty").Bold();
                                    header.Cell().Element(CellStyle).Text("Unit Price").Bold();
                                    header.Cell().Element(CellStyle).Text("Total").Bold();
                                });

                                // Items
                                if (invoice.Order?.Items != null)
                                {
                                    int index = 1;
                                    foreach (var item in invoice.Order.Items)
                                    {
                                        table.Cell().Element(CellStyle).Text(index.ToString());
                                        table.Cell().Element(CellStyle).Text(item.Product?.Name ?? "");
                                        table.Cell().Element(CellStyle).Text(item.Product?.Sku ?? "");
                                        table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                                        table.Cell().Element(CellStyle).Text($"₱{item.UnitPrice:N2}");
                                        table.Cell().Element(CellStyle).Text($"₱{(item.Quantity * item.UnitPrice):N2}");
                                        index++;
                                    }
                                }
                            });

                            // Totals Section
                            column.Item().PaddingVertical(10).AlignRight().Column(col =>
                            {
                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(150).Text("Subtotal:").Bold();
                                    row.ConstantItem(100).Text($"₱{invoice.Order?.SubTotal ?? 0:N2}");
                                });

                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(150).Text("Tax (12%):").Bold();
                                    row.ConstantItem(100).Text($"₱{invoice.TaxAmount:N2}");
                                });

                                col.Item().BorderTop(1).PaddingTop(5).Row(row =>
                                {
                                    row.ConstantItem(150).Text("Total:").Bold().FontSize(12);
                                    row.ConstantItem(100).Text($"₱{invoice.TotalAmount:N2}").Bold().FontSize(12);
                                });
                            });

                            // Payment Terms
                            column.Item().PaddingTop(20).Text("Payment Terms: Net 30 Days").FontSize(9).Italic();
                        });

                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Thank you for your business! | ");
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice PDF");
                throw;
            }
        }

        public byte[] GenerateTripManifestPdf(Trip trip)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("TRIP MANIFEST").FontSize(18).Bold();
                                column.Item().Text($"Trip #: {trip.Id}").FontSize(11);
                                column.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(9);
                            });

                            row.RelativeItem().Column(column =>
                            {
                                column.Item().AlignRight().Text("ASTRA System").FontSize(12).Bold();
                                column.Item().AlignRight().Text($"Warehouse: {trip.Warehouse?.Name}").FontSize(9);
                                column.Item().AlignRight().Text($"Vehicle: {trip.Vehicle ?? "N/A"}").FontSize(9);
                            });
                        });

                        page.Content().PaddingVertical(15).Column(column =>
                        {
                            // Summary Information
                            column.Item().PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Text($"Total Stops: {trip.Assignments?.Count ?? 0}").Bold();
                                row.RelativeItem().Text($"Status: {trip.Status}").Bold();
                                row.RelativeItem().Text($"Departure: {trip.DepartureAt?.ToString("hh:mm tt") ?? "N/A"}").Bold();
                            });

                            // Stops Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);  // Seq
                                    columns.RelativeColumn(3);   // Store
                                    columns.RelativeColumn(2);   // Address
                                    columns.RelativeColumn(1);   // Items
                                    columns.RelativeColumn(1);   // Total
                                    columns.ConstantColumn(60);  // Status
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#").Bold();
                                    header.Cell().Element(CellStyle).Text("Store").Bold();
                                    header.Cell().Element(CellStyle).Text("Address").Bold();
                                    header.Cell().Element(CellStyle).Text("Items").Bold();
                                    header.Cell().Element(CellStyle).Text("Total").Bold();
                                    header.Cell().Element(CellStyle).Text("Status").Bold();
                                });

                                // Stops
                                if (trip.Assignments != null)
                                {
                                    foreach (var assignment in trip.Assignments.OrderBy(a => a.SequenceNo))
                                    {
                                        table.Cell().Element(CellStyle).Text(assignment.SequenceNo.ToString());
                                        table.Cell().Element(CellStyle).Text(assignment.Order?.Store?.Name ?? "");
                                        table.Cell().Element(CellStyle).Text($"{assignment.Order?.Store?.Barangay}, {assignment.Order?.Store?.City}");
                                        table.Cell().Element(CellStyle).Text(assignment.Order?.Items?.Count.ToString() ?? "0");
                                        table.Cell().Element(CellStyle).Text($"₱{assignment.Order?.Total ?? 0:N2}");
                                        table.Cell().Element(CellStyle).Text(assignment.Status.ToString());
                                    }
                                }
                            });

                            // Item Details per Stop
                            column.Item().PaddingTop(15).Text("Item Details by Stop:").FontSize(11).Bold();

                            if (trip.Assignments != null)
                            {
                                foreach (var assignment in trip.Assignments.OrderBy(a => a.SequenceNo))
                                {
                                    column.Item().PaddingTop(10).Column(col =>
                                    {
                                        col.Item().Background(Colors.Grey.Lighten3).Padding(5)
                                            .Text($"Stop {assignment.SequenceNo}: {assignment.Order?.Store?.Name}").FontSize(10).Bold();

                                        if (assignment.Order?.Items != null)
                                        {
                                            foreach (var item in assignment.Order.Items)
                                            {
                                                col.Item().PaddingLeft(10).Row(row =>
                                                {
                                                    row.RelativeItem().Text($"• {item.Product?.Name} ({item.Product?.Sku})");
                                                    row.ConstantItem(80).AlignRight().Text($"Qty: {item.Quantity}");
                                                });
                                            }
                                        }
                                    });
                                }
                            }
                        });

                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("ASTRA Distribution System | ");
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating trip manifest PDF");
                throw;
            }
        }

        public byte[] GeneratePackingSlipPdf(Order order)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(50);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("PACKING SLIP").FontSize(18).Bold();
                                column.Item().Text($"Order #: {order.Id}").FontSize(12);
                                column.Item().Text($"Date: {order.CreatedAt:yyyy-MM-dd}").FontSize(10);
                            });

                            row.RelativeItem().Column(column =>
                            {
                                column.Item().AlignRight().Text("Ship To:").Bold().FontSize(12);
                                column.Item().AlignRight().Text(order.Store?.Name ?? "");
                                column.Item().AlignRight().Text($"{order.Store?.Barangay}, {order.Store?.City}");
                            });
                        });

                        page.Content().PaddingVertical(20).Column(column =>
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);  // No.
                                    columns.RelativeColumn(4);   // Product
                                    columns.RelativeColumn(2);   // SKU
                                    columns.RelativeColumn(1);   // Qty
                                    columns.ConstantColumn(80);  // Checked
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#").Bold();
                                    header.Cell().Element(CellStyle).Text("Product").Bold();
                                    header.Cell().Element(CellStyle).Text("SKU").Bold();
                                    header.Cell().Element(CellStyle).Text("Qty").Bold();
                                    header.Cell().Element(CellStyle).Text("☐ Checked").Bold();
                                });

                                // Items
                                if (order.Items != null)
                                {
                                    int index = 1;
                                    foreach (var item in order.Items)
                                    {
                                        table.Cell().Element(CellStyle).Text(index.ToString());
                                        table.Cell().Element(CellStyle).Text(item.Product?.Name ?? "");
                                        table.Cell().Element(CellStyle).Text(item.Product?.Sku ?? "");
                                        table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                                        table.Cell().Element(CellStyle).Text("");
                                        index++;
                                    }
                                }
                            });

                            // Notes Section
                            column.Item().PaddingTop(30).Column(col =>
                            {
                                col.Item().Text("Special Instructions / Notes:").Bold();
                                col.Item().BorderBottom(1).PaddingVertical(30).Text("");
                            });

                            // Signature Section
                            column.Item().PaddingTop(30).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Packed By:").Bold();
                                    col.Item().PaddingTop(30).BorderBottom(1).Text("");
                                    col.Item().PaddingTop(5).Text("Signature & Date").FontSize(8);
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Received By:").Bold();
                                    col.Item().PaddingTop(30).BorderBottom(1).Text("");
                                    col.Item().PaddingTop(5).Text("Signature & Date").FontSize(8);
                                });
                            });
                        });

                        page.Footer().AlignCenter().Text("ASTRA Distribution System - Packing Slip");
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating packing slip PDF");
                throw;
            }
        }

        public byte[] GeneratePickListPdf(List<Order> orders)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        page.Header().Column(column =>
                        {
                            column.Item().Text("PICK LIST").FontSize(18).Bold();
                            column.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10);
                            column.Item().Text($"Total Orders: {orders.Count}").FontSize(10);
                        });

                        page.Content().PaddingVertical(15).Column(column =>
                        {
                            // Consolidated items across all orders
                            var consolidatedItems = orders
                                .SelectMany(o => o.Items)
                                .GroupBy(i => new { i.ProductId, i.Product.Name, i.Product.Sku })
                                .Select(g => new
                                {
                                    ProductName = g.Key.Name,
                                    Sku = g.Key.Sku,
                                    TotalQuantity = g.Sum(i => i.Quantity),
                                    OrderCount = g.Count()
                                })
                                .OrderBy(i => i.ProductName)
                                .ToList();

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);   // Product
                                    columns.RelativeColumn(2);   // SKU
                                    columns.RelativeColumn(1);   // Total Qty
                                    columns.RelativeColumn(1);   // Orders
                                    columns.ConstantColumn(60);  // Picked
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Product").Bold();
                                    header.Cell().Element(CellStyle).Text("SKU").Bold();
                                    header.Cell().Element(CellStyle).Text("Total Qty").Bold();
                                    header.Cell().Element(CellStyle).Text("Orders").Bold();
                                    header.Cell().Element(CellStyle).Text("☐ Picked").Bold();
                                });

                                // Items
                                foreach (var item in consolidatedItems)
                                {
                                    table.Cell().Element(CellStyle).Text(item.ProductName);
                                    table.Cell().Element(CellStyle).Text(item.Sku);
                                    table.Cell().Element(CellStyle).Text(item.TotalQuantity.ToString());
                                    table.Cell().Element(CellStyle).Text(item.OrderCount.ToString());
                                    table.Cell().Element(CellStyle).Text("");
                                }
                            });

                            // Order Details Section
                            column.Item().PaddingTop(20).Text("Order Details:").FontSize(11).Bold();

                            foreach (var order in orders)
                            {
                                column.Item().PaddingTop(10).Column(col =>
                                {
                                    col.Item().Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Order #{order.Id} - {order.Store?.Name}").FontSize(10).Bold();
                                        row.ConstantItem(100).AlignRight().Text($"Items: {order.Items?.Count ?? 0}");
                                    });

                                    if (order.Items != null)
                                    {
                                        foreach (var item in order.Items)
                                        {
                                            col.Item().PaddingLeft(10).Text($"• {item.Product?.Name} - Qty: {item.Quantity}");
                                        }
                                    }
                                });
                            }
                        });

                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pick list PDF");
                throw;
            }
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        }
    }
}
