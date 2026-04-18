namespace EGreetings.Identity.Application.DTOs;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    string? Phone = null
);

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    string Token,
    int UserId,
    string Email,
    string FullName,
    string Role,
    DateTime ExpiresAt
);

public record ForgotPasswordRequest(
    string Email
);

public record ResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmPassword
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);

public record UserSummaryDto(
    int Id,
    string Email,
    string FullName,
    string Role,
    string Status,
    DateTime CreatedAt
);
