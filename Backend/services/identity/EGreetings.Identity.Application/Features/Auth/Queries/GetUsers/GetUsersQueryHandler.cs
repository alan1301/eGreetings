using EGreetings.Identity.Application.Common.Interfaces;
using EGreetings.Identity.Application.DTOs;
using EGreetings.Shared.Domain;
using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserSummaryDto>>
{
    private readonly IIdentityDbContext _dbContext;

    public GetUsersQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PagedResult<UserSummaryDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(searchLower) || u.Email.ToLower().Contains(searchLower));
        }

        var total = query.Count();
        var items = query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserSummaryDto(
                u.Id,
                u.Email,
                u.FullName,
                u.Role,
                u.Status,
                u.CreatedAt
            ))
            .ToList();

        var result = new PagedResult<UserSummaryDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total
        };

        return Task.FromResult(result);
    }
}
