using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Users.DTOs;
using EGreetings.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserListItemDto>>
{
    private readonly IRepository<User> _userRepo;

    public GetUsersQueryHandler(IRepository<User> userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<PagedResult<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userRepo.Query();

        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(u => u.FullName.Contains(request.Search) || u.Email.Contains(request.Search));

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .ThenBy(u => u.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserListItemDto(
                u.Id, u.FullName, u.Email, u.Phone,
                u.Role.ToString(), u.Status.ToString(),
                u.EmailVerifiedAt, u.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListItemDto>(items, total, request.Page, request.PageSize);
    }
}
