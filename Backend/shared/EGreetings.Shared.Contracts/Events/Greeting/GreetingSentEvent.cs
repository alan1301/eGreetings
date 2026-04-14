namespace EGreetings.Shared.Contracts.Events.Greeting;

public record GreetingSentEvent(
    int GreetingId,
    int? UserId,
    string RecipientEmail,
    string TemplateName,
    DateTime SentAt
);
