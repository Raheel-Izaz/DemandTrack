using System;
using System.Collections.Generic;

namespace DemandTrack.Models
{
    public class Season
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<Demand> Demands { get; set; } = new List<Demand>();
    }
}
