using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.DTOs;
using EGreetings.Greeting.Domain.Entities;
using EGreetings.Greeting.Domain.Enums;
using EGreetings.Shared.Contracts.Events.Greeting;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Greetings.Commands;

public class SendGreetingCommandHandler : IRequestHandler<SendGreetingCommand, SendGreetingResponse>
{
    private readonly IGreetingDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendGreetingCommandHandler(IGreetingDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<SendGreetingResponse> Handle(SendGreetingCommand request, CancellationToken cancellationToken)
    {
        // Validate template exists
        var template = await _context.GreetingTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Template with id {request.TemplateId} not found");

        // BR-09: Validate guest user can only send free templates
        if (request.UserId == null && !template.IsFree)
        {
            throw new InvalidOperationException("Guest users can only send free templates");
        }

        // Generate unique view token
        var viewToken = Guid.NewGuid().ToString("N");

        // Determine status
        var status = request.ScheduledAt.HasValue ? GreetingStatus.Scheduled : GreetingStatus.Pending;

        var greeting = new Domain.Entities.Greeting
        {
            UserId = request.UserId,
            TemplateId = request.TemplateId,
            RecipientEmail = request.RecipientEmail,
            RecipientName = request.RecipientName,
            SenderMessage = request.SenderMessage,
            CustomHtml = request.CustomHtml,
            ViewToken = viewToken,
            Status = status,
            ScheduledAt = request.ScheduledAt,
            SentAt = status == GreetingStatus.Pending ? DateTime.UtcNow : null,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Greetings.Add(greeting);

        // Increment template usage count
        template.UsageCount++;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Publish event only if immediately sent
        if (status == GreetingStatus.Pending)
        {
            var eventViewUrl = $"https://egreetings.vn/view/{greeting.ViewToken}";
            var @event = new GreetingSentEvent(
                greeting.Id,
                greeting.RecipientEmail,
                greeting.RecipientName,
                "E-Greetings",
                greeting.SenderMessage,
                eventViewUrl,
                DateTime.UtcNow
            );

            await _publishEndpoint.Publish(@event, cancellationToken);
        }

        var viewUrl = $"https://egreetings.vn/view/{viewToken}";

        return new SendGreetingResponse(
            greeting.Id,
            viewToken,
            viewUrl,
            greeting.ScheduledAt
        );
    }
}
