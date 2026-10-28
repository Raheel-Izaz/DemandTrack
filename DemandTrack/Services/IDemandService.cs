using DemandTrack.Dtos;
using DemandTrack.Models;

namespace DemandTrack.Services
{
    public interface IDemandService
    {
        Task<PagedResult<DemandListDto>> GetDemandsAsync(int? seasonId, DemandStatus? status, int? supplierId, string? search, int pageNumber = 1, int pageSize = 10);
        Task<DemandDetailDto?> GetDemandByIdAsync(int id);
        Task<DemandDetailDto> CreateDemandAsync(CreateDemandRequest request);
        Task<DemandDetailDto> UpdateDemandAsync(int id, UpdateDemandRequest request);
        Task<DemandDetailDto> UpdateDemandStatusAsync(int id, DemandStatus status);
    }
}
