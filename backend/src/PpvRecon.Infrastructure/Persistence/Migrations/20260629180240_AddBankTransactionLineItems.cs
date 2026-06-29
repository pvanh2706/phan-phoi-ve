using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PpvRecon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankTransactionLineItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LineItemsJson",
                table: "BankTransactionDetails",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 1,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 2,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 3,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 4,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 5,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 6,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 7,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 8,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 9,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 10,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 11,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 12,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 13,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 14,
                column: "LineItemsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "BankTransactionDetails",
                keyColumn: "Id",
                keyValue: 15,
                column: "LineItemsJson",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LineItemsJson",
                table: "BankTransactionDetails");
        }
    }
}
