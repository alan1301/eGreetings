using EGreetings.Application.Commands.Admin.CardDesign;
using EGreetings.Application.Queries.CardDesign;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// Public endpoints for card backgrounds and decorations (used by Personalize component).
/// Admin CRUD endpoints for managing design assets.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public class CardDesignController : ControllerBase
{
    private readonly IMediator _mediator;
    public CardDesignController(IMediator mediator) => _mediator = mediator;

    // ── Public: list active backgrounds ──────────────────────────────
    [HttpGet("card-backgrounds")]
    public async Task<IActionResult> GetBackgrounds()
    {
        var items = await _mediator.Send(new GetCardBackgroundsQuery(AdminMode: false));
        return Ok(ApiResponse<object>.Ok(items));
    }

    // ── Public: list active decorations ──────────────────────────────
    [HttpGet("card-decorations")]
    public async Task<IActionResult> GetDecorations()
    {
        var items = await _mediator.Send(new GetCardDecorationsQuery(AdminMode: false));
        return Ok(ApiResponse<object>.Ok(items));
    }

    // ════════════════════════════════════════════════════════════════
    // ADMIN – Backgrounds
    // ════════════════════════════════════════════════════════════════

    [HttpGet("admin/card-backgrounds")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminGetBackgrounds()
    {
        var items = await _mediator.Send(new GetCardBackgroundsQuery(AdminMode: true));
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpPost("admin/card-backgrounds")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminCreateBackground([FromBody] CreateCardBackgroundRequest req)
    {
        var id = await _mediator.Send(new CreateCardBackgroundCommand(
            req.Id, req.Label, req.BgStyle, req.Categories, req.IsPremium, req.SortOrder));
        return Ok(ApiResponse<object>.Ok(new { id }));
    }

    [HttpPut("admin/card-backgrounds/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminUpdateBackground(string id, [FromBody] UpdateCardBackgroundRequest req)
    {
        await _mediator.Send(new UpdateCardBackgroundCommand(
            id, req.Label, req.BgStyle, req.Categories, req.IsPremium, req.IsActive, req.SortOrder));
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPatch("admin/card-backgrounds/{id}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminToggleBackground(string id)
    {
        var isActive = await _mediator.Send(new ToggleCardBackgroundCommand(id));
        return Ok(ApiResponse<object>.Ok(new { isActive }));
    }

    [HttpDelete("admin/card-backgrounds/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDeleteBackground(string id)
    {
        await _mediator.Send(new DeleteCardBackgroundCommand(id));
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    // ════════════════════════════════════════════════════════════════
    // ADMIN – Decorations
    // ════════════════════════════════════════════════════════════════

    [HttpGet("admin/card-decorations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminGetDecorations()
    {
        var items = await _mediator.Send(new GetCardDecorationsQuery(AdminMode: true));
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpPost("admin/card-decorations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminCreateDecoration([FromBody] CreateCardDecorationRequest req)
    {
        var id = await _mediator.Send(new CreateCardDecorationCommand(
            req.Id, req.Label, req.Preview, req.Categories, req.Elements, req.SortOrder));
        return Ok(ApiResponse<object>.Ok(new { id }));
    }

    [HttpPut("admin/card-decorations/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminUpdateDecoration(string id, [FromBody] UpdateCardDecorationRequest req)
    {
        await _mediator.Send(new UpdateCardDecorationCommand(
            id, req.Label, req.Preview, req.Categories, req.Elements, req.IsActive, req.SortOrder));
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPatch("admin/card-decorations/{id}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminToggleDecoration(string id)
    {
        var isActive = await _mediator.Send(new ToggleCardDecorationCommand(id));
        return Ok(ApiResponse<object>.Ok(new { isActive }));
    }

    [HttpDelete("admin/card-decorations/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDeleteDecoration(string id)
    {
        await _mediator.Send(new DeleteCardDecorationCommand(id));
        return Ok(ApiResponse<object>.Ok(new { }));
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────

public record CreateCardBackgroundRequest(
    string Id, string Label, string BgStyle,
    string Categories, bool IsPremium = false, int SortOrder = 0);

public record UpdateCardBackgroundRequest(
    string Label, string BgStyle, string Categories,
    bool IsPremium, bool IsActive, int SortOrder);

public record CreateCardDecorationRequest(
    string Id, string Label, string Preview,
    string Categories, string Elements, int SortOrder = 0);

public record UpdateCardDecorationRequest(
    string Label, string Preview, string Categories,
    string Elements, bool IsActive, int SortOrder);
