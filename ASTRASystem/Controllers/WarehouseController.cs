using ASTRASystem.DTO.Common;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IDistributorService _distributorService;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(
            IWarehouseService warehouseService,
            IDistributorService distributorService,
            ILogger<WarehouseController> logger)
        {
            _warehouseService = warehouseService;
            _distributorService = distributorService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWarehouseById(long id)
        {
            var result = await _warehouseService.GetWarehouseByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetWarehouses([FromQuery] long? distributorId = null)
        {
            var result = await _warehouseService.GetWarehousesAsync(distributorId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _warehouseService.CreateWarehouseAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetWarehouseById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> UpdateWarehouse(long id, [FromBody] WarehouseDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }

            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _warehouseService.UpdateWarehouseAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteWarehouse(long id)
        {
            var result = await _warehouseService.DeleteWarehouseAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // Distributor endpoints
        [HttpGet("distributor/{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetDistributorById(long id)
        {
            var result = await _distributorService.GetDistributorByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("distributor")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetDistributors()
        {
            var result = await _distributorService.GetDistributorsAsync();
            return Ok(result);
        }

        [HttpPost("distributor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDistributor([FromBody] DistributorDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _distributorService.CreateDistributorAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("distributor/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDistributor(long id, [FromBody] DistributorDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }

            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _distributorService.UpdateDistributorAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("distributor/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDistributor(long id)
        {
            var result = await _distributorService.DeleteDistributorAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
