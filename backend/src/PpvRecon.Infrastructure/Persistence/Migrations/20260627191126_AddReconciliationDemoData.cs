using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PpvRecon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReconciliationDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Parks",
                columns: new[] { "Id", "ApiNote", "ApiProfileId", "ApiSiteId", "BalanceTransformRule", "BankAccount", "BankName", "Code", "CreatedAtUtc", "CreatedByUserId", "CreditLimit", "DeletedAtUtc", "DeletedByUserId", "IsDeleted", "Location", "Name", "PaymentType", "SearchCode", "Status", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 9001, null, null, null, null, "1SB2B24", null, "11681", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Bản Mòng", "Prepaid", null, "Active", null, null },
                    { 9002, null, null, null, null, "1029876329", null, "11682", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Vin Nam Hội An", "Prepaid", null, "Active", null, null },
                    { 9003, null, null, null, null, "0091000593278", null, "11683", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Vin Phú Quốc", "Prepaid", null, "Active", null, null },
                    { 9004, null, null, null, null, "700029610000", null, "11684", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Thủy Cung Lotte", "Prepaid", null, "Active", null, null },
                    { 9005, null, null, null, null, "19139932758899", null, "11685", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Vin Cửa Hội", "Prepaid", null, "Active", null, null },
                    { 9006, null, null, null, null, "1100030038237", null, "11686", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Sealinks", "Prepaid", null, "Active", null, null },
                    { 9007, null, null, null, null, "57457", null, "21001", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Sơn Tiên", "Debt", null, "Active", null, null },
                    { 9008, null, null, null, null, "200077779999", null, "21002", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Mikazuki", "Debt", null, "Active", null, null },
                    { 9009, null, null, null, null, "60300641396", null, "21003", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Mekong", "Debt", null, "Active", null, null },
                    { 9010, null, null, null, null, "1027882298", null, "21004", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Hồ Tràm", "Debt", null, "Active", null, null },
                    { 9011, null, null, null, null, "3336333979", null, "21005", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Nova Phan Thiết", "Debt", null, "Active", null, null },
                    { 9012, null, null, null, null, "11004009888", null, "21006", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Công viên nước Hồ Tây", "Debt", null, "Active", null, null },
                    { 9013, null, null, null, null, "1100030038237", null, "21007", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, null, "Sightseeing HN", "Debt", null, "Active", null, null }
                });

            migrationBuilder.InsertData(
                table: "ParkReconciliations",
                columns: new[] { "Id", "ActualBalance", "AdditionalAmount", "AdjustmentAmount", "AdjustmentNote", "BusinessDate", "CreatedAtUtc", "CreatedByUserId", "ExpectedBalance", "LastBuiltJobRunId", "LastSourceHash", "MissingActualBalance", "MissingBankTransaction", "MissingPreviousBalance", "MissingTicketCost", "ParkId", "PaymentType", "PreviousBalance", "PreviousBusinessDate", "RebuildCount", "ResolvedAtUtc", "ResolvedByUserId", "ResolvedSourceHash", "SourceChangedAfterResolved", "Status", "UpdatedAtUtc", "UpdatedByUserId", "UsedAmount", "VarianceAmount" },
                values: new object[,]
                {
                    { 9001, 490000000L, 490000000L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 490000000L, null, null, false, false, false, false, 9001, "Prepaid", 0L, "2025-09-16", 0, null, null, null, false, "Matched", null, null, 0L, 0L },
                    { 9002, 165000000L, 50000000L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 165000000L, null, null, false, false, false, false, 9002, "Prepaid", 150000000L, "2025-09-16", 0, null, null, null, false, "Matched", null, null, 35000000L, 0L },
                    { 9003, 250000000L, 100000000L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 255000000L, null, null, false, false, false, false, 9003, "Prepaid", 200000000L, "2025-09-16", 0, null, null, null, false, "Variance", null, null, 45000000L, -5000000L },
                    { 9004, 327625000L, 365625000L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 325625000L, null, null, false, false, false, false, 9004, "Prepaid", 80000000L, "2025-09-16", 0, null, null, null, false, "Variance", null, null, 120000000L, 2000000L },
                    { 9005, 37500000L, 50000000L, null, null, "2026-04-29", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 37500000L, null, null, false, false, false, false, 9005, "Prepaid", 0L, "2026-04-28", 0, null, null, null, false, "Matched", null, null, 12500000L, 0L },
                    { 9006, 16890000L, 0L, null, null, "2026-04-29", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 16890000L, null, null, false, false, false, false, 9006, "Prepaid", 25000000L, "2026-04-28", 0, null, null, null, false, "Matched", null, null, 8110000L, 0L },
                    { 9007, 77505000L, 0L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 77505000L, null, null, false, false, false, false, 9007, "Debt", 120000000L, "2025-09-16", 0, null, null, null, false, "Matched", null, null, 42495000L, 0L },
                    { 9008, 59047000L, 0L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 59047000L, null, null, false, false, false, false, 9008, "Debt", 95000000L, "2025-09-16", 0, null, null, null, false, "Matched", null, null, 35953000L, 0L },
                    { 9009, 99666500L, 0L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 100166500L, null, null, false, false, false, false, 9009, "Debt", 150000000L, "2025-09-16", 0, null, null, null, false, "Variance", null, null, 49833500L, -500000L },
                    { 9010, 32150000L, 0L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 32150000L, null, null, false, false, false, false, 9010, "Debt", 55000000L, "2025-09-16", 0, null, null, null, false, "Matched", null, null, 22850000L, 0L },
                    { 9011, 130005000L, 0L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 130005000L, null, null, false, false, false, false, 9011, "Debt", 250000000L, "2025-09-16", 0, null, null, null, false, "Matched", null, null, 119995000L, 0L },
                    { 9012, 48917500L, 0L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 47917500L, null, null, false, false, false, false, 9012, "Debt", 80000000L, "2025-09-16", 0, null, null, null, false, "Variance", null, null, 32082500L, 1000000L },
                    { 9013, -9496172999L, 0L, null, null, "2025-09-17", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, -9496172999L, null, null, false, false, false, false, 9013, "Debt", 500000000L, "2025-09-16", 0, null, null, null, false, "Matched", null, null, 9996172999L, 0L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9001);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9002);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9003);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9004);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9005);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9006);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9007);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9008);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9009);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9010);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9011);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9012);

            migrationBuilder.DeleteData(
                table: "ParkReconciliations",
                keyColumn: "Id",
                keyValue: 9013);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9001);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9002);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9003);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9004);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9005);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9006);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9007);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9008);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9009);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9010);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9011);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9012);

            migrationBuilder.DeleteData(
                table: "Parks",
                keyColumn: "Id",
                keyValue: 9013);
        }
    }
}
