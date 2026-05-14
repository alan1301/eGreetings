using EGreetings.Application.Commands.Cards.AutoSaveDraft;
using EGreetings.Application.Commands.Cards.CreateDraft;
using EGreetings.Application.Commands.Cards.DeleteDraft;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC05, UC17 – Draft management (create, autosave, delete).
/// </summary>
[ApiController]
[Route("api/drafts")]
[Authorize]
[Produces("application/json")]
public class DraftsController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;

    public DraftsController(IMediator mediator) => _mediator = mediator;

    // UC05 – Create draft
    [HttpPost]
    public async Task<IActionResult> CreateDraft([FromBody] CreateDraftCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = CurrentUserId };
        var id = await _mediator.Send(cmd, ct);
        return Created($"/api/drafts/{id}", ApiResponse<object>.Created(new { id }));
    }

    // UC05 – Auto-save draft (BR-09)
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> AutoSaveDraft(Guid id, [FromBody] AutoSaveDraftCommand command, CancellationToken ct)
    {
        var cmd = command with { DraftId = id, UserId = CurrentUserId };
        await _mediator.Send(cmd, ct);
        return Ok(ApiResponse.Ok("Draft saved."));
    }

    // UC17 – Delete draft (called after send to clean up)
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDraft(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteDraftCommand(id, CurrentUserId), ct);
        return Ok(ApiResponse.Ok("Draft deleted."));
    }
}
