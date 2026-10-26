namespace DemandTrack.Models
{
    public class ProductInventory
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int QuantityOnHand { get; set; } = 0;
        public int QuantityReserved { get; set; } = 0;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
