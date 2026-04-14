using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Greetings.DTOs;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Greetings.Commands.SendGreeting;

public class SendGreetingCommandHandler : IRequestHandler<SendGreetingCommand, GreetingDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IAuditService _auditService;

    public SendGreetingCommandHandler(IUnitOfWork uow,
        IPublishEndpoint publishEndpoint, IAuditService auditService)
    {
        _uow = uow;
        _publishEndpoint = publishEndpoint;
        _auditService = auditService;
    }

    public async Task<GreetingDto> Handle(SendGreetingCommand request, CancellationToken cancellationToken)
    {
        // --- Template lookup ---
        var template = await _uow.Repository<GreetingTemplate>()
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Mẫu thiệp không tồn tại hoặc đã bị vô hiệu hóa.");

        // --- Quyền dùng template trả phí ---
        if (!template.IsFree && request.UserId == null)
            throw new InvalidOperationException("Mẫu thiệp này yêu cầu đăng nhập và Subscribe.");

        bool hasActiveSubscription = false;
        if (!template.IsFree && request.UserId.HasValue)
        {
            hasActiveSubscription = await _uow.Repository<Subscription>()
                .AnyAsync(s => s.UserId == request.UserId.Value
                    && s.Status == SubscriptionStatus.Active
                    && s.ExpiredAt > DateTime.UtcNow, cancellationToken);

            if (!hasActiveSubscription)
                throw new InvalidOperationException("Bạn cần có gói Subscribe đang hoạt động để gửi mẫu thiệp trả phí.");
        }

        // --- BR-10: Tối đa 50 thiệp/ngày cho tài khoản không Subscribe ---
        if (request.UserId.HasValue && !request.IsAutoSend)
        {
            if (!hasActiveSubscription)
            {
                // Xác nhận lại nếu chưa kiểm tra (template free + user đăng nhập)
                hasActiveSubscription = await _uow.Repository<Subscription>()
                    .AnyAsync(s => s.UserId == request.UserId.Value
                        && s.Status == SubscriptionStatus.Active
                        && s.ExpiredAt > DateTime.UtcNow, cancellationToken);
            }

            if (!hasActiveSubscription)
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);
                var sentToday = await _uow.Repository<Greeting>()
                    .CountAsync(g => g.UserId == request.UserId.Value
                        && g.SentAt >= today
                        && g.SentAt < tomorrow, cancellationToken);

                if (sentToday >= 50)
                    throw new InvalidOperationException("BR-10: Bạn đã đạt giới hạn 50 thiệp/ngày. Nâng cấp Subscribe để gửi không giới hạn.");
            }
        }

        // --- Xác định replyTo - BR-28 ---
        string? replyTo = null;
        if (request.UserId.HasValue)
        {
            var user = await _uow.Repository<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId.Value, cancellationToken);
            replyTo = user?.Email;
        }
        else
        {
            replyTo = request.GuestSenderEmail;
        }

        var greeting = new Greeting
        {
            UserId = request.UserId,
            TemplateId = request.TemplateId,
            GuestSenderEmail = request.GuestSenderEmail,
            GuestSenderName = request.GuestSenderName,
            SenderMessage = request.SenderMessage,
            CustomHtml = string.IsNullOrEmpty(request.CustomHtml) ? template.HtmlContent : request.CustomHtml,
            RecipientEmail = request.RecipientEmail,
            RecipientName = request.RecipientName,
            ReplyToEmail = replyTo,
            Status = request.ScheduledAt.HasValue ? GreetingStatus.Scheduled : GreetingStatus.Draft,
            ScheduledAt = request.ScheduledAt
        };

        _uow.Repository<Greeting>().Add(greeting);
        template.UsageCount++;
        _uow.Repository<GreetingTemplate>().Update(template);
        await _uow.SaveChangesAsync(cancellationToken);

        // --- Gửi ngay nếu không lên lịch ---
        if (!request.ScheduledAt.HasValue)
        {
            await _publishEndpoint.Publish(new EGreetings.Shared.Contracts.Events.Greeting.GreetingSentEvent(
                greeting.Id,
                greeting.UserId,
                greeting.RecipientEmail,
                template.Name,
                DateTime.UtcNow
            ), cancellationToken);

            greeting.Status = GreetingStatus.Sent;
            greeting.SentAt = DateTime.UtcNow;
            _uow.Repository<Greeting>().Update(greeting);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        await _auditService.LogAsync(
            request.IsAutoSend ? AuditEventType.AutoSendTriggered : AuditEventType.GreetingSent,
            "Greeting", greeting.Id.ToString(),
            $"Thiệp gửi tới {request.RecipientEmail}", cancellationToken: cancellationToken);

        return new GreetingDto(greeting.Id, template.Id, template.Name, greeting.RecipientName,
            greeting.RecipientEmail, greeting.SenderMessage, greeting.Status.ToString(),
            greeting.ScheduledAt, greeting.SentAt, greeting.ViewCount,
            greeting.UniqueToken, greeting.CreatedAt);
    }
}
