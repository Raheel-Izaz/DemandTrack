using System.Text.Json;

namespace DemandTrack.Dtos
{
    public class GradeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateGradeDto
    {
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string UnitOfMeasure { get; set; } = "Piece";
        public decimal DefaultUnitPrice { get; set; }
        public decimal CostPrice { get; set; }
        public bool IsActive { get; set; }
        
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public int? GradeId { get; set; }
        public string? GradeName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Dynamic Attributes dictionary / JSON
        public Dictionary<string, object?>? Attributes { get; set; }

        // Inventory snapshot
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable => QuantityOnHand - QuantityReserved;
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = null!;
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string UnitOfMeasure { get; set; } = "Piece";
        public decimal DefaultUnitPrice { get; set; }
        public decimal CostPrice { get; set; }
        public int CategoryId { get; set; }
        public int? GradeId { get; set; }

        // Dynamic category metadata dictionary
        public Dictionary<string, object?>? Attributes { get; set; }

        // Initial inventory stock
        public int InitialQuantityOnHand { get; set; } = 0;
    }

    public class UpdateProductDto
    {
        public string Name { get; set; } = null!;
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string UnitOfMeasure { get; set; } = "Piece";
        public decimal DefaultUnitPrice { get; set; }
        public decimal CostPrice { get; set; }
        public int CategoryId { get; set; }
        public int? GradeId { get; set; }
        public bool IsActive { get; set; } = true;

        public Dictionary<string, object?>? Attributes { get; set; }
    }

    public class ProductFilterDto
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? GradeId { get; set; }
        public bool? IsActive { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class InventoryAdjustmentDto
    {
        public int QuantityDelta { get; set; }
        public string? Reason { get; set; }
    }

    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int QuantityDelta { get; set; }
        public string TransactionType { get; set; } = null!;
        public string? ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PagedResultDto<T>
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public List<T> Items { get; set; } = new();
    }
}
