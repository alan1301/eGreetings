using FluentValidation;

namespace EGreetings.Application.Features.Greetings.Commands.SendGreeting;

public class SendGreetingCommandValidator : AbstractValidator<SendGreetingCommand>
{
    public SendGreetingCommandValidator()
    {
        RuleFor(x => x.TemplateId).GreaterThan(0);

        RuleFor(x => x.RecipientEmail)
            .NotEmpty().WithMessage("Email người nhận không được để trống.")
            .EmailAddress().WithMessage("Email người nhận không hợp lệ.");

        RuleFor(x => x.RecipientName)
            .NotEmpty().WithMessage("Tên người nhận không được để trống.");

        RuleFor(x => x.SenderMessage)
            .MaximumLength(2000).WithMessage("Lời nhắn tối đa 2000 ký tự.");

        // UC08: Guest phải cung cấp email
        When(x => x.UserId == null, () =>
        {
            RuleFor(x => x.GuestSenderEmail)
                .NotEmpty().WithMessage("Guest cần cung cấp email người gửi.")
                .EmailAddress().WithMessage("Email người gửi không hợp lệ.");
            RuleFor(x => x.GuestSenderName)
                .NotEmpty().WithMessage("Guest cần cung cấp tên người gửi.");
        });

        // ScheduledAt phải trong tương lai
        When(x => x.ScheduledAt.HasValue, () =>
        {
            RuleFor(x => x.ScheduledAt!.Value)
                .GreaterThan(DateTime.UtcNow).WithMessage("Thời gian lên lịch phải trong tương lai.");
        });
    }
}
