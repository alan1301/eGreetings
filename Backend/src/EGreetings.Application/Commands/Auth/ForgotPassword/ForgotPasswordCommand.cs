using EGreetings.Application.Interfaces;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EGreetings.Application.Commands.Auth.ForgotPassword;

/// <summary>UC22 – BR-26: Reset token TTL 15 min, single use.</summary>
public record ForgotPasswordCommand(string Email) : IRequest<Unit>;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly string _frontendUrl;

    public ForgotPasswordCommandHandler(IAppDbContext db, IEmailService emailService, IConfiguration configuration)
    {
        _db = db;
        _emailService = emailService;
        _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:4200";
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower() && !u.IsDeleted, ct);

        // UC22-E1: Do NOT reveal if email exists (security)
        if (user == null)
            return Unit.Value;

        // BR-26: Token TTL 15 minutes, single use
        var token = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(BusinessConstants.PasswordResetTokenTtlMinutes);
        user.PasswordResetTokenUsed = false;
        await _db.SaveChangesAsync(ct);

        var resetLink = $"{_frontendUrl}/reset-password?token={token}";
        await _emailService.SendAsync(new EmailMessage
        {
            To = user.Email,
            ToName = user.FullName,
            Subject = "Reset your E-Greetings password",
            HtmlBody = $"""
                <h2>Reset Your Password</h2>
                <p>Click the link below to set a new password (expires in 15 minutes):</p>
                <a href="{resetLink}">Reset Password</a>
                <p>If you did not request this, please ignore this email.</p>
            """,
            ReplyTo = "support@e-greetings.com"
        }, ct);

        return Unit.Value;
    }
}
