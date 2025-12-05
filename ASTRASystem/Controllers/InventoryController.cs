using ASTRASystem.DTO.Inventory;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInventoryById(long id)
        {
            var result = await _inventoryService.GetInventoryByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetInventories([FromQuery] InventoryQueryDto query)
        {
            var result = await _inventoryService.GetInventoriesAsync(query);
            return Ok(result);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetInventorySummary([FromQuery] long? warehouseId = null)
        {
            var result = await _inventoryService.GetInventorySummaryAsync(warehouseId);
            return Ok(result);
        }

        [HttpGet("{id}/movements")]
        public async Task<IActionResult> GetInventoryMovements(long id, [FromQuery] int limit = 50)
        {
            var result = await _inventoryService.GetInventoryMovementsAsync(id, limit);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CreateInventory: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("CreateInventory: User {UserId} creating inventory for product {ProductId}", userId, request.ProductId);

            var result = await _inventoryService.CreateInventoryAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetInventoryById), new { id = result.Data.Id }, result);
        }

        [HttpPost("adjust")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> AdjustInventory([FromBody] AdjustInventoryDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("AdjustInventory: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("AdjustInventory: User {UserId} adjusting inventory {InventoryId}", userId, request.InventoryId);

            var result = await _inventoryService.AdjustInventoryAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("restock")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> RestockInventory([FromBody] RestockInventoryDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("RestockInventory: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("RestockInventory: User {UserId} restocking product {ProductId}", userId, request.ProductId);

            var result = await _inventoryService.RestockInventoryAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("levels")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> UpdateInventoryLevels([FromBody] UpdateInventoryLevelsDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UpdateInventoryLevels: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("UpdateInventoryLevels: User {UserId} updating inventory {InventoryId}", userId, request.InventoryId);

            var result = await _inventoryService.UpdateInventoryLevelsAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteInventory(long id)
        {
            var result = await _inventoryService.DeleteInventoryAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
