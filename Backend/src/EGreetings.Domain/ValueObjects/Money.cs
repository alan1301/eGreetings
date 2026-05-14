namespace EGreetings.Domain.ValueObjects;

/// <summary>
/// Money value object — non-negative amount + ISO currency code.
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0m)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "USD") => new(0m, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot add Money with different currencies: {Currency} vs {other.Currency}.");
        return new Money(Amount + other.Amount, Currency);
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
