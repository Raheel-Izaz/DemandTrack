using DemandTrack.Dtos;
using DemandTrack.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemandTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeasonsController : ControllerBase
    {
        private readonly ISeasonService _seasonService;

        public SeasonsController(ISeasonService seasonService)
        {
            _seasonService = seasonService;
        }

        /// <summary>
        /// List all seasons ordered by CreatedAt DESC
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<SeasonDto>>> GetAll()
        {
            var seasons = await _seasonService.GetAllAsync();
            return Ok(seasons);
        }

        /// <summary>
        /// Get the current default season
        /// </summary>
        [HttpGet("default")]
        public async Task<ActionResult<SeasonDto>> GetDefault()
        {
            var season = await _seasonService.GetDefaultSeasonAsync();
            if (season == null)
            {
                return NotFound("No default season is currently configured.");
            }
            return Ok(season);
        }

        /// <summary>
        /// Create a new season
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SeasonDto>> Create([FromBody] CreateSeasonRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _seasonService.CreateAsync(request);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        /// <summary>
        /// Set a season as the default season
        /// </summary>
        [HttpPut("{id:int}/set-default")]
        public async Task<ActionResult<SeasonDto>> SetDefault(int id)
        {
            try
            {
                var updated = await _seasonService.SetDefaultAsync(id);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
