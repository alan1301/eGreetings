using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGreetings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigratePaymentMethodToCardPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Subscriptions
                SET PaymentMethod = 'CardPayment'
                WHERE PaymentMethod IN ('Gateway', 'BankTransfer');

                UPDATE PaymentTransactions
                SET PaymentMethod = 'CardPayment'
                WHERE PaymentMethod IN ('Gateway', 'BankTransfer');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
