using EGreetings.Application.Features.Auth.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Auth.Commands.Register;

/// <summary>UC01 - Đăng ký tài khoản</summary>
public record RegisterCommand(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    string? Phone
) : IRequest<AuthResponseDto>;
