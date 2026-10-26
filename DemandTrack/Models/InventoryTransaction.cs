namespace DemandTrack.Models
{
    public enum InventoryTransactionType
    {
        Adjustment = 1,
        PurchaseReceipt = 2,
        DemandFulfillment = 3,
        Return = 4,
        Damaged = 5
    }

    public class InventoryTransaction
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int QuantityDelta { get; set; }
        public InventoryTransactionType TransactionType { get; set; }
        public string? ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
