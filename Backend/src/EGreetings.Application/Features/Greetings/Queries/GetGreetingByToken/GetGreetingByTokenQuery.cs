using EGreetings.Application.Features.Greetings.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Greetings.Queries.GetGreetingByToken;

/// <summary>UC09 - Xem thiệp (người nhận qua link) | UC24 - Xem chi tiết thiệp (Guest)</summary>
public record GetGreetingByTokenQuery(string Token) : IRequest<GreetingDetailDto>;
