using System;
using System.Collections.Generic;

namespace DemandTrack.Models
{
    public class SupplyUpload
    {
        public int Id { get; set; }

        public int DemandId { get; set; }
        public Demand? Demand { get; set; }

        public string FileName { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SupplyItem> Items { get; set; } = new List<SupplyItem>();
    }
}
