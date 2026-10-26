namespace DemandTrack.Models
{
    public class SupplierInvoiceItem
    {
        public int Id { get; set; }

        public int SupplierInvoiceId { get; set; }
        public SupplierInvoice? SupplierInvoice { get; set; }

        public int? DemandItemId { get; set; }
        public DemandItem? DemandItem { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public string? RawItemDescription { get; set; } // Raw text extracted from invoice
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal ConfidenceScore { get; set; } = 1.0m; // 0.0 to 1.0 for OCR/Fuzzy match
        public bool IsManuallyConfirmed { get; set; } = false;
    }
}
