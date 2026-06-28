using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PpvRecon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveManualParkBalanceFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentDebt",
                table: "DailyParkBalanceSnapshots");

            migrationBuilder.DropColumn(
                name: "ManualReason",
                table: "DailyParkBalanceSnapshots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CurrentDebt",
                table: "DailyParkBalanceSnapshots",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualReason",
                table: "DailyParkBalanceSnapshots",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);
        }
    }
}
