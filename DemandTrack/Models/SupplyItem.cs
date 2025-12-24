using System.Text.Json.Serialization;

namespace DemandTrack.Models
{
    public class SupplyItem
    {
        public int Id { get; set; }

        public int SupplyUploadId { get; set; }
        [JsonIgnore]
        public SupplyUpload? SupplyUpload { get; set; }

        public int DemandItemId { get; set; }
        [JsonIgnore]
        public DemandItem? DemandItem { get; set; }

        public string Isbn { get; set; } = null!;
        public int ShippedQty { get; set; }
    }
}
