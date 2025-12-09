using ASTRASystem.DTO.Location;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BarangayController : ControllerBase
    {
        private readonly IBarangayService _barangayService;
        private readonly ILogger<BarangayController> _logger;

        public BarangayController(IBarangayService barangayService, ILogger<BarangayController> logger)
        {
            _barangayService = barangayService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBarangayById(long id)
        {
            var result = await _barangayService.GetBarangayByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBarangays([FromQuery] BarangayQueryDto query)
        {
            var result = await _barangayService.GetBarangaysAsync(query);
            return Ok(result);
        }

        [HttpGet("city/{cityId}")]
        public async Task<IActionResult> GetBarangaysByCity(long cityId)
        {
            var result = await _barangayService.GetBarangaysByCityAsync(cityId);
            return Ok(result);
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetBarangaysForLookup([FromQuery] long? cityId = null, [FromQuery] string? searchTerm = null)
        {
            var result = await _barangayService.GetBarangaysForLookupAsync(cityId, searchTerm);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> CreateBarangay([FromBody] CreateBarangayDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CreateBarangay: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("CreateBarangay: User {UserId} creating barangay {BarangayName}", userId, request.Name);

            var result = await _barangayService.CreateBarangayAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetBarangayById), new { id = result.Data.Id }, result);
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> BulkCreateBarangays([FromBody] BulkCreateBarangaysDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("BulkCreateBarangays: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("BulkCreateBarangays: User {UserId} creating {Count} barangays for city {CityId}",
                userId, request.BarangayNames.Count, request.CityId);

            var result = await _barangayService.BulkCreateBarangaysAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> UpdateBarangay(long id, [FromBody] UpdateBarangayDto request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { success = false, message = "ID mismatch" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UpdateBarangay: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("UpdateBarangay: User {UserId} updating barangay {BarangayId}", userId, id);

            var result = await _barangayService.UpdateBarangayAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBarangay(long id)
        {
            var result = await _barangayService.DeleteBarangayAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
