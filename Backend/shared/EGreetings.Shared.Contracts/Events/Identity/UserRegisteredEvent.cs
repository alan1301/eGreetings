namespace EGreetings.Shared.Contracts.Events.Identity;

public record UserRegisteredEvent(
    int UserId,
    string Email,
    string FullName,
    DateTime RegisteredAt
);
