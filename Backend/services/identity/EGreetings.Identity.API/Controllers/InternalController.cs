using EGreetings.Identity.Application.DTOs;
using EGreetings.Identity.Application.Common.Interfaces;
using EGreetings.Identity.API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.Identity.API.Controllers;

[ApiController]
[Route("api/internal")]
[ServiceFilter(typeof(InternalApiAuthFilter))]
public class InternalController : ControllerBase
{
    private readonly IIdentityDbContext _dbContext;
    private readonly ILogger<InternalController> _logger;

    public InternalController(IIdentityDbContext dbContext, ILogger<InternalController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("users/{id}")]
    public IActionResult GetUser(int id)
    {
        try
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                _logger.LogWarning("User not found. UserId: {UserId}", id);
                return NotFound();
            }

            var userDto = new UserSummaryDto(
                Id: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                Role: user.Role,
                Status: user.Status,
                CreatedAt: user.CreatedAt
            );

            _logger.LogInformation("Internal user request retrieved. UserId: {UserId}", id);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user");
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }
}
