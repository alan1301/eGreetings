namespace EGreetings.Application.Interfaces;

/// <summary>
/// Email message model.
/// BR-28: Always set From = noreply@e-greetings.com and ReplyTo = sender's email.
/// </summary>
public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string ReplyTo { get; set; } = string.Empty;   // BR-28: sender's email
    public string From { get; set; } = "noreply@e-greetings.com"; // BR-28
    public string FromName { get; set; } = "E-Greetings";
}

public interface IEmailService
{
    /// <summary>
    /// Send an email. Always sets Reply-To per BR-28.
    /// </summary>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
