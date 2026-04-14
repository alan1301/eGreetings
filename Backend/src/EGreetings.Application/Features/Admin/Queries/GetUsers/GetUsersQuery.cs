using EGreetings.Application.Features.Users.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Queries.GetUsers;

/// <summary>UC14 - Xem danh sách người dùng (Admin)</summary>
public record GetUsersQuery(
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<UserListItemDto>>;
