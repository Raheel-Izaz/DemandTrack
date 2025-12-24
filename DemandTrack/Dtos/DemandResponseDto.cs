using System;
using System.Collections.Generic;

namespace DemandTrack.Dtos
{
    public class DemandResponseDto
    {
        public int Id { get; set; }
        public string SeasonName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalQty { get; set; }
        public List<DemandItemResponseDto> Items { get; set; } = new();
    }

    public class DemandItemResponseDto
    {
        public string BookTitle { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public int RequestedQty { get; set; }
    }
}
