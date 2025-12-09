using ASTRASystem.DTO.Store;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly ILogger<StoreController> _logger;

        public StoreController(IStoreService storeService, ILogger<StoreController> logger)
        {
            _storeService = storeService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStoreById(long id)
        {
            var result = await _storeService.GetStoreByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetStores([FromQuery] StoreQueryDto query)
        {
            var result = await _storeService.GetStoresAsync(query);
            return Ok(result);
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetStoresForLookup([FromQuery] string? searchTerm = null)
        {
            var result = await _storeService.GetStoresForLookupAsync(searchTerm);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DistributorAdmin,Agent")]
        public async Task<IActionResult> CreateStore([FromBody] CreateStoreDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CreateStore: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("CreateStore: User {UserId} creating store {StoreName}", userId, request.Name);

            var result = await _storeService.CreateStoreAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetStoreById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin,Agent")]
        public async Task<IActionResult> UpdateStore(long id, [FromBody] UpdateStoreDto request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { success = false, message = "ID mismatch" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UpdateStore: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("UpdateStore: User {UserId} updating store {StoreId}", userId, id);

            var result = await _storeService.UpdateStoreAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStore(long id)
        {
            var result = await _storeService.DeleteStoreAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("credit-limit")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant")]
        public async Task<IActionResult> UpdateCreditLimit([FromBody] UpdateCreditLimitDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UpdateCreditLimit: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("UpdateCreditLimit: User {UserId} updating credit limit for store {StoreId}", userId, request.StoreId);

            var result = await _storeService.UpdateCreditLimitAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("{id}/balance")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant,Agent")]
        public async Task<IActionResult> GetStoreWithBalance(long id)
        {
            var result = await _storeService.GetStoreWithBalanceAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("outstanding-balances")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant")]
        public async Task<IActionResult> GetStoresWithOutstandingBalance()
        {
            var result = await _storeService.GetStoresWithOutstandingBalanceAsync();
            return Ok(result);
        }

    }
}
