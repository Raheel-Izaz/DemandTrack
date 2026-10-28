using DemandTrack.Data;
using DemandTrack.Dtos;
using DemandTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace DemandTrack.Services
{
    public class SeasonService : ISeasonService
    {
        private readonly AppDbContext _context;

        public SeasonService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SeasonDto>> GetAllAsync()
        {
            return await _context.Seasons
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SeasonDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsActive = s.IsActive,
                    IsDefault = s.IsDefault
                })
                .ToListAsync();
        }

        public async Task<SeasonDto?> GetDefaultSeasonAsync()
        {
            var season = await _context.Seasons
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IsDefault);

            if (season == null) return null;

            return new SeasonDto
            {
                Id = season.Id,
                Name = season.Name,
                IsActive = season.IsActive,
                IsDefault = season.IsDefault
            };
        }

        public async Task<SeasonDto> CreateAsync(CreateSeasonRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            if (request.IsDefault)
            {
                // Unset IsDefault on all other seasons
                await _context.Seasons
                    .Where(s => s.IsDefault)
                    .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsDefault, false));
            }

            var season = new Season
            {
                Name = request.Name.Trim(),
                IsActive = true,
                IsDefault = request.IsDefault,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Seasons.Add(season);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new SeasonDto
            {
                Id = season.Id,
                Name = season.Name,
                IsActive = season.IsActive,
                IsDefault = season.IsDefault
            };
        }

        public async Task<SeasonDto> SetDefaultAsync(int seasonId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var season = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId);
            if (season == null)
            {
                throw new KeyNotFoundException($"Season with ID {seasonId} not found.");
            }

            await _context.Seasons
                .Where(s => s.IsDefault && s.Id != seasonId)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsDefault, false));

            season.IsDefault = true;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new SeasonDto
            {
                Id = season.Id,
                Name = season.Name,
                IsActive = season.IsActive,
                IsDefault = season.IsDefault
            };
        }
    }
}
