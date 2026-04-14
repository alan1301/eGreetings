using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Subscriptions.Commands.AddRecipient;
using EGreetings.Application.Features.Subscriptions.Commands.CreateSubscription;
using EGreetings.Application.Features.Subscriptions.Commands.RemoveRecipient;
using EGreetings.Application.Features.Subscriptions.Commands.RenewSubscription;
using EGreetings.Application.Features.Subscriptions.Queries.GetMySubscription;
using EGreetings.Application.Features.Subscriptions.Queries.GetPaymentHistory;
using EGreetings.Application.Features.Subscriptions.Queries.GetRecipients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC10 - Đăng ký Subscribe | UC11 - Quản lý Subscribe
/// UC25 - Lịch sử thanh toán | UC26 - Gia hạn Subscribe
/// ✅ CQRS compliant: tất cả actions dùng MediatR, không inject DbContext trực tiếp
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public SubscriptionsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>UC11 - Xem gói Subscribe của tôi</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMySubscription(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        var subscription = await _mediator.Send(new GetMySubscriptionQuery(userId), ct);
        return Ok(new { Success = true, Data = subscription });
    }

    /// <summary>UC10 - Đăng ký gói Subscribe</summary>
    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] CreateSubscriptionCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");
        var cmd = command with { UserId = userId };
        var result = await _mediator.Send(cmd, ct);
        return Ok(new { Success = true, Data = result, Message = "Đăng ký gói thành công! Vui lòng thanh toán để kích hoạt." });
    }

    /// <summary>UC26 - Gia hạn Subscription - BR-30</summary>
    [HttpPost("{id}/renew")]
    public async Task<IActionResult> Renew(int id, [FromBody] string? paymentMethod, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");
        var result = await _mediator.Send(new RenewSubscriptionCommand(id, userId, paymentMethod), ct);
        return Ok(new { Success = true, Data = result, Message = "Gia hạn thành công!" });
    }

    /// <summary>UC11 - Xem danh sách người nhận</summary>
    [HttpGet("{id}/recipients")]
    public async Task<IActionResult> GetRecipients(int id, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        var recipients = await _mediator.Send(new GetRecipientsQuery(userId, id), ct);
        return Ok(new { Success = true, Data = recipients });
    }

    /// <summary>UC11 - Thêm người nhận - BR-14, BR-20, BR-21</summary>
    [HttpPost("{id}/recipients")]
    public async Task<IActionResult> AddRecipient(int id, [FromBody] AddRecipientRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        var recipientId = await _mediator.Send(new AddRecipientCommand(userId, id, request.Name, request.Email, request.Birthday), ct);
        return Ok(new { Success = true, Data = new { Id = recipientId }, Message = "Thêm người nhận thành công." });
    }

    /// <summary>UC11 - Xoá người nhận</summary>
    [HttpDelete("{id}/recipients/{recipientId}")]
    public async Task<IActionResult> RemoveRecipient(int id, int recipientId, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        await _mediator.Send(new RemoveRecipientCommand(userId, id, recipientId), ct);
        return Ok(new { Success = true, Message = "Xoá người nhận thành công." });
    }

    /// <summary>UC25 - Lịch sử thanh toán</summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetPaymentHistory(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        var payments = await _mediator.Send(new GetPaymentHistoryQuery(userId), ct);
        return Ok(new { Success = true, Data = payments });
    }
}

public record AddRecipientRequest(string Name, string Email, DateTime? Birthday);
