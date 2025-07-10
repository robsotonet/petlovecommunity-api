using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetLoveCommunity.Application.Interfaces;

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
    public async Task<IActionResult> GetAllPetsAsync()
    {
        try
        {
            var pets = await _petService.GetAllAvailablePetsAsync();
            _logger.LogInformation("Retrieved {Count} pets from service", pets.Count());
            return Ok(pets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pets");
            return StatusCode(500, "An error occurred while retrieving pets");
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPetByIdAsync(Guid id)
    {
        try
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                return NotFound($"Pet with ID {id} not found");
            }

            // Increment views asynchronously without blocking the response
            _ = Task.Run(async () => await _petService.IncrementViewsAsync(id));

            return Ok(pet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pet with ID {PetId}", id);
            return StatusCode(500, "An error occurred while retrieving the pet");
        }
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchPetsAsync([FromQuery] string q, [FromQuery] string? type = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search term is required");
            }

            var pets = await _petService.SearchPetsAsync(q, type);
            _logger.LogInformation("Search for '{SearchTerm}' returned {Count} pets", q, pets.Count());
            return Ok(pets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching pets with term '{SearchTerm}'", q);
            return StatusCode(500, "An error occurred while searching pets");
        }
    }

    [HttpGet("type/{petType}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPetsByTypeAsync(string petType)
    {
        try
        {
            var pets = await _petService.GetPetsByTypeAsync(petType);
            _logger.LogInformation("Retrieved {Count} pets of type {PetType}", pets.Count(), petType);
            return Ok(pets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pets of type {PetType}", petType);
            return StatusCode(500, "An error occurred while retrieving pets");
        }
    }
}