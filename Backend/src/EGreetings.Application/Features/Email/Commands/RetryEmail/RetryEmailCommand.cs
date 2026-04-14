using MediatR;

namespace EGreetings.Application.Features.Email.Commands.RetryEmail;

/// <summary>UC29 - Retry Email - BR-32: Tối đa 3 lần retry, cách 5 phút</summary>
public record RetryEmailCommand(int EmailLogId) : IRequest<bool>;
