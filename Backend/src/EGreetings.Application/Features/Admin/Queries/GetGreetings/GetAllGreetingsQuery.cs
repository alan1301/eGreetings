using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Greetings.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Queries.GetGreetings;

/// <summary>UC12 - Xem báo cáo thiệp (Admin)</summary>
public record GetAllGreetingsQuery(
    int? UserId,
    string? Status,
    int Page,
    int PageSize
) : IRequest<PagedResult<GreetingDto>>;
