using MediatR;
using EGreetings.User.Application.DTOs;

namespace EGreetings.User.Application.Features.Contacts;

public record GetContactsQuery(int UserId) : IRequest<List<ContactDto>>;
