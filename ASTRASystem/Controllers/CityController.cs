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
    public class CityController : ControllerBase
    {
        private readonly ICityService _cityService;
        private readonly ILogger<CityController> _logger;

        public CityController(ICityService cityService, ILogger<CityController> logger)
        {
            _cityService = cityService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCityById(long id)
        {
            var result = await _cityService.GetCityByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetCityByName(string name)
        {
            var result = await _cityService.GetCityByNameAsync(name);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCities([FromQuery] CityQueryDto query)
        {
            var result = await _cityService.GetCitiesAsync(query);
            return Ok(result);
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetCitiesForLookup([FromQuery] string? searchTerm = null)
        {
            var result = await _cityService.GetCitiesForLookupAsync(searchTerm);
            return Ok(result);
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var result = await _cityService.GetProvincesAsync();
            return Ok(result);
        }

        [HttpGet("regions")]
        public async Task<IActionResult> GetRegions()
        {
            var result = await _cityService.GetRegionsAsync();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> CreateCity([FromBody] CreateCityDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CreateCity: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("CreateCity: User {UserId} creating city {CityName}", userId, request.Name);

            var result = await _cityService.CreateCityAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetCityById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> UpdateCity(long id, [FromBody] UpdateCityDto request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { success = false, message = "ID mismatch" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UpdateCity: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("UpdateCity: User {UserId} updating city {CityId}", userId, id);

            var result = await _cityService.UpdateCityAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCity(long id)
        {
            var result = await _cityService.DeleteCityAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
