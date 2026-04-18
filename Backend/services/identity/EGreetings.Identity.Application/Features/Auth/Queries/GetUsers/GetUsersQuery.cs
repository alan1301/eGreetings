using EGreetings.Identity.Application.DTOs;
using EGreetings.Shared.Domain;
using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Queries.GetUsers;

public class GetUsersQuery : IRequest<PagedResult<UserSummaryDto>>
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
