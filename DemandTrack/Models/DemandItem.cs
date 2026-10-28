namespace DemandTrack.Models
{
    public class DemandItem
    {
        public int Id { get; set; }

        public int DemandId { get; set; }
        public Demand Demand { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int RequestedQty { get; set; }
        public int SuppliedQty { get; set; } = 0;
        public int RemainingQty { get; set; } = 0;
        public decimal UnitPrice { get; set; } = 0.0m;

        public DemandItemStatus Status { get; set; } = DemandItemStatus.Pending;

        public ICollection<SupplierInvoiceItem> InvoiceItems { get; set; } = new List<SupplierInvoiceItem>();
    }
}
