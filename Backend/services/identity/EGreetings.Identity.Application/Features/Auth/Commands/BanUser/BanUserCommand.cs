using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.BanUser;

public class BanUserCommand : IRequest<Unit>
{
    public int TargetUserId { get; set; }
    public bool IsBanning { get; set; }
    public string? Reason { get; set; }
    public int AdminUserId { get; set; }
}
