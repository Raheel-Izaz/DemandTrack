using DemandTrack.Dtos;
using DemandTrack.Models;
using DemandTrack.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemandTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DemandsController : ControllerBase
    {
        private readonly IDemandService _demandService;

        public DemandsController(IDemandService demandService)
        {
            _demandService = demandService;
        }

        /// <summary>
        /// Get paginated list of demands filterable by seasonId, status, supplierId, and search term
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<DemandListDto>>> GetDemands(
            [FromQuery] int? seasonId,
            [FromQuery] DemandStatus? status,
            [FromQuery] int? supplierId,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _demandService.GetDemandsAsync(seasonId, status, supplierId, search, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get full demand detail with line items by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DemandDetailDto>> GetById(int id)
        {
            var demand = await _demandService.GetDemandByIdAsync(id);
            if (demand == null)
            {
                return NotFound($"Demand with ID {id} not found.");
            }
            return Ok(demand);
        }

        /// <summary>
        /// Create a new demand (supports Draft or Open status)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DemandDetailDto>> Create([FromBody] CreateDemandRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _demandService.CreateDemandAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update demand header and line items (if not already PartiallyFulfilled or Fulfilled)
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<DemandDetailDto>> Update(int id, [FromBody] UpdateDemandRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _demandService.UpdateDemandAsync(id, request);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update demand status (e.g., mark as Open or Cancelled)
        /// </summary>
        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<DemandDetailDto>> UpdateStatus(int id, [FromBody] UpdateDemandStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _demandService.UpdateDemandStatusAsync(id, request.Status);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
