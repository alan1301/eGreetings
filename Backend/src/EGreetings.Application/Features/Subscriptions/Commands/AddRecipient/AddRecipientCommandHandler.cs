using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace EGreetings.Application.Features.Subscriptions.Commands.AddRecipient;

public class AddRecipientCommandHandler : IRequestHandler<AddRecipientCommand, int>
{
    private readonly IApplicationDbContext _context;

    public AddRecipientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AddRecipientCommand request, CancellationToken cancellationToken)
    {
        // Verify subscription belongs to user
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == request.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Gói Subscribe không tồn tại hoặc không có quyền truy cập.");

        // BR-20: Check max recipients
        var currentCount = await _context.SubscriptionRecipients
            .CountAsync(r => r.SubscriptionId == request.SubscriptionId && r.IsActive, cancellationToken);

        if (currentCount >= subscription.MaxRecipients)
            throw new InvalidOperationException($"Đã đạt giới hạn {subscription.MaxRecipients} người nhận.");

        // BR-21: Validate email
        if (!IsValidEmail(request.Email))
            throw new InvalidOperationException("Email không hợp lệ.");

        // BR-14: Minimum 10 recipients check (if applicable)
        // This is enforced at subscription creation level

        var recipient = new SubscriptionRecipient
        {
            SubscriptionId = request.SubscriptionId,
            Name = request.Name,
            Email = request.Email,
            Birthday = request.Birthday,
            IsActive = true
        };

        _context.SubscriptionRecipients.Add(recipient);
        await _context.SaveChangesAsync(cancellationToken);

        return recipient.Id;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
        catch
        {
            return false;
        }
    }
}
