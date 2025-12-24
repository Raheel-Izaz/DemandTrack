using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DemandTrack.Models
{
    public class DemandItem
    {
        public int Id { get; set; }

        public int DemandId { get; set; }
        [JsonIgnore]
        public Demand? Demand { get; set; }

        public int BookId { get; set; }
        [JsonIgnore]
        public Book? Book { get; set; }

        public int RequestedQty { get; set; }
        
        [JsonIgnore]
        public ICollection<SupplyItem> SupplyItems { get; set; } = new List<SupplyItem>();
    }
}
