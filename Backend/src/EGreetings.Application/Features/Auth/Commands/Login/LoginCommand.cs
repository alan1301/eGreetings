using EGreetings.Application.Features.Auth.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Auth.Commands.Login;

/// <summary>UC02 - Đăng nhập (User + Admin)</summary>
public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
