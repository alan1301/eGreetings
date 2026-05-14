using EGreetings.Application.Queries.Cards;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC03, UC04, UC23, UC24 – Public greeting card listing and detail.
/// </summary>
[ApiController]
[Route("api/cards")]
[Produces("application/json")]
public class CardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CardsController(IMediator mediator) => _mediator = mediator;

    // UC03 / UC23 – List cards (public)
    [HttpGet]
    public async Task<IActionResult> GetCards(
        [FromQuery] string? category, [FromQuery] string? search,
        [FromQuery] string? sort, [FromQuery] bool? featured,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetCardsQuery(category, search, sort, featured, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<CardDto>>.OkPaged(result, result.Meta));
    }

    // UC04 / UC24 – Card detail (public)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCard(Guid id, CancellationToken ct)
    {
        var card = await _mediator.Send(new GetCardByIdQuery(id), ct);
        if (card == null) return NotFound(ApiResponse.Fail("Greeting card not found."));
        return Ok(ApiResponse<CardDetailDto>.Ok(card));
    }
}
