using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetLoveCommunity.Application.DTOs;
using PetLoveCommunity.Application.DTOs.Shared;
using PetLoveCommunity.Application.Interfaces;
using PetLoveCommunity.API.Extensions;

namespace PetLoveCommunity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly IPetService _petService;
    private readonly ILogger<PetsController> _logger;

    public PetsController(IPetService petService, ILogger<PetsController> logger)
    {
        _petService = petService;
        _logger = logger;
    }


    [HttpGet]
    [AllowAnonymous]
    public async Task<ApiResponse<PetListDto[]>> GetAllPetsAsync()
    {
        var correlationId = this.GetCorrelationId();
        try
        {
            var pets = await _petService.GetAllAvailablePetsAsync();
            _logger.LogInformation("CorrelationId: {CorrelationId} - Retrieved {Count} pets from service", correlationId, pets.Count());
            return ApiResponse<PetListDto[]>.Success(pets.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CorrelationId: {CorrelationId} - Error occurred while retrieving pets", correlationId);
            return ApiResponse<PetListDto[]>.Fail("An error occurred while retrieving pets");
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ApiResponse<PetDetailDto?>> GetPetByIdAsync(Guid id)
    {
        var correlationId = this.GetCorrelationId();
        try
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                _logger.LogWarning("CorrelationId: {CorrelationId} - Pet with ID {PetId} not found", correlationId, id);
                return ApiResponse<PetDetailDto?>.Fail($"Pet with ID {id} not found");
            }

            // Increment views asynchronously without blocking the response
            _ = Task.Run(async () => await _petService.IncrementViewsAsync(id));

            _logger.LogInformation("CorrelationId: {CorrelationId} - Retrieved pet with ID {PetId}", correlationId, id);
            return ApiResponse<PetDetailDto?>.Success(pet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CorrelationId: {CorrelationId} - Error occurred while retrieving pet with ID {PetId}", correlationId, id);
            return ApiResponse<PetDetailDto?>.Fail("An error occurred while retrieving the pet");
        }
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ApiResponse<PetListDto[]>> SearchPetsAsync([FromQuery] string q, [FromQuery] string? type = null)
    {
        var correlationId = this.GetCorrelationId();
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                _logger.LogWarning("CorrelationId: {CorrelationId} - Search attempted with empty or null search term", correlationId);
                return ApiResponse<PetListDto[]>.Fail("Search term is required");
            }

            var pets = await _petService.SearchPetsAsync(q, type);
            _logger.LogInformation("CorrelationId: {CorrelationId} - Search for '{SearchTerm}' returned {Count} pets", correlationId, q, pets.Count());
            return ApiResponse<PetListDto[]>.Success(pets.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CorrelationId: {CorrelationId} - Error occurred while searching pets with term '{SearchTerm}'", correlationId, q);
            return ApiResponse<PetListDto[]>.Fail("An error occurred while searching pets");
        }
    }

    [HttpGet("type/{petType}")]
    [AllowAnonymous]
    public async Task<ApiResponse<PetListDto[]>> GetPetsByTypeAsync(string petType)
    {
        var correlationId = this.GetCorrelationId();
        try
        {
            var pets = await _petService.GetPetsByTypeAsync(petType);
            _logger.LogInformation("CorrelationId: {CorrelationId} - Retrieved {Count} pets of type {PetType}", correlationId, pets.Count(), petType);
            return ApiResponse<PetListDto[]>.Success(pets.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CorrelationId: {CorrelationId} - Error occurred while retrieving pets of type {PetType}", correlationId, petType);
            return ApiResponse<PetListDto[]>.Fail("An error occurred while retrieving pets");
        }
    }
}