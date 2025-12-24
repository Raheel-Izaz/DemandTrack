namespace DemandTrack.Models
{
    public class ShipmentItem
    {
        public int Id { get; set; }

        public int ShipmentId { get; set; }
        public Shipment? Shipment { get; set; } = null!;

        public int BookId { get; set; }
        public Book? Book { get; set; } = null!;

        public int ReceivedQty { get; set; }
    }
}
