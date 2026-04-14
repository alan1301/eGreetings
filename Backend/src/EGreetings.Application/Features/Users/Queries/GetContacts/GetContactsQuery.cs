using EGreetings.Application.Features.Users.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Users.Queries.GetContacts;

/// <summary>UC16 - Xem danh bạ người nhận</summary>
public record GetContactsQuery(int UserId) : IRequest<IReadOnlyList<ContactDto>>;
