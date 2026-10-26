namespace DemandTrack.Models
{
    public class DemandItem
    {
        public int Id { get; set; }

        public int DemandId { get; set; }
        public Demand? Demand { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int RequestedQty { get; set; }
        public int SuppliedQty { get; set; } = 0;
        public int RemainingQty { get; set; } = 0;
        public decimal UnitPrice { get; set; } = 0.0m;

        public string Status { get; set; } = "Pending"; // Pending, PartiallyReceived, Completed, Backordered

        public ICollection<SupplierInvoiceItem> InvoiceItems { get; set; } = new List<SupplierInvoiceItem>();
    }
}
