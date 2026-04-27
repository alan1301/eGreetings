using EGreetings.Application.Commands.Cards.SendGreeting;
using EGreetings.Application.Commands.Cards.ScheduleGreeting;
using EGreetings.Application.Commands.Cards.CreateDraft;
using EGreetings.Application.Commands.Cards.AutoSaveDraft;
using EGreetings.Application.Queries.Cards;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC03, UC04, UC05, UC06, UC17, UC23, UC24 – Greeting cards, drafts, history.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public class CardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CardsController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    // ── UC03 / UC23 – List cards (public) ──────────────────────
    [HttpGet("cards")]
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

    // ── UC04 / UC24 – Card detail (public) ────────────────────
    [HttpGet("cards/{id:guid}")]
    public async Task<IActionResult> GetCard(Guid id, CancellationToken ct)
    {
        var card = await _mediator.Send(new GetCardByIdQuery(id), ct);
        if (card == null) return NotFound(ApiResponse.Fail("Không tìm thấy mẫu thiệp."));
        return Ok(ApiResponse<CardDetailDto>.Ok(card));
    }

    // ── UC03 – Categories list ──────────────────────────────────
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), ct);
        return Ok(ApiResponse<List<CategoryDto>>.Ok(result));
    }

    // ── UC05 – Create draft ────────────────────────────────────
    [HttpPost("drafts")]
    [Authorize]
    public async Task<IActionResult> CreateDraft([FromBody] CreateDraftCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = CurrentUserId };
        var id = await _mediator.Send(cmd, ct);
        return Created($"/api/drafts/{id}", ApiResponse<object>.Created(new { id }));
    }

    // ── UC05 – Auto-save draft (BR-09) ─────────────────────────
    [HttpPatch("drafts/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> AutoSaveDraft(Guid id, [FromBody] AutoSaveDraftCommand command, CancellationToken ct)
    {
        var cmd = command with { DraftId = id, UserId = CurrentUserId };
        await _mediator.Send(cmd, ct);
        return Ok(ApiResponse.Ok("Bản nháp đã lưu."));
    }

    // ── UC17 – My drafts ───────────────────────────────────────
    [HttpGet("me/drafts")]
    [Authorize]
    public async Task<IActionResult> MyDrafts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDraftsQuery(CurrentUserId, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<DraftDto>>.OkPaged(result, result.Meta));
    }

    // ── UC06 – Send greeting card ──────────────────────────────
    [HttpPost("greetings")]
    [Authorize]
    public async Task<IActionResult> SendGreeting([FromBody] SendGreetingCardCommand command, CancellationToken ct)
    {
        var cmd = command with { SenderId = CurrentUserId, SenderEmail = CurrentUserEmail };
        var txId = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<object>.Ok(new { transactionId = txId }, "Thiệp đã được gửi thành công!"));
    }

    // ── UC06 Alt – Schedule greeting ──────────────────────────
    [HttpPost("greetings/schedule")]
    [Authorize]
    public async Task<IActionResult> ScheduleGreeting([FromBody] ScheduleGreetingCardCommand command, CancellationToken ct)
    {
        var cmd = command with { SenderId = CurrentUserId, SenderEmail = CurrentUserEmail };
        var txId = await _mediator.Send(cmd, ct);
        return Accepted($"/api/greetings/{txId}", ApiResponse<object>.Ok(
            new { transactionId = txId }, "Thiệp sẽ được gửi theo lịch đã đặt."));
    }

    // ── UC17 – Greeting history ─────────────────────────────────
    [HttpGet("me/greeting-history")]
    [Authorize]
    public async Task<IActionResult> MyGreetingHistory(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetGreetingHistoryQuery(CurrentUserId, status, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<GreetingHistoryDto>>.OkPaged(result, result.Meta));
    }
}
