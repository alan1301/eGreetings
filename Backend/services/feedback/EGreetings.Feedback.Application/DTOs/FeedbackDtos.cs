namespace EGreetings.Feedback.Application.DTOs;

public record FeedbackDto(
    int Id,
    int? UserId,
    string Subject,
    string Content,
    int? StarRating,
    string Status,
    string? Reply,
    DateTime? RepliedAt,
    bool IsRead,
    DateTime CreatedAt
);

public record PagedResult<T>(
    List<T> Items,
    int Total,
    int Page,
    int PageSize
);
