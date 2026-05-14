using System.Text.RegularExpressions;

namespace EGreetings.Domain.ValueObjects;

/// <summary>
/// Email value object — validation at construction, normalized to lowercase.
/// </summary>
public sealed record Email
{
    private static readonly Regex _format = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        var trimmed = value.Trim();
        if (!_format.IsMatch(trimmed))
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));

        Value = trimmed.ToLowerInvariant();
    }

    public static bool TryCreate(string? value, out Email? email)
    {
        try
        {
            email = new Email(value ?? string.Empty);
            return true;
        }
        catch
        {
            email = null;
            return false;
        }
    }

    public static implicit operator string(Email e) => e.Value;
    public override string ToString() => Value;
}
