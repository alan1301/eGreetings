using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Cards;

// ── DTO ──────────────────────────────────────────────────────────────────────

/// <summary>
/// Unified item returned by GET /api/me/upcoming-events.
/// Covers two sources: Scheduled greetings (type=ScheduledGreeting)
/// and Contact occasion dates (type=ContactOccasion).
/// </summary>
public record UpcomingEventDto(
    /// <summary>ScheduledGreeting | ContactOccasion</summary>
    string Type,
    /// <summary>TransactionId (ScheduledGreeting) or ContactId (ContactOccasion)</summary>
    string Id,
    /// <summary>Human-friendly title: card name or occasion label + contact name</summary>
    string Title,
    /// <summary>Supporting detail: recipient email or contact email</summary>
    string Subtitle,
    /// <summary>The target date — scheduled send time or next occurrence of the occasion</summary>
    DateTime EventDate,
    /// <summary>Days until EventDate (0 = today)</summary>
    int DaysLeft,
    /// <summary>Card thumbnail, null for ContactOccasion items</summary>
    string? ThumbnailUrl,
    /// <summary>Card name, null for ContactOccasion items</summary>
    string? CardName,
    /// <summary>Contact email (ContactOccasion) or recipient email (ScheduledGreeting) — used for pre-filling the personalize flow</summary>
    string? RecipientEmail
);

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns a merged list of upcoming events for the dashboard panel:
/// • ScheduledGreeting: transactions with Status=Scheduled (status!=Cancelled/Sent/Failed), ordered by ScheduledSendAt
/// • ContactOccasion: contacts with OccasionDate within the next <WindowDays> days
/// </summary>
public record GetUpcomingEventsQuery(Guid UserId, int WindowDays = 30, int MaxItems = 8)
    : IRequest<List<UpcomingEventDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetUpcomingEventsQueryHandler : IRequestHandler<GetUpcomingEventsQuery, List<UpcomingEventDto>>
{
    private readonly IAppDbContext _db;

    public GetUpcomingEventsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<List<UpcomingEventDto>> Handle(GetUpcomingEventsQuery request, CancellationToken ct)
    {
        var now     = DateTime.UtcNow;
        var window  = now.AddDays(request.WindowDays);
        var today   = DateOnly.FromDateTime(DateTime.Today);
        var results = new List<UpcomingEventDto>();

        // ── 1. Scheduled Greetings ──────────────────────────────────────────
        var scheduled = await _db.GreetingTransactions
            .Include(t => t.Card)
            .Where(t => t.SenderId == request.UserId
                     && t.Status == TransactionStatus.Scheduled
                     && t.ScheduledSendAt.HasValue
                     && t.ScheduledSendAt.Value >= now
                     && t.ScheduledSendAt.Value <= window)
            .OrderBy(t => t.ScheduledSendAt)
            .Select(t => new
            {
                t.Id,
                CardName       = t.Card.Name,
                ThumbnailUrl   = t.Card.ThumbnailUrl,
                t.RecipientEmail,
                t.Subject,
                ScheduledSendAt = t.ScheduledSendAt!.Value
            })
            .ToListAsync(ct);

        foreach (var t in scheduled)
        {
            var daysLeft = (int)Math.Ceiling((t.ScheduledSendAt - now).TotalDays);
            results.Add(new UpcomingEventDto(
                Type:          "ScheduledGreeting",
                Id:            t.Id.ToString(),
                Title:         t.Subject,
                Subtitle:      t.RecipientEmail,
                EventDate:     t.ScheduledSendAt,
                DaysLeft:      Math.Max(0, daysLeft),
                ThumbnailUrl:  t.ThumbnailUrl,
                CardName:      t.CardName,
                RecipientEmail: t.RecipientEmail
            ));
        }

        // ── 2. Contact Occasions ────────────────────────────────────────────
        var contacts = await _db.Contacts
            .Where(c => c.UserId == request.UserId && c.OccasionDate.HasValue)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Email,
                c.OccasionDate,
                c.OccasionLabel
            })
            .ToListAsync(ct);

        foreach (var c in contacts)
        {
            var occ      = c.OccasionDate!.Value;
            var thisYear = new DateOnly(today.Year, occ.Month, occ.Day);
            var nextOcc  = thisYear >= today ? thisYear : new DateOnly(today.Year + 1, occ.Month, occ.Day);
            var daysLeft = nextOcc.DayNumber - today.DayNumber;

            if (daysLeft > request.WindowDays) continue;

            // Convert DateOnly → DateTime at 08:00 local (as UTC for response)
            var eventDate = new DateTime(nextOcc.Year, nextOcc.Month, nextOcc.Day, 1, 0, 0, DateTimeKind.Utc); // 08:00 ICT = 01:00 UTC

            var label = string.IsNullOrWhiteSpace(c.OccasionLabel) ? "Occasion" : c.OccasionLabel;

            results.Add(new UpcomingEventDto(
                Type:          "ContactOccasion",
                Id:            c.Id.ToString(),
                Title:         $"{label} – {c.Name}",
                Subtitle:      c.Email,
                EventDate:     eventDate,
                DaysLeft:      daysLeft,
                ThumbnailUrl:  null,
                CardName:      null,
                RecipientEmail: c.Email
            ));
        }

        // ── Merge, sort by date, cap at MaxItems ────────────────────────────
        return results
            .OrderBy(e => e.DaysLeft)
            .ThenBy(e => e.EventDate)
            .Take(request.MaxItems)
            .ToList();
    }
}
