using EGreetings.Greeting.Application.DTOs;
using EGreetings.Greeting.Application.Features.Admin;
using EGreetings.Greeting.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.Greeting.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetPublicCategories()
    {
        var result = await _mediator.Send(new GetPublicCategoriesQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        // Check admin role from header
        if (!IsAdmin())
        {
            return Forbid();
        }

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPublicCategories), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        // Check admin role from header
        if (!IsAdmin())
        {
            return Forbid();
        }

        var command = new UpdateCategoryCommand(id, request.Name, request.Description, request.IconUrl, request.IsActive);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteCategory(int id)
    {
        // Check admin role from header
        if (!IsAdmin())
        {
            return Forbid();
        }

        var result = await _mediator.Send(new DeleteCategoryCommand(id));
        return Ok(result);
    }

    private bool IsAdmin()
    {
        var userRole = HttpContext.Request.Headers["X-UserRole"].ToString();
        return userRole == "Admin";
    }
}

public record UpdateCategoryRequest(string? Name, string? Description, string? IconUrl, bool? IsActive);
