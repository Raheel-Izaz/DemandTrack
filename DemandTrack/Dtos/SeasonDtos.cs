using System.ComponentModel.DataAnnotations;

namespace DemandTrack.Dtos
{
    public class SeasonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
    }

    public class CreateSeasonRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public bool IsDefault { get; set; }
    }
}
