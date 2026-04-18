using EGreetings.Identity.Application.Common.Interfaces;
using EGreetings.Identity.Application.DTOs;
using EGreetings.Identity.Domain.Enums;
using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IIdentityDbContext dbContext, IJwtService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        // BR-03: Check if email is verified
        if (!user.IsEmailVerified)
        {
            throw new InvalidOperationException("Email not verified. Please verify your email before logging in.");
        }

        // BR-04: Check lockout status
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Account is locked due to too many failed login attempts. Please try again later.");
        }

        // Check password
        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }
            _dbContext.SaveChangesAsync(cancellationToken).GetAwaiter().GetResult();
            throw new InvalidOperationException("Invalid email or password.");
        }

        // Reset failed attempts on success
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        _dbContext.SaveChangesAsync(cancellationToken).GetAwaiter().GetResult();

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(1); // Matching 60 minute default

        return Task.FromResult(new LoginResponse(
            Token: token,
            UserId: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role,
            ExpiresAt: expiresAt
        ));
    }
}
