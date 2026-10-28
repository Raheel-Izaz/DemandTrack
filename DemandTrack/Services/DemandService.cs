using DemandTrack.Data;
using DemandTrack.Dtos;
using DemandTrack.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DemandTrack.Services
{
    public class DemandService : IDemandService
    {
        private readonly AppDbContext _context;

        public DemandService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<DemandListDto>> GetDemandsAsync(
            int? seasonId,
            DemandStatus? status,
            int? supplierId,
            string? search,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Demands
                .AsNoTracking()
                .Include(d => d.Season)
                .Include(d => d.Supplier)
                .Include(d => d.Items)
                .AsQueryable();

            if (seasonId.HasValue)
            {
                query = query.Where(d => d.SeasonId == seasonId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            if (supplierId.HasValue)
            {
                query = query.Where(d => d.SupplierId == supplierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(d =>
                    EF.Functions.ILike(d.DemandNumber, $"%{term}%") ||
                    EF.Functions.ILike(d.Title, $"%{term}%") ||
                    EF.Functions.ILike(d.Supplier.Name, $"%{term}%"));
            }

            var totalCount = await query.CountAsync();

            var demands = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DemandListDto
                {
                    Id = d.Id,
                    DemandNumber = d.DemandNumber,
                    Title = d.Title,
                    SeasonId = d.SeasonId,
                    SeasonName = d.Season.Name,
                    SupplierId = d.SupplierId,
                    SupplierName = d.Supplier.Name,
                    Status = d.Status,
                    TotalRequestedQty = d.Items.Sum(i => i.RequestedQty),
                    TotalSuppliedQty = d.Items.Sum(i => i.SuppliedQty),
                    TotalAmount = d.Items.Sum(i => i.RequestedQty * i.UnitPrice),
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<DemandListDto>
            {
                Items = demands,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<DemandDetailDto?> GetDemandByIdAsync(int id)
        {
            var demand = await _context.Demands
                .AsNoTracking()
                .Include(d => d.Season)
                .Include(d => d.Supplier)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (demand == null) return null;

            return MapToDetailDto(demand);
        }

        public async Task<DemandDetailDto> CreateDemandAsync(CreateDemandRequest request)
        {
            var season = await _context.Seasons.FindAsync(request.SeasonId);
            if (season == null)
            {
                throw new ArgumentException($"Season with ID {request.SeasonId} does not exist.");
            }

            var supplier = await _context.Suppliers.FindAsync(request.SupplierId);
            if (supplier == null)
            {
                throw new ArgumentException($"Supplier with ID {request.SupplierId} does not exist.");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                throw new ArgumentException("A demand must contain at least one item.");
            }

            // Validate products
            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            var existingProductIds = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var missingIds = productIds.Except(existingProductIds).ToList();
            if (missingIds.Count > 0)
            {
                throw new ArgumentException($"Product ID(s) {string.Join(", ", missingIds)} do not exist.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            // Extract 4-digit year from season name (e.g. "2026-2027") or fall back to current UTC year
            int year = DateTime.UtcNow.Year;
            var match = Regex.Match(season.Name, @"\b(20\d\d)\b");
            if (match.Success && int.TryParse(match.Value, out int parsedYear))
            {
                year = parsedYear;
            }

            string prefix = $"DEM-{year}-";

            // Find last sequential number for this prefix
            var lastDemand = await _context.Demands
                .Where(d => d.DemandNumber.StartsWith(prefix))
                .OrderByDescending(d => d.DemandNumber)
                .Select(d => d.DemandNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (lastDemand != null && lastDemand.Length >= prefix.Length + 4)
            {
                var seqPart = lastDemand.Substring(prefix.Length);
                if (int.TryParse(seqPart, out int parsedSeq))
                {
                    nextSeq = parsedSeq + 1;
                }
            }

            string demandNumber = $"{prefix}{nextSeq:D4}";

            var demand = new Demand
            {
                DemandNumber = demandNumber,
                Title = request.Title.Trim(),
                SeasonId = request.SeasonId,
                SupplierId = request.SupplierId,
                Status = request.Status, // Supports Draft or Open
                Notes = request.Notes?.Trim(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null
            };

            foreach (var itemReq in request.Items)
            {
                int suppliedQty = 0;
                int remainingQty = itemReq.RequestedQty - suppliedQty;

                demand.Items.Add(new DemandItem
                {
                    ProductId = itemReq.ProductId,
                    RequestedQty = itemReq.RequestedQty,
                    SuppliedQty = suppliedQty,
                    RemainingQty = remainingQty,
                    UnitPrice = itemReq.UnitPrice,
                    Status = DemandItemStatus.Pending
                });
            }

            _context.Demands.Add(demand);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetDemandByIdAsync(demand.Id))!;
        }

        public async Task<DemandDetailDto> UpdateDemandAsync(int id, UpdateDemandRequest request)
        {
            var demand = await _context.Demands
                .Include(d => d.Items)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (demand == null)
            {
                throw new KeyNotFoundException($"Demand with ID {id} not found.");
            }

            // Disallow editing items if demand is already in PartiallyFulfilled or Fulfilled status
            if (demand.Status == DemandStatus.PartiallyFulfilled || demand.Status == DemandStatus.Fulfilled)
            {
                throw new InvalidOperationException($"Cannot edit demand line items when status is {demand.Status}.");
            }

            var supplier = await _context.Suppliers.FindAsync(request.SupplierId);
            if (supplier == null)
            {
                throw new ArgumentException($"Supplier with ID {request.SupplierId} does not exist.");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                throw new ArgumentException("A demand must contain at least one item.");
            }

            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            var existingProductIds = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var missingIds = productIds.Except(existingProductIds).ToList();
            if (missingIds.Count > 0)
            {
                throw new ArgumentException($"Product ID(s) {string.Join(", ", missingIds)} do not exist.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            demand.Title = request.Title.Trim();
            demand.SupplierId = request.SupplierId;
            demand.Notes = request.Notes?.Trim();
            demand.UpdatedAt = DateTimeOffset.UtcNow;

            // Update line items: remove existing and re-add
            _context.DemandItems.RemoveRange(demand.Items);
            demand.Items.Clear();

            foreach (var itemReq in request.Items)
            {
                int suppliedQty = 0;
                int remainingQty = itemReq.RequestedQty - suppliedQty;

                demand.Items.Add(new DemandItem
                {
                    DemandId = demand.Id,
                    ProductId = itemReq.ProductId,
                    RequestedQty = itemReq.RequestedQty,
                    SuppliedQty = suppliedQty,
                    RemainingQty = remainingQty,
                    UnitPrice = itemReq.UnitPrice,
                    Status = DemandItemStatus.Pending
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetDemandByIdAsync(demand.Id))!;
        }

        public async Task<DemandDetailDto> UpdateDemandStatusAsync(int id, DemandStatus status)
        {
            var demand = await _context.Demands
                .Include(d => d.Items)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (demand == null)
            {
                throw new KeyNotFoundException($"Demand with ID {id} not found.");
            }

            demand.Status = status;
            demand.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return (await GetDemandByIdAsync(demand.Id))!;
        }

        private static DemandDetailDto MapToDetailDto(Demand demand)
        {
            var itemsDto = demand.Items.Select(i => new DemandItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                Sku = i.Product?.Sku,
                RequestedQty = i.RequestedQty,
                SuppliedQty = i.SuppliedQty,
                RemainingQty = i.RequestedQty - i.SuppliedQty,
                UnitPrice = i.UnitPrice,
                LineTotal = i.RequestedQty * i.UnitPrice,
                Status = i.Status
            }).ToList();

            return new DemandDetailDto
            {
                Id = demand.Id,
                DemandNumber = demand.DemandNumber,
                Title = demand.Title,
                SeasonId = demand.SeasonId,
                SeasonName = demand.Season?.Name ?? string.Empty,
                SupplierId = demand.SupplierId,
                SupplierName = demand.Supplier?.Name ?? string.Empty,
                Status = demand.Status,
                Notes = demand.Notes,
                CreatedAt = demand.CreatedAt,
                UpdatedAt = demand.UpdatedAt,
                TotalRequestedQty = itemsDto.Sum(i => i.RequestedQty),
                TotalSuppliedQty = itemsDto.Sum(i => i.SuppliedQty),
                TotalAmount = itemsDto.Sum(i => i.LineTotal),
                Items = itemsDto
            };
        }
    }
}
