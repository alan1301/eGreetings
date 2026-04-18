using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Greetings.Commands;

public record SendGreetingCommand(
    int? UserId,
    int TemplateId,
    string RecipientEmail,
    string RecipientName,
    string? SenderMessage,
    string? CustomHtml,
    DateTime? ScheduledAt
) : IRequest<SendGreetingResponse>;
