using System.Text.Json;
using ClosedXML.Excel;
using DemandTrack.Data;
using DemandTrack.Dtos;
using DemandTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandTrack.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> GetProducts([FromQuery] ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Grade)
                .Include(p => p.Inventory)
                .AsQueryable();

            if (filter.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filter.IsActive.Value);
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            if (filter.GradeId.HasValue)
            {
                query = query.Where(p => p.GradeId == filter.GradeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(s) ||
                    (p.Sku != null && p.Sku.ToLower().Contains(s)) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(s)) ||
                    (p.Description != null && p.Description.ToLower().Contains(s))
                );
            }

            var totalCount = await query.CountAsync();
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : (filter.PageSize > 200 ? 200 : filter.PageSize);

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = products.Select(MapToDto).ToList();

            return Ok(new PagedResultDto<ProductDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = items
            });
        }

        // GET: api/products/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Grade)
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            return Ok(MapToDto(product));
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Product name is required." });
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = $"Category with ID {dto.CategoryId} does not exist." });
            }

            if (dto.GradeId.HasValue)
            {
                var gradeExists = await _context.Grades.AnyAsync(g => g.Id == dto.GradeId.Value);
                if (!gradeExists)
                {
                    return BadRequest(new { message = $"Grade with ID {dto.GradeId.Value} does not exist." });
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Sku))
            {
                var skuExists = await _context.Products.AnyAsync(p => p.Sku == dto.Sku.Trim());
                if (skuExists)
                {
                    return BadRequest(new { message = $"A product with SKU '{dto.Sku}' already exists." });
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Barcode))
            {
                var barcodeExists = await _context.Products.AnyAsync(p => p.Barcode == dto.Barcode.Trim());
                if (barcodeExists)
                {
                    return BadRequest(new { message = $"A product with Barcode '{dto.Barcode}' already exists." });
                }
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Sku = dto.Sku?.Trim(),
                Barcode = dto.Barcode?.Trim(),
                Description = dto.Description?.Trim(),
                UnitOfMeasure = string.IsNullOrWhiteSpace(dto.UnitOfMeasure) ? "Piece" : dto.UnitOfMeasure.Trim(),
                DefaultUnitPrice = dto.DefaultUnitPrice,
                CostPrice = dto.CostPrice,
                CategoryId = dto.CategoryId,
                GradeId = dto.GradeId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Attributes = SerializeAttributes(dto.Attributes)
            };

            // Initialize 1:1 inventory record
            product.Inventory = new ProductInventory
            {
                QuantityOnHand = dto.InitialQuantityOnHand,
                QuantityReserved = 0,
                UpdatedAt = DateTime.UtcNow
            };

            if (dto.InitialQuantityOnHand > 0)
            {
                product.InventoryTransactions.Add(new InventoryTransaction
                {
                    QuantityDelta = dto.InitialQuantityOnHand,
                    TransactionType = InventoryTransactionType.PurchaseReceipt,
                    ReferenceId = "Initial Stock",
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Reload relationships for DTO response
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
            if (product.GradeId.HasValue)
            {
                await _context.Entry(product).Reference(p => p.Grade).LoadAsync();
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, MapToDto(product));
        }

        // PUT: api/products/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = $"Category with ID {dto.CategoryId} does not exist." });
            }

            if (dto.GradeId.HasValue)
            {
                var gradeExists = await _context.Grades.AnyAsync(g => g.Id == dto.GradeId.Value);
                if (!gradeExists)
                {
                    return BadRequest(new { message = $"Grade with ID {dto.GradeId.Value} does not exist." });
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Sku))
            {
                var skuExists = await _context.Products.AnyAsync(p => p.Id != id && p.Sku == dto.Sku.Trim());
                if (skuExists)
                {
                    return BadRequest(new { message = $"Another product with SKU '{dto.Sku}' already exists." });
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Barcode))
            {
                var barcodeExists = await _context.Products.AnyAsync(p => p.Id != id && p.Barcode == dto.Barcode.Trim());
                if (barcodeExists)
                {
                    return BadRequest(new { message = $"Another product with Barcode '{dto.Barcode}' already exists." });
                }
            }

            product.Name = dto.Name.Trim();
            product.Sku = dto.Sku?.Trim();
            product.Barcode = dto.Barcode?.Trim();
            product.Description = dto.Description?.Trim();
            product.UnitOfMeasure = string.IsNullOrWhiteSpace(dto.UnitOfMeasure) ? "Piece" : dto.UnitOfMeasure.Trim();
            product.DefaultUnitPrice = dto.DefaultUnitPrice;
            product.CostPrice = dto.CostPrice;
            product.CategoryId = dto.CategoryId;
            product.GradeId = dto.GradeId;
            product.IsActive = dto.IsActive;
            product.UpdatedAt = DateTime.UtcNow;
            product.Attributes = SerializeAttributes(dto.Attributes);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.DemandItems)
                .Include(p => p.Inventory)
                .Include(p => p.InventoryTransactions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            if (product.DemandItems.Any())
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Product has associated demand items, so it was marked as inactive instead of permanently deleted." });
            }

            _context.InventoryTransactions.RemoveRange(product.InventoryTransactions);
            if (product.Inventory != null)
            {
                _context.ProductInventories.Remove(product.Inventory);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/products/{id}/adjust-inventory
        [HttpPost("{id:int}/adjust-inventory")]
        public async Task<ActionResult<ProductDto>> AdjustInventory(int id, [FromBody] InventoryAdjustmentDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Grade)
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            if (product.Inventory == null)
            {
                product.Inventory = new ProductInventory
                {
                    ProductId = product.Id,
                    QuantityOnHand = 0,
                    QuantityReserved = 0,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            product.Inventory.QuantityOnHand += dto.QuantityDelta;
            product.Inventory.UpdatedAt = DateTime.UtcNow;

            var transaction = new InventoryTransaction
            {
                ProductId = product.Id,
                QuantityDelta = dto.QuantityDelta,
                TransactionType = InventoryTransactionType.Adjustment,
                ReferenceId = dto.Reason ?? "Manual Adjustment",
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(MapToDto(product));
        }

        // GET: api/products/{id}/transactions
        [HttpGet("{id:int}/transactions")]
        public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> GetTransactions(int id)
        {
            var exists = await _context.Products.AnyAsync(p => p.Id == id);
            if (!exists)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            var transactions = await _context.InventoryTransactions
                .Where(t => t.ProductId == id)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new InventoryTransactionDto
                {
                    Id = t.Id,
                    ProductId = t.ProductId,
                    QuantityDelta = t.QuantityDelta,
                    TransactionType = t.TransactionType.ToString(),
                    ReferenceId = t.ReferenceId,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // GET: api/products/grades
        [HttpGet("grades")]
        public async Task<ActionResult<IEnumerable<GradeDto>>> GetGrades()
        {
            var grades = await _context.Grades
                .Where(g => g.IsActive)
                .OrderBy(g => g.DisplayOrder)
                .ThenBy(g => g.Name)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Code = g.Code,
                    DisplayOrder = g.DisplayOrder,
                    IsActive = g.IsActive
                })
                .ToListAsync();

            return Ok(grades);
        }

        // POST: api/products/grades
        [HttpPost("grades")]
        public async Task<ActionResult<GradeDto>> CreateGrade([FromBody] CreateGradeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Grade name is required." });
            }

            var grade = new Grade
            {
                Name = dto.Name.Trim(),
                Code = dto.Code?.Trim(),
                DisplayOrder = dto.DisplayOrder,
                IsActive = true
            };

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            return Ok(new GradeDto
            {
                Id = grade.Id,
                Name = grade.Name,
                Code = grade.Code,
                DisplayOrder = grade.DisplayOrder,
                IsActive = grade.IsActive
            });
        }

        // PUT: api/products/grades/{id}
        [HttpPut("grades/{id:int}")]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] GradeDto dto)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                return NotFound(new { message = $"Grade with ID {id} not found." });
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Grade name is required." });
            }

            grade.Name = dto.Name.Trim();
            grade.Code = dto.Code?.Trim();
            grade.DisplayOrder = dto.DisplayOrder;
            grade.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/products/grades/{id}
        [HttpDelete("grades/{id:int}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                return NotFound(new { message = $"Grade with ID {id} not found." });
            }

            var hasProducts = await _context.Products.AnyAsync(p => p.GradeId == id);
            if (hasProducts)
            {
                grade.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Grade has associated products, so it was marked as inactive instead of permanently deleted." });
            }

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/products/export-excel
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Grade)
                .Include(p => p.Inventory)
                .AsQueryable();

            if (filter.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filter.IsActive.Value);
            }
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }
            if (filter.GradeId.HasValue)
            {
                query = query.Where(p => p.GradeId == filter.GradeId.Value);
            }

            var products = await query.OrderBy(p => p.Name).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Products");

            // Header Row
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "SKU";
            worksheet.Cell(1, 4).Value = "Barcode";
            worksheet.Cell(1, 5).Value = "Category";
            worksheet.Cell(1, 6).Value = "Grade";
            worksheet.Cell(1, 7).Value = "UnitOfMeasure";
            worksheet.Cell(1, 8).Value = "DefaultUnitPrice";
            worksheet.Cell(1, 9).Value = "CostPrice";
            worksheet.Cell(1, 10).Value = "QuantityOnHand";
            worksheet.Cell(1, 11).Value = "QuantityReserved";
            worksheet.Cell(1, 12).Value = "IsActive";
            worksheet.Cell(1, 13).Value = "AttributesJSON";

            var row = 2;
            foreach (var p in products)
            {
                worksheet.Cell(row, 1).Value = p.Id;
                worksheet.Cell(row, 2).Value = p.Name;
                worksheet.Cell(row, 3).Value = p.Sku ?? string.Empty;
                worksheet.Cell(row, 4).Value = p.Barcode ?? string.Empty;
                worksheet.Cell(row, 5).Value = p.Category?.Name ?? string.Empty;
                worksheet.Cell(row, 6).Value = p.Grade?.Name ?? string.Empty;
                worksheet.Cell(row, 7).Value = p.UnitOfMeasure;
                worksheet.Cell(row, 8).Value = p.DefaultUnitPrice;
                worksheet.Cell(row, 9).Value = p.CostPrice;
                worksheet.Cell(row, 10).Value = p.Inventory?.QuantityOnHand ?? 0;
                worksheet.Cell(row, 11).Value = p.Inventory?.QuantityReserved ?? 0;
                worksheet.Cell(row, 12).Value = p.IsActive ? "Yes" : "No";
                worksheet.Cell(row, 13).Value = p.Attributes != null ? p.Attributes.RootElement.GetRawText() : string.Empty;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Products_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx"
            );
        }

        // POST: api/products/import-excel
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Please upload an Excel file." });
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                return BadRequest(new { message = "Uploaded Excel workbook contains no worksheets." });
            }

            var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1); // skip header
            if (rows == null)
            {
                return BadRequest(new { message = "No data rows found in the sheet." });
            }

            var categories = await _context.Categories.ToDictionaryAsync(c => c.Name.ToLower(), c => c.Id);
            var grades = await _context.Grades.ToDictionaryAsync(g => g.Name.ToLower(), g => g.Id);

            var createdCount = 0;
            var updatedCount = 0;

            foreach (var r in rows)
            {
                var name = r.Cell(2).GetString().Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                var sku = r.Cell(3).GetString().Trim();
                var barcode = r.Cell(4).GetString().Trim();
                var categoryName = r.Cell(5).GetString().Trim().ToLower();
                var gradeName = r.Cell(6).GetString().Trim().ToLower();
                var uom = r.Cell(7).GetString().Trim();
                var defaultUnitPrice = r.Cell(8).TryGetValue<decimal>(out var price) ? price : 0m;
                var costPrice = r.Cell(9).TryGetValue<decimal>(out var cost) ? cost : 0m;
                var qtyOnHand = r.Cell(10).TryGetValue<int>(out var qty) ? qty : 0;
                var rawAttributes = r.Cell(13).GetString().Trim();

                int categoryId;
                if (!string.IsNullOrEmpty(categoryName) && categories.TryGetValue(categoryName, out var catId))
                {
                    categoryId = catId;
                }
                else
                {
                    var firstCat = await _context.Categories.FirstOrDefaultAsync();
                    if (firstCat == null)
                    {
                        var defaultCat = new Category { Name = "General", IsActive = true };
                        _context.Categories.Add(defaultCat);
                        await _context.SaveChangesAsync();
                        categoryId = defaultCat.Id;
                        categories[defaultCat.Name.ToLower()] = defaultCat.Id;
                    }
                    else
                    {
                        categoryId = firstCat.Id;
                    }
                }

                int? gradeId = null;
                if (!string.IsNullOrEmpty(gradeName) && grades.TryGetValue(gradeName, out var gId))
                {
                    gradeId = gId;
                }

                JsonDocument? attributesDoc = null;
                if (!string.IsNullOrWhiteSpace(rawAttributes))
                {
                    try
                    {
                        attributesDoc = JsonDocument.Parse(rawAttributes);
                    }
                    catch
                    {
                        // ignore malformed attributes in excel import
                    }
                }

                Product? product = null;
                if (!string.IsNullOrEmpty(sku))
                {
                    product = await _context.Products.Include(p => p.Inventory).FirstOrDefaultAsync(p => p.Sku == sku);
                }

                if (product != null)
                {
                    product.Name = name;
                    product.Barcode = string.IsNullOrEmpty(barcode) ? product.Barcode : barcode;
                    product.CategoryId = categoryId;
                    product.GradeId = gradeId ?? product.GradeId;
                    product.UnitOfMeasure = string.IsNullOrEmpty(uom) ? product.UnitOfMeasure : uom;
                    product.DefaultUnitPrice = defaultUnitPrice;
                    product.CostPrice = costPrice;
                    product.UpdatedAt = DateTime.UtcNow;
                    if (attributesDoc != null) product.Attributes = attributesDoc;

                    if (product.Inventory == null)
                    {
                        product.Inventory = new ProductInventory
                        {
                            ProductId = product.Id,
                            QuantityOnHand = qtyOnHand,
                            QuantityReserved = 0,
                            UpdatedAt = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        product.Inventory.QuantityOnHand = qtyOnHand;
                        product.Inventory.UpdatedAt = DateTime.UtcNow;
                    }

                    updatedCount++;
                }
                else
                {
                    product = new Product
                    {
                        Name = name,
                        Sku = string.IsNullOrEmpty(sku) ? null : sku,
                        Barcode = string.IsNullOrEmpty(barcode) ? null : barcode,
                        CategoryId = categoryId,
                        GradeId = gradeId,
                        UnitOfMeasure = string.IsNullOrEmpty(uom) ? "Piece" : uom,
                        DefaultUnitPrice = defaultUnitPrice,
                        CostPrice = costPrice,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        Attributes = attributesDoc,
                        Inventory = new ProductInventory
                        {
                            QuantityOnHand = qtyOnHand,
                            QuantityReserved = 0,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    _context.Products.Add(product);
                    createdCount++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Import completed successfully.",
                created = createdCount,
                updated = updatedCount
            });
        }

        private static ProductDto MapToDto(Product p)
        {
            Dictionary<string, object?>? attributesDict = null;
            if (p.Attributes != null)
            {
                try
                {
                    attributesDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(p.Attributes.RootElement.GetRawText());
                }
                catch
                {
                    // Fallback to empty if not a key-value object
                }
            }

            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Barcode = p.Barcode,
                Description = p.Description,
                UnitOfMeasure = p.UnitOfMeasure,
                DefaultUnitPrice = p.DefaultUnitPrice,
                CostPrice = p.CostPrice,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                GradeId = p.GradeId,
                GradeName = p.Grade?.Name,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Attributes = attributesDict,
                QuantityOnHand = p.Inventory?.QuantityOnHand ?? 0,
                QuantityReserved = p.Inventory?.QuantityReserved ?? 0
            };
        }

        private static JsonDocument? SerializeAttributes(Dictionary<string, object?>? dict)
        {
            if (dict == null || dict.Count == 0) return null;
            var json = JsonSerializer.Serialize(dict);
            return JsonDocument.Parse(json);
        }
    }
}
