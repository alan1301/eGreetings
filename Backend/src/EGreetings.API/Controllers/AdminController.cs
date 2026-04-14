using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Admin.Commands.BanUser;
using EGreetings.Application.Features.Admin.Commands.ConfirmPayment;
using EGreetings.Application.Features.Admin.Commands.DisableSubscription;
using EGreetings.Application.Features.Admin.Commands.ManageCategory;
using EGreetings.Application.Features.Admin.Commands.ManageWebContent;
using EGreetings.Application.Features.Admin.Commands.RollbackWebContent;
using EGreetings.Application.Features.Admin.Queries.GetAuditLogs;
using EGreetings.Application.Features.Admin.Queries.GetCategories;
using EGreetings.Application.Features.Admin.Queries.GetContentVersions;
using EGreetings.Application.Features.Admin.Queries.GetDashboard;
using EGreetings.Application.Features.Admin.Queries.GetGreetings;
using EGreetings.Application.Features.Admin.Queries.GetPayments;
using EGreetings.Application.Features.Admin.Queries.GetSubscriptions;
using EGreetings.Application.Features.Admin.Queries.GetUsers;
using EGreetings.Application.Features.Admin.Queries.GetWebContent;
using EGreetings.Application.Features.Categories.DTOs;
using EGreetings.Application.Features.Feedback.Commands.MarkFeedbackRead;
using EGreetings.Application.Features.Feedback.Commands.ReplyFeedback;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC14 - Quản lý User | UC21 - Quản lý User/Subscribe (Admin)
/// UC27 - Quản lý danh mục | UC28 - Quản lý nội dung website
/// UC30 - Logging & Audit
/// ✅ CQRS compliant: tất cả actions dùng MediatR, không inject DbContext trực tiếp
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AdminController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // === UC14/UC21 - Quản lý Users ===

    /// <summary>UC14 - Xem danh sách người dùng</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUsersQuery(search, page, pageSize), ct);
        return Ok(new { Success = true, Data = result.Items, Total = result.Total });
    }

    /// <summary>UC21 A4 - Ban/Unban User → kéo theo Disable Subscription</summary>
    [HttpPost("users/{id}/ban")]
    public async Task<IActionResult> BanUser(int id, [FromBody] BanUserCommand command, CancellationToken ct)
    {
        var cmd = command with { TargetUserId = id };
        await _mediator.Send(cmd, ct);
        return Ok(new { Success = true, Message = command.IsBanning ? "User đã bị ban." : "User đã được mở ban." });
    }

    // === UC27 - Quản lý danh mục ===

    /// <summary>UC27 - Xem danh sách danh mục (Admin)</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var categories = await _mediator.Send(new GetCategoriesQuery(), ct);
        return Ok(new { Success = true, Data = categories });
    }

    /// <summary>UC27 - Tạo danh mục mới</summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(request.Name, request.Description, request.IconUrl), ct);
        return CreatedAtAction(nameof(GetCategories), new { id = result.Id },
            new { Success = true, Data = result, Message = "Danh mục đã được tạo." });
    }

    /// <summary>UC27 - Cập nhật danh mục</summary>
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand(id, request.Name, request.Description, request.IconUrl, request.IsActive), ct);
        return Ok(new { Success = true, Data = result, Message = "Danh mục đã được cập nhật." });
    }

    /// <summary>UC27 - Xóa danh mục (soft delete) - BR-31</summary>
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), ct);
        return Ok(new { Success = true, Message = "Danh mục đã bị vô hiệu hóa." });
    }

    // === UC21 - Quản lý gói Subscribe ===

    /// <summary>UC21 - Xem danh sách gói Subscribe</summary>
    [HttpGet("subscriptions")]
    public async Task<IActionResult> GetSubscriptions(
        [FromQuery] string? status,
        [FromQuery] int? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllSubscriptionsQuery(status, userId, page, pageSize), ct);
        return Ok(new { Success = true, Data = result.Items, Total = result.Total });
    }

    /// <summary>UC21 - Vô hiệu hóa gói Subscribe</summary>
    [HttpPost("subscriptions/{id}/disable")]
    public async Task<IActionResult> DisableSubscription(int id, [FromBody] DisableSubscriptionRequest request, CancellationToken ct)
    {
        var adminId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        await _mediator.Send(new DisableSubscriptionCommand(id, adminId, request.Reason), ct);
        return Ok(new { Success = true, Message = "Gói Subscribe đã được vô hiệu hóa." });
    }

    // === UC12 - Báo cáo thiệp ===

    /// <summary>UC12 - Xem báo cáo thiệp</summary>
    [HttpGet("greetings")]
    public async Task<IActionResult> GetGreetings(
        [FromQuery] int? userId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllGreetingsQuery(userId, status, page, pageSize), ct);
        return Ok(new { Success = true, Data = result.Items, Total = result.Total });
    }

    // === UC11 - Quản lý phản hồi ===

    /// <summary>UC11 - Trả lời phản hồi</summary>
    [HttpPost("feedback/{id}/reply")]
    public async Task<IActionResult> ReplyFeedback(int id, [FromBody] ReplyFeedbackRequest request, CancellationToken ct)
    {
        var adminId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        await _mediator.Send(new ReplyFeedbackCommand(id, adminId, request.Reply), ct);
        return Ok(new { Success = true, Message = "Trả lời phản hồi thành công." });
    }

    /// <summary>UC11 - Đánh dấu phản hồi đã đọc</summary>
    [HttpPatch("feedback/{id}/read")]
    public async Task<IActionResult> MarkFeedbackRead(int id, CancellationToken ct)
    {
        await _mediator.Send(new MarkFeedbackReadCommand(id), ct);
        return Ok(new { Success = true, Message = "Phản hồi đã được đánh dấu đã đọc." });
    }

    // === UC13 - Quản lý thanh toán ===

    /// <summary>UC13 - Xem danh sách thanh toán</summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPaymentsQuery(status, page, pageSize), ct);
        return Ok(new { Success = true, Data = result.Items, Total = result.Total });
    }

    /// <summary>UC13 - Xác nhận thanh toán</summary>
    [HttpPost("payments/{id}/confirm")]
    public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmPaymentRequest request, CancellationToken ct)
    {
        var adminId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        await _mediator.Send(new ConfirmPaymentCommand(id, adminId, request.TransactionCode), ct);
        return Ok(new { Success = true, Message = "Thanh toán đã được xác nhận. Gói Subscribe đã kích hoạt." });
    }

    // === UC28 - Quản lý nội dung website ===

    /// <summary>UC28 - Cập nhật nội dung website (Banner, Footer, About)</summary>
    [HttpPut("content/{key}")]
    public async Task<IActionResult> UpdateContent(string key, [FromBody] UpdateWebContentCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");
        var cmd = command with { Key = key, UpdatedByUserId = userId };
        var newId = await _mediator.Send(cmd, ct);
        return Ok(new { Success = true, Data = new { Id = newId }, Message = "Nội dung đã được cập nhật." });
    }

    /// <summary>UC28 - Lấy nội dung website theo key</summary>
    [HttpGet("content/{key}")]
    public async Task<IActionResult> GetWebContent(string key, CancellationToken ct)
    {
        var content = await _mediator.Send(new GetWebContentQuery(key), ct);
        return Ok(new { Success = true, Data = content });
    }

    /// <summary>UC28 - Xem lịch sử phiên bản nội dung (để rollback)</summary>
    [HttpGet("content/{key}/versions")]
    public async Task<IActionResult> GetContentVersions(string key, CancellationToken ct)
    {
        var versions = await _mediator.Send(new GetContentVersionsQuery(key), ct);
        return Ok(new { Success = true, Data = versions });
    }

    /// <summary>UC28 - Rollback nội dung website về phiên bản cũ</summary>
    [HttpPost("content/{key}/rollback/{versionId}")]
    public async Task<IActionResult> RollbackWebContent(string key, int versionId, CancellationToken ct)
    {
        var adminId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        await _mediator.Send(new RollbackWebContentCommand(key, versionId, adminId), ct);
        return Ok(new { Success = true, Message = "Rollback nội dung thành công." });
    }

    // === UC30 - Logging & Audit ===

    /// <summary>UC30 - Xem audit logs - BR-33</summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityName,
        [FromQuery] int? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(entityName, userId, from, to, page, pageSize), ct);
        return Ok(new { Success = true, Data = result.Items, Total = result.Total });
    }

    /// <summary>UC30 - Export audit logs as CSV</summary>
    [HttpGet("audit-logs/export")]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(null, null, from, to, 1, int.MaxValue), ct);
        var logs = result.Items;

        var csv = "Id,EventType,EntityName,EntityId,Description,UserId,IpAddress,IsSystemAction,CreatedAt\n";
        csv += string.Join("\n", logs.Select(a =>
            $"{a.Id},{a.EventType},{a.EntityName},{a.EntityId},\"{a.Description}\",{a.UserId},{a.IpAddress},{a.IsSystemAction},{a.CreatedAt:yyyy-MM-dd HH:mm:ss}"));

        var bytes = Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"audit-logs-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>UC14 - Xem thống kê tổng quan (Dashboard)</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var stats = await _mediator.Send(new GetDashboardQuery(), ct);
        return Ok(new { Success = true, Data = stats });
    }
}

public record CreateCategoryRequest(string Name, string? Description, string? IconUrl);
public record UpdateCategoryRequest(string? Name, string? Description, string? IconUrl, bool? IsActive);
public record DisableSubscriptionRequest(string? Reason);
public record ConfirmPaymentRequest(string? TransactionCode);
public record ReplyFeedbackRequest(string Reply);
