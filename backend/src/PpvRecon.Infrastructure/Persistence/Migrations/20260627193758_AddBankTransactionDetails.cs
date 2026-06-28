using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PpvRecon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankTransactionDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankTransactionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: false),
                    TransactionAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DebitAmount = table.Column<long>(type: "INTEGER", nullable: false),
                    CreditAmount = table.Column<long>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    BankAccount = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceJobRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceJobRunItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    RawResponseId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankTransactionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankTransactionDetails_ExternalApiRawResponses_RawResponseId",
                        column: x => x.RawResponseId,
                        principalTable: "ExternalApiRawResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankTransactionDetails_JobRunItems_SourceJobRunItemId",
                        column: x => x.SourceJobRunItemId,
                        principalTable: "JobRunItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankTransactionDetails_JobRuns_SourceJobRunId",
                        column: x => x.SourceJobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankTransactionDetails_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankTransactionDetails_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankTransactionDetails_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "BankTransactionDetails",
                columns: new[] { "Id", "BankAccount", "BusinessDate", "Content", "CreatedAtUtc", "CreatedByUserId", "CreditAmount", "DebitAmount", "ParkId", "PaymentType", "RawResponseId", "SourceJobRunId", "SourceJobRunItemId", "SourceType", "TransactionAtUtc", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, null, "2025-09-17", "HBK-TKThe :1SB2B24, tại NCB. ND Top up Sunworld - ezCloud 17.09.2025 -CTLNHIDO000012817233009-1/1-PMT-002", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 490000000L, null, "Prepaid", null, null, null, "Api", new DateTime(2025, 9, 17, 17, 33, 32, 0, DateTimeKind.Utc), null, null },
                    { 2, null, "2026-04-28", "HBK-TKThe :19139932758899, tại Techcombank. ND ezCloud topup trien khai ban ve bang he thong API cho VIN CUA HOI ngay 28 04 2026 -CTLNHIDO000015124913428-1/1-PMT-002 244", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 50000000L, null, "Prepaid", null, null, null, "Api", new DateTime(2026, 4, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 3, null, "2026-04-28", "HBK-TKThe :1029876329, tại Vietcombank. ND ezCloud topup trien khai ban ve bang he thong API cho VIN NAM HOI AN ngay 28 04 2026 -CTLNHIDO000015124944210-1/1-PMT-002 245", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 50000000L, null, "Prepaid", null, null, null, "Api", new DateTime(2026, 4, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 4, null, "2026-04-28", "HBK-TKThe :1SB2B24, tại NCB. ND Top-up SUNWORLD ezCloud ngay 28 04 2026 -CTLNHIDO000015124947382-1/1-PMT-002 246", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 490000000L, null, "Prepaid", null, null, null, "Api", new DateTime(2026, 4, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 5, null, "2026-04-28", "HBK-TKThe :0091000593278, tại Vietcombank. ND ezCloud topup trien khai ban ve bang he thong API cho VIN PHU QUOC ngay 28 04 2026 -CTLNHIDO000015124982897-1/1-PMT-002 248", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 100000000L, null, "Prepaid", null, null, null, "Api", new DateTime(2026, 4, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 6, null, "2026-04-29", "HBK-TKThe :700029610000, tại Shinhan Bank V. ND ezCloud thanh toan nhap lo cho THUY CUNG LOTTE ngay 29 04 2026 lan 1 -CTLNHIDO000015134693760-1/1-PMT-002 243", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 365625000L, null, "Prepaid", null, null, null, "Api", new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 7, null, "2026-04-29", "HBK-TKThe :700029610000, tại Shinhan Bank V. ND ezCloud thanh toan nhap lo cho THUY CUNG LOTTE ngay 29 04 2026 lan 2 -CTLNHIDO000015134727960-1/1-PMT-002 246", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 237375000L, null, "Prepaid", null, null, null, "Api", new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 8, null, "2025-09-16", "HBK-TKThe :57457, tại Techcombank. ND ezCloud thanh toan cong no cho Son Tien ngay 16 09 2025 -CTLNHIDO-PMT-001", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 42495000L, null, "Debt", null, null, null, "Api", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 9, null, "2025-09-16", "HBK-TKThe :200077779999, tại Vietcombank. ND ezCloud thanh toan cong no cho Mikazuki ngay 16 09 2025 -CTLNHIDO-PMT-002", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 35953000L, null, "Debt", null, null, null, "Api", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 10, null, "2025-09-16", "HBK-TKThe :60300641396, tại Vietcombank. ND ezCloud thanh toan cong no cho Mekong ngay 16 09 2025 -CTLNHIDO-PMT-003", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 49833500L, null, "Debt", null, null, null, "Api", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 11, null, "2025-09-16", "HBK-TKThe :1027882298, tại Vietcombank. ND ezCloud thanh toan cong no cho Ho Tram ngay 16 09 2025 -CTLNHIDO-PMT-004", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 22850000L, null, "Debt", null, null, null, "Api", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 12, null, "2025-09-16", "HBK-TKThe :3336333979, tại Vietcombank. ND ezCloud thanh toan cong no cho Nova Phan Thiet ngay 16 09 2025 -CTLNHIDO-PMT-005", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 119995000L, null, "Debt", null, null, null, "Api", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 13, null, "2025-09-16", "HBK-TKThe :11004009888, tại Vietcombank. ND ezCloud thanh toan cong no cho Cong Vien Nuoc Ho Tay ngay 16 09 2025 -CTLNHIDO-PMT-006", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 32082500L, null, "Debt", null, null, null, "Api", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 14, null, "2025-09-16", "HBK-TKThe :1100030038237, tại Vietcombank. ND ezCloud thanh toan cong no cho Sealinks ngay 16 09 2025 -CTLNHIDO-PMT-007", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 8110000L, null, "Debt", null, null, null, "Api", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, null },
                    { 15, null, "2025-09-16", "HBK-TKThe :sightseeing, tại Vietcombank. ND ezCloud thanh toan cong no cho Sightseeing HN ngay 16 09 2025 -CTLNHIDO-PMT-008", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, 0L, 9996172999L, null, "Debt", null, null, null, "Api", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactionDetails_BusinessDate",
                table: "BankTransactionDetails",
                column: "BusinessDate");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactionDetails_CreatedByUserId",
                table: "BankTransactionDetails",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactionDetails_ParkId",
                table: "BankTransactionDetails",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactionDetails_PaymentType",
                table: "BankTransactionDetails",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactionDetails_RawResponseId",
                table: "BankTransactionDetails",
                column: "RawResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactionDetails_SourceJobRunId",
                table: "BankTransactionDetails",
                column: "SourceJobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactionDetails_SourceJobRunItemId",
                table: "BankTransactionDetails",
                column: "SourceJobRunItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactionDetails_SourceType",
                table: "BankTransactionDetails",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactionDetails_UpdatedByUserId",
                table: "BankTransactionDetails",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankTransactionDetails");
        }
    }
}
