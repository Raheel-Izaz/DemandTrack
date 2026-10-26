using System.Text.Json;

namespace DemandTrack.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string UnitOfMeasure { get; set; } = "Piece";
        public decimal DefaultUnitPrice { get; set; } = 0.0m;
        public decimal CostPrice { get; set; } = 0.0m;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Category link
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Dynamic Grade link
        public int? GradeId { get; set; }
        public Grade? Grade { get; set; }

        // Dynamic JSONB attributes for category metadata (e.g., subject, isbn, rulingType, etc.)
        public JsonDocument? Attributes { get; set; }

        // Inventory
        public ProductInventory? Inventory { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

        // Downstream Demands
        public ICollection<DemandItem> DemandItems { get; set; } = new List<DemandItem>();
    }
}
