namespace DemandTrack.Dtos
{
    public class SupplierInvoiceDto
    {
        public int Id { get; set; }
        public int DemandId { get; set; }
        public string? DemandNumber { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public DateTime InvoiceDate { get; set; }
        public string SourceType { get; set; } = null!;
        public string? DocumentFilePath { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public List<SupplierInvoiceItemDto> Items { get; set; } = new();
    }

    public class SupplierInvoiceItemDto
    {
        public int Id { get; set; }
        public int? DemandItemId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? RawItemDescription { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal ConfidenceScore { get; set; }
        public bool IsManuallyConfirmed { get; set; }
    }

    public class ConfirmReconciliationItemRequestDto
    {
        public int InvoiceItemId { get; set; }
        public int? MatchedProductId { get; set; }
        public int? MatchedDemandItemId { get; set; }
        public int ConfirmedQuantity { get; set; }
        public decimal ConfirmedUnitPrice { get; set; }
    }

    public class ConfirmReconciliationRequestDto
    {
        public int InvoiceId { get; set; }
        public bool AutoCreateBackorderForRemaining { get; set; } = true;
        public List<ConfirmReconciliationItemRequestDto> Items { get; set; } = new();
    }

    public class ReconciliationResultDto
    {
        public int InvoiceId { get; set; }
        public int DemandId { get; set; }
        public bool IsDemandFullyFulfilled { get; set; }
        public int? GeneratedBackorderDemandId { get; set; }
        public string? GeneratedBackorderDemandNumber { get; set; }
        public int RemainingItemsCount { get; set; }
        public string Message { get; set; } = null!;
    }
}
