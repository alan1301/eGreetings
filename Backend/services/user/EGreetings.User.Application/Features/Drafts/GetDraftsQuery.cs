using MediatR;
using EGreetings.User.Application.DTOs;

namespace EGreetings.User.Application.Features.Drafts;

public record GetDraftsQuery(int UserId) : IRequest<List<DraftDto>>;
