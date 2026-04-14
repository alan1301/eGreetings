using MediatR;

namespace EGreetings.Application.Features.Admin.Commands.BanUser;

/// <summary>UC21 A4 - Ban/Unban User (Admin) - kéo theo Disable Subscription</summary>
public record BanUserCommand(int TargetUserId, bool IsBanning, string? Reason) : IRequest<bool>;
