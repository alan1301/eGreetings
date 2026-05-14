namespace EGreetings.Shared.Constants;

/// <summary>
/// All business rule magic numbers from PROJECT_RULES.md (BR-01 to BR-33)
/// </summary>
public static class BusinessConstants
{
    // ── Auth (BR-01, BR-03, BR-04, BR-05, BR-26) ──────────────────────
    public const int PasswordMinLength = 8;
    public const int MaxFailedLoginAttempts = 5;          // BR-04
    public const int CaptchaFromFailedAttempt = 3;        // BR-04
    public const int LockoutMinutes = 15;                 // BR-04
    public const int EmailVerificationTokenTtlHours = 24; // BR-03
    public const int PasswordResetTokenTtlMinutes = 15;   // BR-26
    public const int AccessTokenTtlMinutes = 1440;        // 24h
    public const int RememberMeTokenTtlDays = 30;         // BR-05
    public const int RefreshTokenTtlDays = 30;

    // ── Cards & Sending (BR-06, BR-07, BR-08, BR-09, BR-10, BR-11) ───
    public const long MaxImageFileSizeBytes = 5L * 1024 * 1024;  // BR-06: 5 MB
    public const long MaxVideoFileSizeBytes = 20L * 1024 * 1024; // BR-07: 20 MB
    public const int MaxPersonalMessageLength = 500;               // BR-08
    public const int AutoSaveIntervalSeconds = 30;                 // BR-09
    public const int MaxCardsPerDayNonSubscribe = 50;              // BR-10
    public const int ScheduledSendMinutesAhead = 5;                // BR-11
    public const int MaxDraftsPerUser = 50;

    public static readonly string[] AllowedImageContentTypes =
        ["image/jpeg", "image/png", "image/gif"];            // BR-06

    public const string AllowedVideoContentType = "video/mp4"; // BR-07

    // ── Feedback (BR-13) ──────────────────────────────────────────────
    public const int MaxFeedbacksPerUserPerDay = 5;        // BR-13

    // ── Subscribe (BR-14, BR-30) ──────────────────────────────────────
    public const int MinSubscriptionEmailCount = 10;        // BR-14
    public const int SubscriptionRenewalDays = 30;          // BR-30
    public const decimal MonthlyPlanPriceUsd = 4.99m;
    public const decimal AnnualPlanPriceUsd  = 39.99m;

    // ── Contacts (BR-20, BR-21) ───────────────────────────────────────
    public const int MaxContactsPerUser = 200;              // BR-20
    public const int MaxContactNameLength = 100;            // BR-21

    // ── Email (BR-28, BR-32) ──────────────────────────────────────────
    public const string SystemFromEmail = "noreply@e-greetings.com"; // BR-28
    public const string SystemFromName = "E-Greetings";
    public const int MaxEmailRetryAttempts = 3;             // BR-32
    public const int EmailRetryIntervalMinutes = 5;         // BR-32

    // ── Logging (BR-33) ───────────────────────────────────────────────
    public const int SystemLogRetentionDays = 30;           // BR-33
    public const int BatchLogRetentionDays = 90;            // UC20

    // ── Pagination ────────────────────────────────────────────────────
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    // ── Subscription reminders (UC15) ─────────────────────────────────
    public static readonly int[] ReminderDaysBeforeExpiry = [5, 1];
}
