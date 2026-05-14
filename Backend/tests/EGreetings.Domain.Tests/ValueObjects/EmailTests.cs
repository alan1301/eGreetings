using EGreetings.Domain.ValueObjects;
using Xunit;

namespace EGreetings.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("Test.User+tag@Domain.CO")]
    public void Valid_email_constructs_and_normalizes(string raw)
    {
        var email = new Email(raw);
        Assert.Equal(raw.Trim().ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@dot")]
    [InlineData("@no-local.com")]
    public void Invalid_email_throws(string raw)
    {
        Assert.Throws<ArgumentException>(() => new Email(raw));
    }

    [Fact]
    public void TryCreate_returns_false_for_invalid()
    {
        Assert.False(Email.TryCreate("nope", out var email));
        Assert.Null(email);
    }

    [Fact]
    public void Records_with_same_value_are_equal()
    {
        Assert.Equal(new Email("a@b.com"), new Email("A@B.com"));
    }
}
