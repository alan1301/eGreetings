namespace EGreetings.Shared.Domain.ValueObjects;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }

        email = email.Trim();

        if (!IsValidEmail(email))
        {
            throw new ArgumentException($"Invalid email format: {email}", nameof(email));
        }

        return new Email(email);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
        catch
        {
            return false;
        }
    }

    public static implicit operator string(Email email)
    {
        return email.Value;
    }

    public static implicit operator Email(string email)
    {
        return Create(email);
    }

    public override string ToString()
    {
        return Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Email other)
        {
            return false;
        }

        return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return Value.ToLowerInvariant().GetHashCode();
    }
}
