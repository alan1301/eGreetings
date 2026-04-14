using EGreetings.Application.Features.Categories.Queries.GetPublicCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC03 - Xem danh sách danh mục | UC27 - Quản lý danh mục (Admin)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>UC03 - Xem danh sách danh mục công khai</summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPublicCategoriesQuery(), ct);
        return Ok(new { Success = true, Data = result });
    }
}
