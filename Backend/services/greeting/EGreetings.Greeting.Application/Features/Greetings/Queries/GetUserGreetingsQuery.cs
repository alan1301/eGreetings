using EGreetings.Greeting.Application.Common.Models;
using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Greetings.Queries;

public record GetUserGreetingsQuery(
    int UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<GreetingDto>>;
