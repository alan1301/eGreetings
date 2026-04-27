using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Contacts.CreateContact;

/// <summary>UC16 – BR-20: Max 200 contacts/user. BR-21: name <= 100, valid email.</summary>
public record CreateContactCommand(
    Guid UserId,
    string Name,
    string Email,
    string Group
) : IRequest<Guid>;

public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    public CreateContactCommandValidator()
    {
        // BR-21
        RuleFor(x => x.Name).NotEmpty().MaximumLength(BusinessConstants.MaxContactNameLength);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email không hợp lệ");
        RuleFor(x => x.Group).NotEmpty();
    }
}

public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, Guid>
{
    private readonly IAppDbContext _db;

    public CreateContactCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateContactCommand request, CancellationToken ct)
    {
        // BR-20: Max 200 contacts per user
        var count = await _db.Contacts.CountAsync(c => c.UserId == request.UserId && !c.IsDeleted, ct);
        if (count >= BusinessConstants.MaxContactsPerUser)
            throw new BusinessRuleViolationException("BR-20",
                $"Danh bạ đầy. Tối đa {BusinessConstants.MaxContactsPerUser} liên hệ. Xóa bớt để thêm mới.");

        var group = Enum.Parse<EGreetings.Domain.Enums.ContactGroup>(request.Group, ignoreCase: true);

        var contact = new Contact
        {
            UserId = request.UserId,
            Name = request.Name.Trim(),
            Email = request.Email.ToLower().Trim(),
            Group = group
        };

        await _db.Contacts.AddAsync(contact, ct);
        await _db.SaveChangesAsync(ct);
        return contact.Id;
    }
}
