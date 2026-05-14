using EGreetings.Application.Commands.Admin.Cards;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers.Admin;

/// <summary>UC09 / UC10 – Admin card template management.</summary>
[ApiController]
[Route("api/admin/cards")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminCardsController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;

    public AdminCardsController(IMediator mediator) => _mediator = mediator;

    // UC09: Create card
    [HttpPost]
    public async Task<IActionResult> CreateCard([FromBody] CreateCardCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return Created($"/api/cards/{id}", ApiResponse<object>.Created(new { id }));
    }

    // UC09: Update card
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCard(Guid id, [FromBody] UpdateCardCommand command, CancellationToken ct)
    {
        var cmd = command with { CardId = id };
        await _mediator.Send(cmd, ct);
        return Ok(ApiResponse.Ok("Card template updated."));
    }

    // UC10: Archive card
    [HttpPatch("{id:guid}/archive")]
    public async Task<IActionResult> ArchiveCard(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ArchiveCardCommand(id), ct);
        return Ok(ApiResponse.Ok("Card template archived."));
    }

    // UC10: Delete card (if no transactions)
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCard(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCardCommand(id), ct);
        return Ok(ApiResponse.Ok("Card template deleted."));
    }
}
