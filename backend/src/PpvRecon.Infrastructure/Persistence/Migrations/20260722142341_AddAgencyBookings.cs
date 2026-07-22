using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PpvRecon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgencyBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceSystem = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceTransactionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BookingCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BookingDate = table.Column<string>(type: "TEXT", nullable: false),
                    BuyingAgentId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsAgencyMatched = table.Column<bool>(type: "INTEGER", nullable: false),
                    BuyingAgentCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BuyingAgentName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ParentBuyingAgentCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ParentBuyingAgentName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    SellingAgentCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SellingAgentName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ParkExternalCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ParkExternalName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    TicketTypeCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TicketTypeName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TicketGroupName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<long>(type: "INTEGER", nullable: false),
                    SalesAmount = table.Column<long>(type: "INTEGER", nullable: false),
                    CostAmount = table.Column<long>(type: "INTEGER", nullable: false),
                    Subtotal = table.Column<long>(type: "INTEGER", nullable: false),
                    Discount = table.Column<long>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_AgencyBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyBookings_Agencies_BuyingAgentId",
                        column: x => x.BuyingAgentId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgencyBookings_ExternalApiRawResponses_RawResponseId",
                        column: x => x.RawResponseId,
                        principalTable: "ExternalApiRawResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgencyBookings_JobRunItems_SourceJobRunItemId",
                        column: x => x.SourceJobRunItemId,
                        principalTable: "JobRunItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgencyBookings_JobRuns_SourceJobRunId",
                        column: x => x.SourceJobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgencyBookings_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgencyBookings_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_BookingCode",
                table: "AgencyBookings",
                column: "BookingCode");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_BookingDate",
                table: "AgencyBookings",
                column: "BookingDate");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_BuyingAgentCode",
                table: "AgencyBookings",
                column: "BuyingAgentCode");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_BuyingAgentId",
                table: "AgencyBookings",
                column: "BuyingAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_CreatedByUserId",
                table: "AgencyBookings",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_RawResponseId",
                table: "AgencyBookings",
                column: "RawResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_SourceJobRunId",
                table: "AgencyBookings",
                column: "SourceJobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_SourceJobRunItemId",
                table: "AgencyBookings",
                column: "SourceJobRunItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_SourceSystem_SourceTransactionId",
                table: "AgencyBookings",
                columns: new[] { "SourceSystem", "SourceTransactionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_SourceType",
                table: "AgencyBookings",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyBookings_UpdatedByUserId",
                table: "AgencyBookings",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyBookings");
        }
    }
}
