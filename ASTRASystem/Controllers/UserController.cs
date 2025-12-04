using ASTRASystem.DTO.User;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _userService.GetUserByIdAsync(userId);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var result = await _userService.GetUserByEmailAsync(email);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetUsers([FromQuery] UserQueryDto query)
        {
            var result = await _userService.GetUsersAsync(query);
            return Ok(result);
        }

        // Update current user's own profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _userService.UpdateUserProfileAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // Admin update any user's profile
        [HttpPut("{id}/profile")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> UpdateUserProfile(string id, [FromBody] UpdateUserProfileDto request)
        {
            var result = await _userService.UpdateUserProfileAsync(id, request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("approve")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> ApproveUser([FromBody] ApproveUserDto request)
        {
            var result = await _userService.ApproveUserAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("assign-roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto request)
        {
            var result = await _userService.AssignRolesAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("{id}/roles")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            var result = await _userService.GetUserRolesAsync(id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _userService.GetAllRolesAsync();
            return Ok(result);
        }

        [HttpGet("role/{role}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            var result = await _userService.GetUsersByRoleAsync(role);
            return Ok(result);
        }

        [HttpGet("unapproved")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetUnapprovedUsers()
        {
            var result = await _userService.GetUnapprovedUsersAsync();
            return Ok(result);
        }
    }
}
