namespace DemandTrack.Services.Exports
{
    public interface IDemandExportService
    {
        Task<byte[]> GeneratePdfAsync(int demandId, CancellationToken cancellationToken = default);
        Task<byte[]> GenerateExcelAsync(int demandId, CancellationToken cancellationToken = default);
    }
}
