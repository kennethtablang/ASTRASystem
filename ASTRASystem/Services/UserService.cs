using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.User;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ASTRASystem.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            ILogger<UserService> logger,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found");
                }

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

                if (user.DistributorId.HasValue)
                {
                    var distributor = await _context.Distributors.FindAsync(user.DistributorId.Value);
                    userDto.DistributorName = distributor?.Name;
                }

                if (user.WarehouseId.HasValue)
                {
                    var warehouse = await _context.Warehouses.FindAsync(user.WarehouseId.Value);
                    userDto.WarehouseName = warehouse?.Name;
                }

                return ApiResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID {UserId}", userId);
                return ApiResponse<UserDto>.ErrorResponse("An error occurred while retrieving user");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found");
                }

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

                if (user.DistributorId.HasValue)
                {
                    var distributor = await _context.Distributors.FindAsync(user.DistributorId.Value);
                    userDto.DistributorName = distributor?.Name;
                }

                if (user.WarehouseId.HasValue)
                {
                    var warehouse = await _context.Warehouses.FindAsync(user.WarehouseId.Value);
                    userDto.WarehouseName = warehouse?.Name;
                }

                return ApiResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email {Email}", email);
                return ApiResponse<UserDto>.ErrorResponse("An error occurred while retrieving user");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<UserListItemDto>>> GetUsersAsync(UserQueryDto query)
        {
            try
            {
                var usersQuery = _userManager.Users.AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchLower = query.SearchTerm.ToLower();
                    usersQuery = usersQuery.Where(u =>
                        u.Email.ToLower().Contains(searchLower) ||
                        u.FirstName.ToLower().Contains(searchLower) ||
                        u.LastName.ToLower().Contains(searchLower));
                }

                if (query.IsApproved.HasValue)
                {
                    usersQuery = usersQuery.Where(u => u.IsApproved == query.IsApproved.Value);
                }

                if (query.DistributorId.HasValue)
                {
                    usersQuery = usersQuery.Where(u => u.DistributorId == query.DistributorId.Value);
                }

                if (query.WarehouseId.HasValue)
                {
                    usersQuery = usersQuery.Where(u => u.WarehouseId == query.WarehouseId.Value);
                }

                var totalCount = await usersQuery.CountAsync();
                var users = await usersQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var userDtos = new List<UserListItemDto>();
                foreach (var user in users)
                {
                    var dto = _mapper.Map<UserListItemDto>(user);
                    dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

                    if (!string.IsNullOrWhiteSpace(query.Role))
                    {
                        if (!dto.Roles.Contains(query.Role))
                            continue;
                    }

                    if (user.DistributorId.HasValue)
                    {
                        var distributor = await _context.Distributors.FindAsync(user.DistributorId.Value);
                        dto.DistributorName = distributor?.Name;
                    }

                    if (user.WarehouseId.HasValue)
                    {
                        var warehouse = await _context.Warehouses.FindAsync(user.WarehouseId.Value);
                        dto.WarehouseName = warehouse?.Name;
                    }

                    userDtos.Add(dto);
                }

                var paginatedResponse = new PaginatedResponse<UserListItemDto>(
                    userDtos, totalCount, query.PageNumber, query.PageSize);

                return ApiResponse<PaginatedResponse<UserListItemDto>>.SuccessResponse(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return ApiResponse<PaginatedResponse<UserListItemDto>>.ErrorResponse("An error occurred while retrieving users");
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserProfileAsync(string userId, UpdateUserProfileDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found");
                }

                if (!string.IsNullOrEmpty(request.Email) && request.Email.ToUpper() != user.NormalizedEmail)
                {
                    var existingUser = await _userManager.FindByEmailAsync(request.Email);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        return ApiResponse<UserDto>.ErrorResponse("Email is already registered by another user");
                    }

                    var setEmailResult = await _userManager.SetEmailAsync(user, request.Email);
                    if (!setEmailResult.Succeeded)
                    {
                        return ApiResponse<UserDto>.ErrorResponse(
                            "Failed to update email",
                            setEmailResult.Errors.Select(e => e.Description).ToList());
                    }

                    // Update username to match email if they were same
                    if (user.UserName.ToUpper() == user.NormalizedEmail) // Logic: if old username was old email
                    {
                        await _userManager.SetUserNameAsync(user, request.Email);
                    }
                    else
                    {
                         // Or force update username?
                         await _userManager.SetUserNameAsync(user, request.Email);
                    }
                }

                _mapper.Map(request, user);
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return ApiResponse<UserDto>.ErrorResponse(
                        "Failed to update profile",
                        result.Errors.Select(e => e.Description).ToList());
                }

                await _auditLogService.LogActionAsync(userId, "Profile updated", request);

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

                return ApiResponse<UserDto>.SuccessResponse(userDto, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return ApiResponse<UserDto>.ErrorResponse("An error occurred while updating profile");
            }
        }

        public async Task<ApiResponse<bool>> ApproveUserAsync(ApproveUserDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                user.IsApproved = request.Approve;
                user.ApprovalMessage = request.Message;

                if (request.Approve)
                {
                    user.EmailConfirmed = true;
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.ErrorResponse("Failed to update approval status");
                }

                await _auditLogService.LogActionAsync(user.Id, $"User {(request.Approve ? "approved" : "rejected")}", request);

                // Send email notification
                await _emailService.SendAccountApprovalEmailAsync(user, request.Approve, request.Message);

                return ApiResponse<bool>.SuccessResponse(true, $"User {(request.Approve ? "approved" : "rejected")} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving user");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(ASTRASystem.DTO.Auth.RegisterRequestDto request)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return ApiResponse<UserDto>.ErrorResponse(
                        "Email is already registered",
                        new List<string> { "A user with this email already exists" });
                }

                // Map DTO to ApplicationUser
                var user = _mapper.Map<ApplicationUser>(request);
                user.UserName = request.Email;
                user.IsApproved = true; // Admin created users are auto-approved
                user.EmailConfirmed = false; // Email confirmation required

                // Create user
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return ApiResponse<UserDto>.ErrorResponse(
                        "User creation failed",
                        result.Errors.Select(e => e.Description).ToList());
                }

                // Assign role
                await _userManager.AddToRoleAsync(user, request.Role);

                // Generate email confirmation token
                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                var clientUrl = _configuration["ClientUrl"] ?? "http://localhost:5173";
                var confirmationLink = $"{clientUrl}/confirm-email?token={Uri.EscapeDataString(confirmationToken)}&userId={user.Id}";

                // Send confirmation email
                await _emailService.SendConfirmationEmailAsync(user, confirmationLink);

                await _auditLogService.LogActionAsync(user.Id, "User created by admin", new { Email = user.Email, Role = request.Role });
                
                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = new List<string> { request.Role };

                return ApiResponse<UserDto>.SuccessResponse(userDto, "User created successfully. Confirmation email sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", request.Email);
                return ApiResponse<UserDto>.ErrorResponse("An error occurred during user creation");
            }
        }

        public async Task<ApiResponse<bool>> AssignRolesAsync(AssignRolesDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeResult.Succeeded)
                {
                    return ApiResponse<bool>.ErrorResponse("Failed to remove existing roles");
                }

                var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!addResult.Succeeded)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Failed to assign roles",
                        addResult.Errors.Select(e => e.Description).ToList());
                }

                await _auditLogService.LogActionAsync(user.Id, "Roles assigned", request);

                return ApiResponse<bool>.SuccessResponse(true, "Roles assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<List<string>>.ErrorResponse("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                return ApiResponse<List<string>>.SuccessResponse(roles.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles");
                return ApiResponse<List<string>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Failed to delete user",
                        result.Errors.Select(e => e.Description).ToList());
                }

                await _auditLogService.LogActionAsync(userId, "User deleted", null);

                return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<string>>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return ApiResponse<List<string>>.SuccessResponse(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return ApiResponse<List<string>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<UserListItemDto>>> GetUsersByRoleAsync(string role, long? distributorId = null)
        {
            try
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);

                if (distributorId.HasValue)
                {
                    usersInRole = usersInRole.Where(u => u.DistributorId == distributorId.Value).ToList();
                }

                var userDtos = new List<UserListItemDto>();

                foreach (var user in usersInRole)
                {
                    var dto = _mapper.Map<UserListItemDto>(user);
                    dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                    userDtos.Add(dto);
                }

                return ApiResponse<List<UserListItemDto>>.SuccessResponse(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role");
                return ApiResponse<List<UserListItemDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<UserListItemDto>>> GetUnapprovedUsersAsync()
        {
            try
            {
                var unapprovedUsers = await _userManager.Users
                    .Where(u => !u.IsApproved)
                    .ToListAsync();

                var userDtos = new List<UserListItemDto>();
                foreach (var user in unapprovedUsers)
                {
                    var dto = _mapper.Map<UserListItemDto>(user);
                    dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                    userDtos.Add(dto);
                }

                return ApiResponse<List<UserListItemDto>>.SuccessResponse(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unapproved users");
                return ApiResponse<List<UserListItemDto>>.ErrorResponse("An error occurred");
            }
        }
    }
}
