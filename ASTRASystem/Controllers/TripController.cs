using ASTRASystem.DTO.Trip;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ILogger<TripController> _logger;

        public TripController(ITripService tripService, ILogger<TripController> logger)
        {
            _tripService = tripService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripById(long id)
        {
            var result = await _tripService.GetTripByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips([FromQuery] TripQueryDto query)
        {
            var result = await _tripService.GetTripsAsync(query);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _tripService.CreateTripAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetTripById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> UpdateTrip(long id, [FromBody] UpdateTripDto request)
        {
            if (id != request.TripId)
            {
                return BadRequest("ID mismatch");
            }

            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _tripService.UpdateTripAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("status")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> UpdateTripStatus([FromBody] UpdateTripStatusDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _tripService.UpdateTripStatusAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("reorder")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> ReorderTripAssignments([FromBody] ReorderTripAssignmentsDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _tripService.ReorderTripAssignmentsAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> CancelTrip(long id, [FromQuery] string? reason)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _tripService.CancelTripAsync(id, userId, reason);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("{id}/manifest")]
        public async Task<IActionResult> GetTripManifest(long id)
        {
            var result = await _tripService.GetTripManifestAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("{id}/manifest/pdf")]
        public async Task<IActionResult> GenerateTripManifestPdf(long id)
        {
            var result = await _tripService.GenerateTripManifestPdfAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return File(result.Data, "application/pdf", $"trip_manifest_{id}.pdf");
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> GetActiveTrips([FromQuery] string? dispatcherId = null)
        {
            var result = await _tripService.GetActiveTripsAsync(dispatcherId);
            return Ok(result);
        }

        [HttpPost("suggest-sequence")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> SuggestTripSequence([FromBody] List<long> orderIds)
        {
            var result = await _tripService.SuggestTripSequenceAsync(orderIds);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
