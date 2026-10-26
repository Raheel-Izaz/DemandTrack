namespace DemandTrack.Dtos
{
    public class DemandListDto
    {
        public int Id { get; set; }
        public string DemandNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int? ParentDemandId { get; set; }
        public string? ParentDemandNumber { get; set; }
        public string Status { get; set; } = null!;
        public int TotalItemsCount { get; set; }
        public int RequestedTotalQty { get; set; }
        public int SuppliedTotalQty { get; set; }
        public int RemainingTotalQty { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? TargetDeliveryDate { get; set; }
    }

    public class DemandDetailDto
    {
        public int Id { get; set; }
        public string DemandNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int? ParentDemandId { get; set; }
        public string? ParentDemandNumber { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? TargetDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public List<DemandItemDetailDto> Items { get; set; } = new();
        public List<DemandBackorderSummaryDto> Backorders { get; set; } = new();
    }

    public class DemandItemDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string UnitOfMeasure { get; set; } = "Piece";
        public string? CategoryName { get; set; }
        public int RequestedQty { get; set; }
        public int SuppliedQty { get; set; }
        public int RemainingQty { get; set; }
        public decimal UnitPrice { get; set; }
        public string Status { get; set; } = null!;
    }

    public class CreateDemandRequestDto
    {
        public string Title { get; set; } = null!;
        public int? SupplierId { get; set; }
        public DateTime? TargetDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public List<CreateDemandItemRequestDto> Items { get; set; } = new();
    }

    public class CreateDemandItemRequestDto
    {
        public int ProductId { get; set; }
        public int RequestedQty { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class DemandBackorderSummaryDto
    {
        public int Id { get; set; }
        public string DemandNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int TotalItems { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
