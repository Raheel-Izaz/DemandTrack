using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemandTrack.Data;
using DemandTrack.Dtos;
using DemandTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandTrack.Controllers
{
    [ApiController]
    [Route("api/suppliers")]
    public class SuppliersController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        // GET: api/suppliers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers([FromQuery] bool activeOnly = true)
        {
            var query = _context.Suppliers
                .Include(s => s.Demands)
                .Include(s => s.Invoices)
                .AsQueryable();

            if (activeOnly)
            {
                query = query.Where(s => s.IsActive);
            }

            var suppliers = await query
                .OrderBy(s => s.Name)
                .Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Contact = s.Contact,
                    IsActive = s.IsActive,
                    DemandsCount = s.Demands.Count,
                    InvoicesCount = s.Invoices.Count
                })
                .ToListAsync();

            return Ok(suppliers);
        }

        // GET: api/suppliers/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SupplierDto>> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Demands)
                .Include(s => s.Invoices)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound(new { message = $"Supplier with ID {id} not found." });
            }

            return Ok(new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Contact = supplier.Contact,
                IsActive = supplier.IsActive,
                DemandsCount = supplier.Demands.Count,
                InvoicesCount = supplier.Invoices.Count
            });
        }

        // POST: api/suppliers
        [HttpPost]
        public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] CreateSupplierDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Supplier name is required." });
            }

            var supplier = new Supplier
            {
                Name = dto.Name.Trim(),
                Contact = dto.Contact?.Trim(),
                IsActive = true
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Contact = supplier.Contact,
                IsActive = supplier.IsActive,
                DemandsCount = 0,
                InvoicesCount = 0
            });
        }

        // PUT: api/suppliers/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(new { message = $"Supplier with ID {id} not found." });
            }

            supplier.Name = dto.Name.Trim();
            supplier.Contact = dto.Contact?.Trim();
            supplier.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/suppliers/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Demands)
                .Include(s => s.Invoices)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound(new { message = $"Supplier with ID {id} not found." });
            }

            if (supplier.Demands.Any() || supplier.Invoices.Any())
            {
                supplier.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Supplier has linked demands or invoices, so it was marked as inactive instead of permanently deleted." });
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
