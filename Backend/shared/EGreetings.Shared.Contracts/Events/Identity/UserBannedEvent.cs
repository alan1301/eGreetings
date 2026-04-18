namespace EGreetings.Shared.Contracts.Events.Identity;

public record UserBannedEvent(int UserId, string Email, bool IsBanned, string? Reason, DateTime OccurredAt);
