using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text.RegularExpressions;
using DemandTrack.Data;
using DemandTrack.Dtos;
using DemandTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// PDF and Excel libraries
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace DemandTrack.Controllers
{
    [ApiController]
    [Route("api/demands")]
    public class DemandsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DemandsController(AppDbContext context)
        {
            _context = context;
        }

        // Add this at the class level (preferably as private static readonly field)
        private static readonly string[] LineSeparators = new[] { "\r", "\n" };

        // GET: api/demands
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DemandResponseDto>>> GetDemands()
        {
            var demands = await _context.Demands
                .Include(d => d.Items)
                .ThenInclude(i => i.Book)
                .Select(d => new DemandResponseDto
                {
                    Id = d.Id,
                    SeasonName = d.SeasonName,
                    CreatedAt = d.CreatedAt,
                    TotalQty = d.Items.Sum(i => i.RequestedQty),
                    Items = d.Items.Select(i => new DemandItemResponseDto
                    {
                        BookTitle = i.Book!.Title,
                        Isbn = i.Book!.Isbn,
                        RequestedQty = i.RequestedQty
                    }).ToList()
                })
                .ToListAsync();

            return Ok(demands);
        }

        // GET: api/demands/filtered?status={status}
        [HttpGet("filtered")]
        public async Task<IActionResult> GetDemandsFiltered([FromQuery] string? status = null)
        {
            // Normalize status parameter
            var filterStatus = status?.ToLowerInvariant() switch
            {
                "pending" => "Pending",
                "partiallyshipped" => "PartiallyShipped",
                "completed" => "Completed",
                "all" => null,
                null => null,
                _ => null
            };

            // Load all demands with related data
            var demands = await _context.Demands
                .Include(d => d.Items)
                .ThenInclude(i => i.Book)
                .ToListAsync();

            if (demands.Count == 0)
                return Ok(new List<object>());

            // Get all demand item IDs
            var allDemandItemIds = demands
                .SelectMany(d => d.Items)
                .Select(i => i.Id)
                .ToList();

            // Load all supply items in one query and group by DemandItemId
            var supplyItemsGrouped = await _context.SupplyItems
                .Where(si => allDemandItemIds.Contains(si.DemandItemId))
                .GroupBy(si => si.DemandItemId)
                .Select(g => new
                {
                    DemandItemId = g.Key,
                    TotalShippedQty = g.Sum(si => si.ShippedQty)
                })
                .ToDictionaryAsync(x => x.DemandItemId, x => x.TotalShippedQty);

            // Build response with filtering
            var response = demands.Select(d =>
            {
                var filteredItems = d.Items
                    .Select(i =>
                    {
                        var shippedQty = supplyItemsGrouped.GetValueOrDefault(i.Id, 0);
                        string itemStatus;

                        if (shippedQty == 0)
                            itemStatus = "Pending";
                        else if (shippedQty < i.RequestedQty)
                            itemStatus = "PartiallyShipped";
                        else
                            itemStatus = "Completed";

                        return new
                        {
                            DemandItemId = i.Id,
                            BookTitle = i.Book?.Title ?? string.Empty,
                            Isbn = i.Book?.Isbn ?? string.Empty,
                            RequestedQty = i.RequestedQty,
                            ShippedQty = shippedQty,
                            Status = itemStatus
                        };
                    })
                    .Where(item =>
                    {
                        // Apply status filter
                        if (filterStatus == null)
                            return true;

                        return item.Status == filterStatus;
                    })
                    .ToList();

                return new
                {
                    d.Id,
                    d.SeasonName,
                    d.CreatedAt,
                    Items = filteredItems
                };
            })
            .Where(d => d.Items.Count > 0) // Exclude demands with no items after filtering
            .ToList();

            return Ok(response);
        }

        // GET: api/demands/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DemandResponseDto>> GetDemand(int id)
        {
            var demand = await _context.Demands
                .Where(d => d.Id == id)
                .Include(d => d.Items)
                .ThenInclude(i => i.Book)
                .Select(d => new DemandResponseDto
                {
                    Id = d.Id,
                    SeasonName = d.SeasonName,
                    CreatedAt = d.CreatedAt,
                    TotalQty = d.Items.Sum(i => i.RequestedQty),
                    Items = d.Items.Select(i => new DemandItemResponseDto
                    {
                        BookTitle = i.Book!.Title,
                        Isbn = i.Book!.Isbn,
                        RequestedQty = i.RequestedQty
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (demand == null)
                return NotFound();

            return Ok(demand);
        }

        // POST: api/demands
        [HttpPost]
        public async Task<ActionResult<DemandResponseDto>> CreateDemand([FromBody] CreateDemandDto dto)
        {
            if (dto == null)
                return BadRequest("Payload required.");

            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest("At least one item is required.");

            // Quantity > 0
            if (dto.Items.Any(i => i.RequestedQty <= 0))
                return BadRequest("Requested quantity must be greater than zero.");

            // No duplicate books
            var bookIds = dto.Items.Select(i => i.BookId).ToList();
            if (bookIds.Distinct().Count() != bookIds.Count)
                return BadRequest("Duplicate books are not allowed in a single demand.");

            // Validate books exist
            var existingBookIds = await _context.Books
                .Where(b => bookIds.Contains(b.Id))
                .Select(b => b.Id)
                .ToListAsync();

            var missing = bookIds.Except(existingBookIds).ToList();
            if (missing.Count > 0)
                return BadRequest("One or more books do not exist.");

            var demand = new Demand
            {
                SeasonName = dto.SeasonName,
                Items = dto.Items.Select(i => new DemandItem
                {
                    BookId = i.BookId,
                    RequestedQty = i.RequestedQty
                }).ToList()
            };

            _context.Demands.Add(demand);
            await _context.SaveChangesAsync();

            // Load with related to map response
            var created = await _context.Demands
                .Where(d => d.Id == demand.Id)
                .Include(d => d.Items)
                .ThenInclude(i => i.Book)
                .Select(d => new DemandResponseDto
                {
                    Id = d.Id,
                    SeasonName = d.SeasonName,
                    CreatedAt = d.CreatedAt,
                    TotalQty = d.Items.Sum(i => i.RequestedQty),
                    Items = d.Items.Select(i => new DemandItemResponseDto
                    {
                        BookTitle = i.Book!.Title,
                        Isbn = i.Book!.Isbn,
                        RequestedQty = i.RequestedQty
                    }).ToList()
                })
                .FirstAsync();

            return CreatedAtAction(nameof(GetDemand), new { id = created.Id }, created);
        }

        // DELETE: api/demands/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDemand(int id)
        {
            var demand = await _context.Demands.FindAsync(id);
            if (demand == null)
                return NotFound();

            _context.Demands.Remove(demand);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // BOOKS CRUD
        // GET: api/demands/books
        [HttpGet("books")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            var books = await _context.Books.ToListAsync();
            return Ok(books);
        }

        // POST: api/demands/books
        [HttpPost("books")]
        public async Task<ActionResult<Book>> CreateBook([FromBody] Book book)
        {
            if (book == null)
                return BadRequest();

            book.Id = 0;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return Created($"/api/demands/books/{book.Id}", book);
        }

        // PUT: api/demands/books/{id}
        [HttpPut("books/{id:int}")]
        public async Task<ActionResult<Book>> UpdateBook(int id, [FromBody] Book book)
        {
            var existing = await _context.Books.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Isbn = book.Isbn;
            existing.Title = book.Title;
            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/demands/books/{id}
        [HttpDelete("books/{id:int}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var existing = await _context.Books.FindAsync(id);
            if (existing == null)
                return NotFound();

            _context.Books.Remove(existing);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DEMAND ITEMS management via Demand
        // POST: api/demands/{demandId}/items
        [HttpPost("{demandId:int}/items")]
        public async Task<ActionResult<DemandItem>> AddDemandItem(int demandId, [FromBody] CreateDemandItemDto item)
        {
            if (item == null)
                return BadRequest();

            var demandExists = await _context.Demands.AnyAsync(d => d.Id == demandId);
            if (!demandExists)
                return NotFound();

            var newItem = new DemandItem
            {
                DemandId = demandId,
                BookId = item.BookId,
                RequestedQty = item.RequestedQty
            };

            _context.DemandItems.Add(newItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDemand), new { id = demandId }, newItem);
        }

        // PUT: api/demands/{demandId}/items/{itemId}
        [HttpPut("{demandId:int}/items/{itemId:int}")]
        public async Task<ActionResult<DemandItem>> UpdateDemandItemQuantity(int demandId, int itemId, [FromBody] CreateDemandItemDto item)
        {
            var existing = await _context.DemandItems.FirstOrDefaultAsync(di => di.Id == itemId && di.DemandId == demandId);
            if (existing == null)
                return NotFound();

            existing.RequestedQty = item.RequestedQty;
            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // GET: api/demands/{id}/export/pdf
        [HttpGet("{id:int}/export/pdf")]
        public async Task<IActionResult> ExportDemandPdf(int id)
        {
            var demand = await _context.Demands
                .Where(d => d.Id == id)
                .Include(d => d.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync();

            if (demand == null)
                return NotFound();

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Column(col =>
                    {
                        col.Item().Text($"Season: {demand.SeasonName}").SemiBold();
                        col.Item().Text($"Created At: {demand.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(200);
                            columns.ConstantColumn(160);
                            columns.ConstantColumn(140);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Book Title");
                            header.Cell().Element(CellStyle).Text("ISBN");
                            header.Cell().Element(CellStyle).Text("Requested Qty");
                        });

                        foreach (var item in demand.Items)
                        {
                            table.Cell().Element(CellStyle).Text(item.Book?.Title ?? string.Empty);
                            table.Cell().Element(CellStyle).Text(item.Book?.Isbn ?? string.Empty);
                            table.Cell().Element(CellStyle).Text(item.RequestedQty.ToString());
                        }

                        IContainer CellStyle(IContainer container)
                        {
                            return container.Padding(5).Border(1).BorderColor(Colors.Grey.Lighten2);
                        }
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;
            var fileName = $"demand_{demand.Id}.pdf";
            return File(stream.ToArray(), "application/pdf", fileName);
        }

        // GET: api/demands/{id}/export/excel
        [HttpGet("{id:int}/export/excel")]
        public async Task<IActionResult> ExportDemandExcel(int id)
        {
            var demand = await _context.Demands
                .Where(d => d.Id == id)
                .Include(d => d.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync();

            if (demand == null)
                return NotFound();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add($"Demand_{demand.Id}");

            // Header info
            ws.Cell(1, 1).Value = "Season"; ws.Cell(1, 2).Value = demand.SeasonName;
            ws.Cell(2, 1).Value = "CreatedAt"; ws.Cell(2, 2).Value = demand.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";

            // Table headers
            ws.Cell(4, 1).Value = "Book Title";
            ws.Cell(4, 2).Value = "ISBN";
            ws.Cell(4, 3).Value = "Requested Qty";

            var row = 5;
            foreach (var item in demand.Items)
            {
                ws.Cell(row, 1).Value = item.Book?.Title ?? string.Empty;
                ws.Cell(row, 2).Value = item.Book?.Isbn ?? string.Empty;
                ws.Cell(row, 3).Value = item.RequestedQty;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            var fileName = $"demand_{demand.Id}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // POST: api/demands/{demandId}/upload-transcript
        [HttpPost("{demandId:int}/upload-transcript")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadTranscript(int demandId, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");
            
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || file.ContentType != "application/pdf")
                return BadRequest("Only PDF files are supported.");

            var demand = await _context.Demands
                .Where(d => d.Id == demandId)
                .Include(d => d.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync();

            if (demand == null)
                return NotFound();

            // Check if this file has already been uploaded
            var existingUpload = await _context.SupplyUploads
                .Where(su => su.DemandId == demandId && su.FileName == file.FileName)
                .FirstOrDefaultAsync();

            if (existingUpload != null)
                return BadRequest($"File '{file.FileName}' has already been uploaded for this demand.");

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            var pairs = ExtractIsbnQtyPairs(bytes);

            if (pairs.Count == 0)
                return BadRequest("No valid ISBN and quantity pairs found in the PDF.");

            // Create SupplyUpload record
            var supplyUpload = new SupplyUpload
            {
                DemandId = demandId,
                FileName = file.FileName,
                UploadedAt = DateTime.UtcNow,
                Items = new List<SupplyItem>()
            };

            // Map ISBN to DemandItem
            var isbnToItem = demand.Items
                .Where(i => i.Book != null && !string.IsNullOrWhiteSpace(i.Book.Isbn))
                .ToDictionary(i => NormalizeIsbn(i.Book!.Isbn), i => i);

            // Create SupplyItem for each matched ISBN
            foreach (var (isbn, qty) in pairs)
            {
                var key = NormalizeIsbn(isbn);
                if (isbnToItem.TryGetValue(key, out var demandItem))
                {
                    var supplyItem = new SupplyItem
                    {
                        SupplyUpload = supplyUpload,
                        DemandItemId = demandItem.Id,
                        Isbn = isbn,
                        ShippedQty = qty
                    };
                    supplyUpload.Items.Add(supplyItem);
                }
            }

            if (supplyUpload.Items.Count == 0)
                return BadRequest("No matching ISBNs found in the PDF for this demand.");

            _context.SupplyUploads.Add(supplyUpload);
            await _context.SaveChangesAsync();

            // Load all supply items for this demand to calculate total shipped qty
            var demandItemIds = demand.Items.Select(i => i.Id).ToList();
            var allSupplyItems = await _context.SupplyItems
                .Where(si => demandItemIds.Contains(si.DemandItemId))
                .ToListAsync();

            // Group supply items by DemandItemId for efficient lookup
            var supplyItemsByDemandItem = allSupplyItems
                .GroupBy(si => si.DemandItemId)
                .ToDictionary(g => g.Key, g => g.Sum(si => si.ShippedQty));

            var response = new
            {
                demand.Id,
                demand.SeasonName,
                demand.CreatedAt,
                UploadId = supplyUpload.Id,
                UploadedAt = supplyUpload.UploadedAt,
                Items = demand.Items.Select(i =>
                {
                    var shippedQty = supplyItemsByDemandItem.GetValueOrDefault(i.Id, 0);
                    string status;
                    
                    if (shippedQty == 0)
                        status = "Pending";
                    else if (shippedQty < i.RequestedQty)
                        status = "Partially Shipped";
                    else
                        status = "Completed";

                    return new
                    {
                        BookTitle = i.Book?.Title ?? string.Empty,
                        Isbn = i.Book?.Isbn ?? string.Empty,
                        RequestedQty = i.RequestedQty,
                        ShippedQty = shippedQty,
                        Status = status
                    };
                }).ToList()
            };

            return Ok(response);
        }

        // GET: api/demands/{demandId}/supply-uploads
        [HttpGet("{demandId:int}/supply-uploads")]
        public async Task<IActionResult> GetSupplyUploads(int demandId)
        {
            var demandExists = await _context.Demands.AnyAsync(d => d.Id == demandId);
            if (!demandExists)
                return NotFound();

            var uploads = await _context.SupplyUploads
                .Where(su => su.DemandId == demandId)
                .Include(su => su.Items)
                .ThenInclude(si => si.DemandItem)
                .ThenInclude(di => di!.Book)
                .OrderByDescending(su => su.UploadedAt)
                .Select(su => new
                {
                    su.Id,
                    su.FileName,
                    su.UploadedAt,
                    Items = su.Items.Select(si => new
                    {
                        BookTitle = si.DemandItem!.Book!.Title,
                        Isbn = si.Isbn,
                        ShippedQty = si.ShippedQty
                    }).ToList()
                })
                .ToListAsync();

            return Ok(uploads);
        }

        private static List<(string isbn, int qty)> ExtractIsbnQtyPairs(byte[] pdfBytes)
        {
            var result = new List<(string, int)>();
            using var reader = new PdfReader(new MemoryStream(pdfBytes));
            using var pdf = new PdfDocument(reader);

            for (int p = 1; p <= pdf.GetNumberOfPages(); p++)
            {
                var page = pdf.GetPage(p);
                var text = PdfTextExtractor.GetTextFromPage(page);
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var lines = text.Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var raw in lines)
                {
                    var line = raw.Trim();

                    // Match ISBN: 10 or 13 digits (X allowed at the end)
                    var isbnMatch = Regex.Match(line, @"\b(?<isbn>\d{9}[\dXx]|\d{13})\b");

                    // Match quantity: the last number in the line
                    var qtyMatch = Regex.Match(line, @"\b(?<qty>\d{1,6})\b$");

                    if (isbnMatch.Success && qtyMatch.Success)
                    {
                        var isbnRaw = isbnMatch.Groups["isbn"].Value;
                        var isbnNorm = NormalizeIsbn(isbnRaw);
                        if (isbnNorm.Length == 10 || isbnNorm.Length == 13)
                        {
                            if (int.TryParse(qtyMatch.Groups["qty"].Value, out var qty))
                            {
                                result.Add((isbnNorm, qty));
                            }
                        }
                    }
                }
            }

            return result;
        }


        private static string NormalizeIsbn(string isbn)
        {
            return new string(isbn.Where(ch => char.IsDigit(ch) || ch == 'X' || ch == 'x').ToArray()).ToUpperInvariant();
        }
    }
}
