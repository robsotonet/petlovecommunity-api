using Microsoft.AspNetCore.Mvc;
using PetLoveCommunity.Application.DTOs.Auth;
using PetLoveCommunity.Application.DTOs.Shared;
using PetLoveCommunity.Application.Interfaces;
using PetLoveCommunity.Domain.Entities;
using PetLoveCommunity.API.Extensions;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("login")]
    public async Task<ApiResponse<AuthResponseDto>> LoginAsync([FromBody] LoginRequestDto loginRequestDto)
    {
        var correlationId = this.GetCorrelationId();
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse<AuthResponseDto>.ValidationError(
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList());
            }

            var user = await _userService.Authenticate(loginRequestDto.Email, loginRequestDto.Password);
            if (user == null)
            {
                _logger.LogWarning("CorrelationId: {CorrelationId} - Failed login attempt for email: {Email}", correlationId, loginRequestDto.Email);
                return ApiResponse<AuthResponseDto>.Failure("Invalid email or password");
            }

            if (user.Status != UserStatus.Active)
            {
                _logger.LogWarning("CorrelationId: {CorrelationId} - Inactive user login attempt for email: {Email}, Status: {Status}", 
                    correlationId, loginRequestDto.Email, user.Status);
                return ApiResponse<AuthResponseDto>.Failure("Account is not active");
            }

            var token = _jwtService.GenerateJwtToken(user);
            var authResponse = new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // Should match JWT settings
                User = MapUserToDto(user)
            };

            _logger.LogInformation("CorrelationId: {CorrelationId} - Successful login for user: {UserId}", correlationId, user.Id);
            return ApiResponse<AuthResponseDto>.Success(authResponse, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CorrelationId: {CorrelationId} - Error during login for email: {Email}", correlationId, loginRequestDto.Email);
            return ApiResponse<AuthResponseDto>.Failure("An error occurred during login");
        }
    }

    [HttpPost("register")]
    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync([FromBody] RegisterRequestDto registerRequestDto)
    {
        var correlationId = this.GetCorrelationId();
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse<AuthResponseDto>.ValidationError(
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList());
            }

            // Check if user already exists
            var existingUser = await _userService.GetUserByEmail(registerRequestDto.Email);
            if (existingUser != null)
            {
                return ApiResponse<AuthResponseDto>.Failure("User with this email already exists");
            }

            // Create new user
            var user = new User
            {
                FirstName = registerRequestDto.FirstName,
                LastName = registerRequestDto.LastName,
                Email = registerRequestDto.Email,
                Password = registerRequestDto.Password,
                PhoneNumber = registerRequestDto.PhoneNumber,
                Role = UserRole.Free,
                Status = UserStatus.Active,
                IsEmailVerified = false
            };

            await _userService.Register(user);

            // Generate token for immediate login
            var token = _jwtService.GenerateJwtToken(user);
            var authResponse = new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // Should match JWT settings
                User = MapUserToDto(user)
            };

            _logger.LogInformation("CorrelationId: {CorrelationId} - Successful registration for user: {UserId}", correlationId, user.Id);
            return ApiResponse<AuthResponseDto>.Success(authResponse, "Registration successful");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "CorrelationId: {CorrelationId} - Registration failed for email: {Email}", correlationId, registerRequestDto.Email);
            return ApiResponse<AuthResponseDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CorrelationId: {CorrelationId} - Error during registration for email: {Email}", correlationId, registerRequestDto.Email);
            return ApiResponse<AuthResponseDto>.Failure("An error occurred during registration");
        }
    }

    private static UserDto MapUserToDto(User user)
    {
        return new UserDto
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
        };
    }
}