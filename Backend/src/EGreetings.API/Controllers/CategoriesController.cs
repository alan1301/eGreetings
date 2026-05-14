using EGreetings.Application.Queries.Cards;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC03 – Public category listing.
/// </summary>
[ApiController]
[Route("api/categories")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), ct);
        return Ok(ApiResponse<List<CategoryDto>>.Ok(result));
    }
}
