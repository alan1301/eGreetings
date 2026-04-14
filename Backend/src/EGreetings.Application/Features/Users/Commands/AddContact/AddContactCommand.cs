using MediatR;

namespace EGreetings.Application.Features.Users.Commands.AddContact;

/// <summary>UC16 - Thêm liên hệ vào danh bạ</summary>
public record AddContactCommand(
    int UserId,
    string Name,
    string Email,
    string? Phone,
    DateTime? Birthday,
    string? Note,
    string? Group        // UC16: Gia đình / Bạn bè / Đồng nghiệp
) : IRequest<int>;
