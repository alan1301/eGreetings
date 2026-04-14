using EGreetings.Application.Features.Greetings.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Greetings.Queries.GetUserGreetings;

/// <summary>UC07 - Xem thiệp đã gửi</summary>
public record GetUserGreetingsQuery(int UserId, int Page = 1, int PageSize = 10) : IRequest<List<GreetingDto>>;
