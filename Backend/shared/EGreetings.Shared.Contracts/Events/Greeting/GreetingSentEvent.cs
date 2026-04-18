namespace EGreetings.Shared.Contracts.Events.Greeting;

public record GreetingSentEvent(
    int GreetingId,
    string RecipientEmail,
    string RecipientName,
    string SenderName,
    string? SenderMessage,
    string ViewUrl,
    DateTime SentAt);
