using EGreetings.Domain.ValueObjects;
using Xunit;

namespace EGreetings.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Add_same_currency_sums_amounts()
    {
        var result = new Money(10m, "USD").Add(new Money(5.5m, "USD"));
        Assert.Equal(15.5m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Add_different_currency_throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => new Money(10m, "USD").Add(new Money(10m, "VND")));
    }

    [Fact]
    public void Negative_amount_throws()
    {
        Assert.Throws<ArgumentException>(() => new Money(-1m));
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Invalid_currency_throws(string currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(0m, currency));
    }

    [Fact]
    public void Zero_factory_returns_zero_money()
    {
        var m = Money.Zero();
        Assert.Equal(0m, m.Amount);
        Assert.Equal("USD", m.Currency);
    }
}
