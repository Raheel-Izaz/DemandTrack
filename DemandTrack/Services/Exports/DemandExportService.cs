using ClosedXML.Excel;
using DemandTrack.Data;
using DemandTrack.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;

namespace DemandTrack.Services.Exports
{
    public class DemandExportService : IDemandExportService
    {
        private readonly AppDbContext _context;

        public DemandExportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GeneratePdfAsync(int demandId, CancellationToken cancellationToken = default)
        {
            var demand = await GetDemandWithDetailsAsync(demandId, cancellationToken);

            var itemsList = demand.Items.OrderBy(i => i.Id).ToList();
            var totalQuantity = itemsList.Sum(i => i.RequestedQty);
            var supplierContact = !string.IsNullOrWhiteSpace(demand.Supplier.Contact)
                ? demand.Supplier.Contact
                : "N/A";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25, Unit.Point);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor(Colors.Grey.Darken3));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(titleCol =>
                            {
                                titleCol.Item().Text("PURCHASE DEMAND")
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.BlueGrey.Darken4);
                                titleCol.Item().Text(demand.DemandNumber)
                                    .FontSize(12)
                                    .SemiBold()
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });

                        col.Item().PaddingTop(15).Row(metaRow =>
                        {
                            // Left column: Supplier Name & Contact/Phone
                            metaRow.RelativeItem().Column(left =>
                            {
                                left.Item().Text(t =>
                                {
                                    t.Span("Supplier: ").Bold().FontColor(Colors.BlueGrey.Darken3);
                                    t.Span(demand.Supplier.Name);
                                });
                                left.Item().PaddingTop(4).Text(t =>
                                {
                                    t.Span("Phone: ").Bold().FontColor(Colors.BlueGrey.Darken3);
                                    t.Span(supplierContact);
                                });
                            });

                            // Right column: Date & Status
                            metaRow.RelativeItem().Column(right =>
                            {
                                right.Item().AlignRight().Text(t =>
                                {
                                    t.Span("Date: ").Bold().FontColor(Colors.BlueGrey.Darken3);
                                    t.Span(demand.CreatedAt.ToString("dd MMM yyyy"));
                                });
                                right.Item().PaddingTop(4).AlignRight().Text(t =>
                                {
                                    t.Span("Status: ").Bold().FontColor(Colors.BlueGrey.Darken3);
                                    t.Span(demand.Status.ToString());
                                });
                            });
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    // Content Table
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // S.No
                            columns.ConstantColumn(110); // SKU / Code
                            columns.RelativeColumn();   // Product Name
                            columns.ConstantColumn(120); // Demanded Quantity
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.BlueGrey.Darken4).PaddingVertical(6).PaddingHorizontal(4)
                                .AlignCenter().Text("S.No").Bold().FontColor(Colors.White).FontSize(9);

                            header.Cell().Background(Colors.BlueGrey.Darken4).PaddingVertical(6).PaddingHorizontal(6)
                                .AlignLeft().Text("SKU / Code").Bold().FontColor(Colors.White).FontSize(9);

                            header.Cell().Background(Colors.BlueGrey.Darken4).PaddingVertical(6).PaddingHorizontal(6)
                                .AlignLeft().Text("Product Name").Bold().FontColor(Colors.White).FontSize(9);

                            header.Cell().Background(Colors.BlueGrey.Darken4).PaddingVertical(6).PaddingHorizontal(6)
                                .AlignRight().Text("Demanded Quantity").Bold().FontColor(Colors.White).FontSize(9);
                        });

                        int serial = 1;
                        foreach (var item in itemsList)
                        {
                            var bg = serial % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                            table.Cell().Background(bg).PaddingVertical(6).PaddingHorizontal(4)
                                .AlignCenter().Text(serial.ToString()).FontSize(9);

                            table.Cell().Background(bg).PaddingVertical(6).PaddingHorizontal(6)
                                .AlignLeft().Text(item.Product?.Sku ?? "N/A").FontSize(9);

                            table.Cell().Background(bg).PaddingVertical(6).PaddingHorizontal(6)
                                .AlignLeft().Text(item.Product?.Name ?? string.Empty).FontSize(9);

                            table.Cell().Background(bg).PaddingVertical(6).PaddingHorizontal(6)
                                .AlignRight().Text(item.RequestedQty.ToString("N0")).FontSize(9);

                            serial++;
                        }

                        // Total summary row
                        table.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten3).PaddingVertical(6).PaddingHorizontal(6)
                            .AlignRight().Text("Total Quantity").Bold().FontSize(9);

                        table.Cell().Background(Colors.Grey.Lighten3).PaddingVertical(6).PaddingHorizontal(6)
                            .AlignRight().Text(totalQuantity.ToString("N0")).Bold().FontSize(9);
                    });

                    // Footer with Page X of Y
                    page.Footer().AlignCenter().PaddingTop(10).Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateExcelAsync(int demandId, CancellationToken cancellationToken = default)
        {
            var demand = await GetDemandWithDetailsAsync(demandId, cancellationToken);
            var itemsList = demand.Items.OrderBy(i => i.Id).ToList();

            using var workbook = new XLWorkbook();
            var safeSheetName = Regex.Replace(demand.DemandNumber, @"[\\/*?:\[\]]", "-");
            if (safeSheetName.Length > 31)
            {
                safeSheetName = safeSheetName.Substring(0, 31);
            }
            if (string.IsNullOrWhiteSpace(safeSheetName))
            {
                safeSheetName = "Demand";
            }

            var worksheet = workbook.Worksheets.Add(safeSheetName);
            var supplierContact = !string.IsNullOrWhiteSpace(demand.Supplier.Contact)
                ? demand.Supplier.Contact
                : "N/A";

            // Row 1: "Demand Number:", demand.DemandNumber | "Status:", demand.Status.ToString()
            worksheet.Cell("A1").Value = "Demand Number:";
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("B1").Value = demand.DemandNumber;
            worksheet.Cell("C1").Value = "Status:";
            worksheet.Cell("C1").Style.Font.Bold = true;
            worksheet.Cell("D1").Value = demand.Status.ToString();

            // Row 2: "Supplier:", demand.Supplier.Name | "Phone:", demand.Supplier.Phone ?? "N/A"
            worksheet.Cell("A2").Value = "Supplier:";
            worksheet.Cell("A2").Style.Font.Bold = true;
            worksheet.Cell("B2").Value = demand.Supplier.Name;
            worksheet.Cell("C2").Value = "Phone:";
            worksheet.Cell("C2").Style.Font.Bold = true;
            worksheet.Cell("D2").Value = supplierContact;

            // Row 3: "Creation Date:", demand.CreatedAt.ToString("dd MMM yyyy")
            worksheet.Cell("A3").Value = "Creation Date:";
            worksheet.Cell("A3").Style.Font.Bold = true;
            worksheet.Cell("B3").Value = demand.CreatedAt.ToString("dd MMM yyyy");

            // Row 4: Empty

            // Row 5: Table Header: S.No, SKU / Code, Product Name, Demanded Quantity (Dark slate fill #1E293B, bold white text)
            worksheet.Cell("A5").Value = "S.No";
            worksheet.Cell("B5").Value = "SKU / Code";
            worksheet.Cell("C5").Value = "Product Name";
            worksheet.Cell("D5").Value = "Demanded Quantity";

            var headerRange = worksheet.Range("A5:D5");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
            worksheet.Cell("A5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell("B5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell("C5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell("D5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            int currentRow = 6;
            int serialNumber = 1;

            foreach (var item in itemsList)
            {
                worksheet.Cell(currentRow, 1).Value = serialNumber;
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 2).Value = item.Product?.Sku ?? "N/A";
                worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Cell(currentRow, 3).Value = item.Product?.Name ?? string.Empty;
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Cell(currentRow, 4).Value = item.RequestedQty;
                worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                currentRow++;
                serialNumber++;
            }

            // Bottom Row: Col C "Total Quantity", Col D =SUM(D6:D{lastRow})
            int lastDataRow = currentRow - 1;
            worksheet.Cell(currentRow, 3).Value = "Total Quantity";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(currentRow, 4).FormulaA1 = $"=SUM(D6:D{lastDataRow})";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
            worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Range(currentRow, 1, currentRow, 4).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            worksheet.Range(currentRow, 1, currentRow, 4).Style.Border.BottomBorder = XLBorderStyleValues.Double;

            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return memoryStream.ToArray();
        }

        private async Task<Demand> GetDemandWithDetailsAsync(int demandId, CancellationToken cancellationToken)
        {
            var demand = await _context.Demands
                .AsNoTracking()
                .Include(d => d.Supplier)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(d => d.Id == demandId, cancellationToken);

            if (demand == null)
            {
                throw new KeyNotFoundException($"Demand with ID {demandId} was not found.");
            }

            if (demand.Items == null || !demand.Items.Any())
            {
                throw new InvalidOperationException($"Demand {demand.DemandNumber} contains zero items and cannot be exported.");
            }

            return demand;
        }
    }
}
