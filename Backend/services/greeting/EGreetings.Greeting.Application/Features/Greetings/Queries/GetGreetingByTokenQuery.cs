using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Greetings.Queries;

public record GetGreetingByTokenQuery(string Token) : IRequest<GreetingDto>;
