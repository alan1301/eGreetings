using EGreetings.Application.Queries.Cards;
using EGreetings.Infrastructure.Persistence;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.API.Controllers;

/// <summary>
/// /api/templates → alias of /api/cards to support Frontend2 which targets the old microservice API.
/// Also handles /api/greetings endpoints Frontend2 calls.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public class CompatController : ControllerBase
{
    private readonly IMediator _mediator;
    public CompatController(IMediator mediator) => _mediator = mediator;

    // ─── /api/templates → /api/cards (Frontend2 CardService calls /api/templates) ───

    /// <summary>GET /api/templates – same as GET /api/cards</summary>
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] string? category,
        [FromQuery] string? keyword,    // Frontend uses 'keyword', backend uses 'search'
        [FromQuery] string? search,
        [FromQuery] bool? isFree,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetCardsQuery(category, keyword ?? search, "newest", null, page, pageSize), ct);

        // Return shape matching ITemplateListResponse: { items, page, pageSize, totalCount, totalPages }
        return Ok(new
        {
            items = result.Items,
            page = result.Meta.Page,
            pageSize = result.Meta.PageSize,
            totalCount = result.Meta.Total,
            totalPages = (int)Math.Ceiling((double)result.Meta.Total / result.Meta.PageSize)
        });
    }

    /// <summary>GET /api/templates/:id – same as GET /api/cards/:id</summary>
    [HttpGet("templates/{id:guid}")]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken ct)
    {
        var card = await _mediator.Send(new GetCardByIdQuery(id), ct);
        if (card == null) return NotFound(ApiResponse.Fail("Không tìm thấy mẫu thiệp."));
        return Ok(card);
    }

    // ─── /api/greetings/my – alias of GET /api/me/greeting-history ─────────────
    // (Frontend2 GreetingService calls /api/greetings/my)
    [HttpGet("greetings/my")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> GetMyGreetings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _mediator.Send(
            new EGreetings.Application.Queries.Cards.GetGreetingHistoryQuery(userId, null, page, pageSize), ct);

        return Ok(new
        {
            items = result.Items,
            page = result.Meta.Page,
            pageSize = result.Meta.PageSize,
            totalCount = result.Meta.Total,
            totalPages = (int)Math.Ceiling((double)result.Meta.Total / result.Meta.PageSize)
        });
    }

    // ─── /api/greetings/send – send a greeting (Frontend2 GreetingService) ─────
    [HttpPost("greetings/send")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> SendGreeting(
        [FromBody] EGreetings.Application.Commands.Cards.SendGreeting.SendGreetingCardCommand command,
        CancellationToken ct)
    {
        var senderId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var senderEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
        var cmd = command with { SenderId = senderId, SenderEmail = senderEmail };
        var txId = await _mediator.Send(cmd, ct);
        return Ok(new { id = txId, viewUrl = $"/view/{txId}" });
    }

    // ─── /api/admin/dashboard – stub stats ──────────────────────────────────────
    [HttpGet("admin/dashboard")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        // Quick aggregate without a full query handler
        using var scope = HttpContext.RequestServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EGreetings.Infrastructure.Persistence.AppDbContext>();

        var totalUsers = await db.Users.CountAsync(ct);
        var activeSubscriptions = await db.Subscriptions
            .CountAsync(s => s.Status == EGreetings.Domain.Enums.SubscriptionStatus.Active, ct);
        var pendingPayments = await db.Subscriptions
            .CountAsync(s => s.Status == EGreetings.Domain.Enums.SubscriptionStatus.Pending, ct);
        var today = DateTime.UtcNow.Date;
        var greetingsSentToday = await db.GreetingTransactions
            .CountAsync(t => t.CreatedAt >= today, ct);
        var unreadFeedbacks = await db.Feedbacks
            .CountAsync(f => f.Status == EGreetings.Domain.Enums.FeedbackStatus.Unread, ct);

        return Ok(new
        {
            totalUsers,
            activeSubscriptions,
            pendingPayments,
            greetingsSentToday,
            unreadFeedbacks,
            revenueThisMonth = 0  // implement later with payment data
        });
    }

    // ─── /api/subscriptions/plans – stub list ───────────────────────────────────
    [HttpGet("subscriptions/plans")]
    public IActionResult GetPlans()
    {
        // Static plan definition (spec: single plan, min 10 emails, 30 days)
        return Ok(new[]
        {
            new
            {
                id = "standard",
                name = "Standard Subscribe",
                description = "Gửi thiệp tự động hàng ngày đến danh sách email. Tối thiểu 10 địa chỉ email.",
                pricePerEmail = 5000,     // VND per email per month
                minEmails = 10,
                durationDays = 30,
                currency = "VND"
            }
        });
    }

    // ─── /api/subscriptions/my – user's active subscription ─────────────────────
    [HttpGet("subscriptions/my")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> GetMySubscription(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        using var scope = HttpContext.RequestServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EGreetings.Infrastructure.Persistence.AppDbContext>();

        var sub = await db.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                id = s.Id,
                status = s.Status.ToString(),
                startDate = s.StartDate,
                expiryDate = s.ExpiryDate,
                paymentMethod = s.PaymentMethod.ToString()
            })
            .FirstOrDefaultAsync(ct);

        if (sub == null) return Ok((object?)null);
        return Ok(sub);
    }
}
