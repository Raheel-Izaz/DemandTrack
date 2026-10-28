using DemandTrack.Dtos;

namespace DemandTrack.Services
{
    public interface ISeasonService
    {
        Task<List<SeasonDto>> GetAllAsync();
        Task<SeasonDto?> GetDefaultSeasonAsync();
        Task<SeasonDto> CreateAsync(CreateSeasonRequest request);
        Task<SeasonDto> SetDefaultAsync(int seasonId);
    }
}
