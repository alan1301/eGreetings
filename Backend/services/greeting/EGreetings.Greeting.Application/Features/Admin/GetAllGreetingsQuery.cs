using EGreetings.Greeting.Application.Common.Models;
using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Admin;

public record GetAllGreetingsQuery(
    int? UserId,
    string? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<GreetingDto>>;
