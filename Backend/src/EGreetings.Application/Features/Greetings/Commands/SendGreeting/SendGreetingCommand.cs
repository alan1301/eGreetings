using EGreetings.Application.Features.Greetings.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Greetings.Commands.SendGreeting;

/// <summary>
/// UC06 - Gửi thiệp (User + Guest UC08)
/// UC20 - Gửi thiệp tự động (triggered by background service)
/// BR-28: Reply-To = email người gửi
/// </summary>
public record SendGreetingCommand(
    int? UserId,                  // null nếu Guest
    int TemplateId,
    string RecipientEmail,
    string RecipientName,
    string SenderMessage,
    string CustomHtml,
    DateTime? ScheduledAt,
    string? GuestSenderEmail,
    string? GuestSenderName,
    bool IsAutoSend = false       // UC20
) : IRequest<GreetingDto>;
