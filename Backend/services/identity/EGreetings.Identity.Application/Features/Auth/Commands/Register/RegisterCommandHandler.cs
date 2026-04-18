using EGreetings.Identity.Application.Common.Interfaces;
using EGreetings.Identity.Domain.Entities;
using EGreetings.Identity.Domain.Enums;
using EGreetings.Shared.Contracts.Events.Identity;
using MassTransit;
using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, int>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterCommandHandler(IIdentityDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<int> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // BR-01: Validate email unique
        var existingUser = _dbContext.Users.FirstOrDefault(u => u.Email == request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email '{request.Email}' already exists.");
        }

        // BR-02: Validate password complexity (>=8 chars, has uppercase, digit, special)
        if (request.Password.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters long.");
        }

        if (!request.Password.Any(char.IsUpper))
        {
            throw new InvalidOperationException("Password must contain at least one uppercase letter.");
        }

        if (!request.Password.Any(char.IsDigit))
        {
            throw new InvalidOperationException("Password must contain at least one digit.");
        }

        if (!request.Password.Any(c => !char.IsLetterOrDigit(c)))
        {
            throw new InvalidOperationException("Password must contain at least one special character.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException("Passwords do not match.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Phone = request.Phone ?? string.Empty,
            Role = UserRole.User,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            EmailVerificationToken = Guid.NewGuid().ToString(),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new UserRegisteredEvent(user.Id, user.Email, user.FullName, DateTime.UtcNow),
            cancellationToken);

        return user.Id;
    }
}
