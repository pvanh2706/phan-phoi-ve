using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PpvRecon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyArTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgencyArTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookingId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TravelAgentName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    TravelAgentCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ReceivableAccountCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<long>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DedupHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SourceRowNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceJobRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceJobRunItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyArTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyArTransactions_JobRunItems_SourceJobRunItemId",
                        column: x => x.SourceJobRunItemId,
                        principalTable: "JobRunItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgencyArTransactions_JobRuns_SourceJobRunId",
                        column: x => x.SourceJobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgencyArTransactions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgencyArTransactions_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyArTransactions_BookingId",
                table: "AgencyArTransactions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyArTransactions_BusinessDate",
                table: "AgencyArTransactions",
                column: "BusinessDate");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyArTransactions_CreatedByUserId",
                table: "AgencyArTransactions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyArTransactions_DedupHash",
                table: "AgencyArTransactions",
                column: "DedupHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgencyArTransactions_SourceJobRunId",
                table: "AgencyArTransactions",
                column: "SourceJobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyArTransactions_SourceJobRunItemId",
                table: "AgencyArTransactions",
                column: "SourceJobRunItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyArTransactions_SourceType",
                table: "AgencyArTransactions",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyArTransactions_TransactionDate",
                table: "AgencyArTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyArTransactions_TravelAgentCode",
                table: "AgencyArTransactions",
                column: "TravelAgentCode");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyArTransactions_UpdatedByUserId",
                table: "AgencyArTransactions",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyArTransactions");
        }
    }
}
