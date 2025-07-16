using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetLoveCommunity.Application.DTOs.Auth;
using PetLoveCommunity.Application.DTOs.Shared;
using PetLoveCommunity.Application.Interfaces;
using PetLoveCommunity.API.Extensions;

namespace PetLoveCommunity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var correlationId = this.GetCorrelationId();
        try
        {
            var users = await _userService.GetAllUsers();
            
            if (users == null)
                throw new ArgumentNullException(nameof(users), "User service returned null");
                
            var userDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                IsEmailVerified = user.IsEmailVerified,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt
            }).ToList();

            var response = ApiResponse<List<UserDto>>.Success(userDtos, "Users retrieved successfully");
            
            _logger.LogInformation("CorrelationId: {CorrelationId} - Retrieved {UserCount} users", correlationId, userDtos.Count);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CorrelationId: {CorrelationId} - Error retrieving users", correlationId);
            var response = ApiResponse<List<UserDto>>.Failure("An error occurred while retrieving users");
            return StatusCode(500, response);
        }
    }
}