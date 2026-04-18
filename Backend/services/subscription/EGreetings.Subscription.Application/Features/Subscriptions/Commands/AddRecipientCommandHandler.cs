using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Domain.Entities;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Commands;

public class AddRecipientCommandHandler : IRequestHandler<AddRecipientCommand, int>
{
    private readonly ISubscriptionDbContext _context;

    public AddRecipientCommandHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AddRecipientCommand request, CancellationToken cancellationToken)
    {
        // Check ownership
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == request.UserId, cancellationToken);

        if (subscription == null)
        {
            throw new InvalidOperationException("Subscription not found or access denied");
        }

        // BR-20: Check < MaxRecipients
        var currentRecipientCount = await _context.SubscriptionRecipients
            .CountAsync(r => r.SubscriptionId == request.SubscriptionId && r.IsActive, cancellationToken);

        if (currentRecipientCount >= subscription.MaxRecipients)
        {
            throw new InvalidOperationException($"Maximum recipients limit ({subscription.MaxRecipients}) reached");
        }

        // BR-21: Email unique in subscription
        var existingEmail = await _context.SubscriptionRecipients
            .FirstOrDefaultAsync(r => r.SubscriptionId == request.SubscriptionId && r.Email == request.Email, cancellationToken);

        if (existingEmail != null)
        {
            throw new InvalidOperationException("Email already exists for this subscription");
        }

        // BR-14: Validate email format
        if (!IsValidEmail(request.Email))
        {
            throw new InvalidOperationException("Invalid email format");
        }

        var recipient = new SubscriptionRecipient
        {
            SubscriptionId = request.SubscriptionId,
            Email = request.Email,
            Name = request.Name,
            Birthday = request.Birthday,
            IsActive = true
        };

        await _context.SubscriptionRecipients.AddAsync(recipient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return recipient.Id.GetHashCode();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
