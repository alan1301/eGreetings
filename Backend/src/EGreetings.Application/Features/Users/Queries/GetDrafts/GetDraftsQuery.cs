using EGreetings.Application.Features.Users.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Users.Queries.GetDrafts;

/// <summary>UC17 - Xem danh sách bản nháp</summary>
public record GetDraftsQuery(int UserId) : IRequest<IReadOnlyList<DraftDto>>;
