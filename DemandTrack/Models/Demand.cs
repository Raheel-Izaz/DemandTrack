using System;
using System.Collections.Generic;

namespace DemandTrack.Models
{
    public class Demand
    {
        public int Id { get; set; }

        public string SeasonName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<DemandItem> Items { get; set; } = new List<DemandItem>();
    }
}
