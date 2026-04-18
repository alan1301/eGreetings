using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EGreetings.Subscription.Application.DTOs;
using EGreetings.Subscription.Application.Features.Admin;
using EGreetings.Subscription.Application.Features.Subscriptions.Commands;
using EGreetings.Subscription.Application.Features.Subscriptions.Queries;

namespace EGreetings.Subscription.API.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId()
    {
        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        return int.TryParse(userIdHeader, out var userId) ? userId : 0;
    }

    private bool IsAdmin()
    {
        var roleHeader = HttpContext.Request.Headers["X-UserRole"].ToString();
        return roleHeader.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }

    [HttpGet("plans")]
    public async Task<ActionResult<List<PlanDto>>> GetPlans()
    {
        var result = await _mediator.Send(new GetPlansQuery());
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDetailDto?>> GetMySubscription()
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var subscription = await _mediator.Send(new GetMySubscriptionQuery(userId));
        return Ok(subscription);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CreateSubscriptionResponse>> CreateSubscription([FromBody] CreateSubscriptionCommand command)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var result = await _mediator.Send(new CreateSubscriptionCommand(userId, command.PlanId, command.PaymentMethod));
        return CreatedAtAction(nameof(GetMySubscription), result);
    }

    [HttpPost("{id}/renew")]
    [Authorize]
    public async Task<ActionResult<CreateSubscriptionResponse>> RenewSubscription(int id, [FromBody] RenewSubscriptionCommand command)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var result = await _mediator.Send(new RenewSubscriptionCommand(id, userId, command.PaymentMethod));
        return Ok(result);
    }

    [HttpGet("{id}/recipients")]
    [Authorize]
    public async Task<ActionResult<List<SubscriptionRecipientDto>>> GetRecipients(int id)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var recipients = await _mediator.Send(new GetRecipientsQuery(userId, id));
        return Ok(recipients);
    }

    [HttpPost("{id}/recipients")]
    [Authorize]
    public async Task<ActionResult<int>> AddRecipient(int id, [FromBody] AddRecipientCommand command)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var result = await _mediator.Send(new AddRecipientCommand(userId, id, command.Name, command.Email, command.Birthday));
        return CreatedAtAction(nameof(GetRecipients), new { id }, result);
    }

    [HttpDelete("{id}/recipients/{recipientId}")]
    [Authorize]
    public async Task<IActionResult> RemoveRecipient(int id, int recipientId)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        await _mediator.Send(new RemoveRecipientCommand(userId, id, recipientId));
        return NoContent();
    }

    [HttpGet("payments")]
    [Authorize]
    public async Task<ActionResult<List<PaymentDto>>> GetPaymentHistory()
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var payments = await _mediator.Send(new GetPaymentHistoryQuery(userId));
        return Ok(payments);
    }

    [HttpGet("admin")]
    [Authorize]
    public async Task<ActionResult<PagedResult<SubscriptionDto>>> GetAllSubscriptions(
        [FromQuery] string? status, [FromQuery] int? userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (!IsAdmin()) return Forbid();

        var result = await _mediator.Send(new GetAllSubscriptionsQuery(status, userId, page, pageSize));
        return Ok(result);
    }

    [HttpPost("admin/{id}/disable")]
    [Authorize]
    public async Task<IActionResult> DisableSubscription(int id, [FromBody] DisableSubscriptionCommand command)
    {
        if (!IsAdmin()) return Forbid();

        var adminId = GetUserId();
        if (adminId <= 0) return Unauthorized();

        await _mediator.Send(new DisableSubscriptionCommand(id, adminId, command.Reason));
        return NoContent();
    }

    [HttpGet("admin/payments")]
    [Authorize]
    public async Task<ActionResult<PagedResult<PaymentDto>>> GetPayments(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (!IsAdmin()) return Forbid();

        var result = await _mediator.Send(new GetPaymentsQuery(status, page, pageSize));
        return Ok(result);
    }

    [HttpPost("admin/payments/{id}/confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmPaymentCommand command)
    {
        if (!IsAdmin()) return Forbid();

        var adminId = GetUserId();
        if (adminId <= 0) return Unauthorized();

        await _mediator.Send(new ConfirmPaymentCommand(id, adminId, command.TransactionCode));
        return NoContent();
    }
}
