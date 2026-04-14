using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Users.Commands.AddContact;
using EGreetings.Application.Features.Users.Commands.ChangePassword;
using EGreetings.Application.Features.Users.Commands.SaveDraft;
using EGreetings.Application.Features.Users.Commands.UpdateProfile;
using EGreetings.Application.Features.Users.Queries.GetContacts;
using EGreetings.Application.Features.Users.Queries.GetDrafts;
using EGreetings.Application.Features.Users.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC19 - Hồ sơ cá nhân | UC16 - Quản lý danh bạ | UC17 - Lưu bản nháp
/// ✅ CQRS compliant: tất cả actions dùng MediatR, không inject DbContext trực tiếp
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>UC19 - Xem hồ sơ cá nhân</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        var profile = await _mediator.Send(new GetProfileQuery(userId), ct);
        return Ok(new { Success = true, Data = profile });
    }

    /// <summary>UC19 - Cập nhật hồ sơ cá nhân</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        await _mediator.Send(new UpdateProfileCommand(userId, request.FullName, request.Phone, request.AvatarUrl), ct);
        return Ok(new { Success = true, Message = "Hồ sơ đã được cập nhật." });
    }

    /// <summary>UC19 A1 - Đổi mật khẩu</summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        await _mediator.Send(new ChangePasswordCommand(userId, request.CurrentPassword,
            request.NewPassword, request.ConfirmPassword), ct);
        return Ok(new { Success = true, Message = "Mật khẩu đã được thay đổi thành công." });
    }

    /// <summary>UC16 - Xem danh bạ người nhận</summary>
    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var contacts = await _mediator.Send(new GetContactsQuery(userId), ct);
        return Ok(new { Success = true, Data = contacts });
    }

    /// <summary>UC16 - Thêm liên hệ vào danh bạ</summary>
    [HttpPost("contacts")]
    public async Task<IActionResult> AddContact(
        [FromBody] AddContactRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var id = await _mediator.Send(new AddContactCommand(
            userId, request.Name, request.Email, request.Phone, request.Birthday, request.Note, request.Group), ct);

        return Ok(new { Success = true, Data = new { Id = id }, Message = "Đã thêm liên hệ." });
    }

    /// <summary>UC17 - Xem danh sách bản nháp</summary>
    [HttpGet("drafts")]
    public async Task<IActionResult> GetDrafts(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var drafts = await _mediator.Send(new GetDraftsQuery(userId), ct);
        return Ok(new { Success = true, Data = drafts });
    }

    /// <summary>UC17 - Lưu bản nháp</summary>
    [HttpPost("drafts")]
    public async Task<IActionResult> SaveDraft([FromBody] SaveDraftRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var id = await _mediator.Send(new SaveDraftCommand(
            userId, request.TemplateId, request.Title,
            request.CustomHtml, request.RecipientEmail,
            request.RecipientName, request.SenderMessage), ct);

        return Ok(new { Success = true, Data = new { Id = id }, Message = "Bản nháp đã được lưu." });
    }
}

public record UpdateProfileRequest(string? FullName, string? Phone, string? AvatarUrl);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
public record AddContactRequest(string Name, string Email, string? Phone, DateTime? Birthday, string? Note, string? Group);
public record SaveDraftRequest(int TemplateId, string? Title, string? CustomHtml, string? RecipientEmail, string? RecipientName, string? SenderMessage);
