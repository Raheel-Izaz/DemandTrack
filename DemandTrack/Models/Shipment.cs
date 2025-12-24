using System;
using System.Collections.Generic;

namespace DemandTrack.Models
{
    public class Shipment
    {
        public int Id { get; set; }

        public int DemandId { get; set; }
        public Demand Demand { get; set; } = null!;

        public string TranscriptPath { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
    }
}
