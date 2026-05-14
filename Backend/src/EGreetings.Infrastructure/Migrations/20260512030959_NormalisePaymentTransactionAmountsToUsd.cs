using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGreetings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalisePaymentTransactionAmountsToUsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalise existing PaymentTransactions: VND amounts → USD, AdminGrant → CardPayment
            migrationBuilder.Sql("""
                UPDATE pt
                SET pt.Amount        = CASE s.[Plan] WHEN 2 THEN 39.99 ELSE 4.99 END,
                    pt.Currency      = 'USD',
                    pt.PaymentMethod = 'CardPayment'
                FROM PaymentTransactions pt
                JOIN Subscriptions s ON s.Id = pt.SubscriptionId
                WHERE pt.Currency = 'VND' OR pt.PaymentMethod = 'AdminGrant';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
