using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PetLoveCommunity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllPetsAsync()
    {
        await Task.CompletedTask; // Placeholder for async operation
        return Ok("Request received");
    }
}