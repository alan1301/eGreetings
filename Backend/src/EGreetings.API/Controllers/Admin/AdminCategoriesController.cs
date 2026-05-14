using EGreetings.Application.Commands.Admin.Categories;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers.Admin;

/// <summary>UC27 – Admin category management.</summary>
[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCategoriesController(IMediator mediator) => _mediator = mediator;

    // UC27: Create category
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return Created($"/api/categories/{id}", ApiResponse<object>.Created(new { id }));
    }

    // UC27: Hide category
    [HttpPatch("{id:guid}/hide")]
    public async Task<IActionResult> HideCategory(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new HideCategoryCommand(id), ct);
        return Ok(ApiResponse.Ok("Category hidden."));
    }
}
