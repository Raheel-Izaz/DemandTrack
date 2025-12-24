using System.Collections.Generic;

namespace DemandTrack.Dtos
{
    public class CreateDemandDto
    {
        public string SeasonName { get; set; } = string.Empty;
        public List<CreateDemandItemDto> Items { get; set; } = new();
    }

    public class CreateDemandItemDto
    {
        public int BookId { get; set; }
        public int RequestedQty { get; set; }
    }
}
