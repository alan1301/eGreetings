using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EGreetings.User.Domain.Entities;
using EGreetings.Shared.Contracts.Events.Identity;

namespace EGreetings.User.Infrastructure.Messaging.Consumers;

public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly UserDbContext _context;
    private readonly ILogger<UserRegisteredEventConsumer> _logger;

    public UserRegisteredEventConsumer(UserDbContext context, ILogger<UserRegisteredEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        try
        {
            // Check if profile already exists
            var existingProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == context.Message.UserId);

            if (existingProfile != null)
            {
                _logger.LogInformation("User profile already exists for user {UserId}", context.Message.UserId);
                return;
            }

            var profile = new UserProfile
            {
                UserId = context.Message.UserId,
                Email = context.Message.Email,
                FullName = context.Message.FullName
            };

            await _context.UserProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created user profile for user {UserId}", context.Message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserRegisteredEvent for user {UserId}", context.Message.UserId);
            throw;
        }
    }
}
