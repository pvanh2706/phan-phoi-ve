using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PpvRecon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketSaleCostDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketSaleCostDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: false),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: true),
                    PaymentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BookingCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UnitPrice = table.Column<long>(type: "INTEGER", nullable: false),
                    TicketTypeName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TicketGroupName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SalesAmount = table.Column<long>(type: "INTEGER", nullable: false),
                    CostAmount = table.Column<long>(type: "INTEGER", nullable: false),
                    SellingAgentCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    BuyingAgentCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BuyingAgentName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ParkCodeSnapshot = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ParkNameSnapshot = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Subtotal = table.Column<long>(type: "INTEGER", nullable: false),
                    ExternalLineId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SellingAgentName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TicketTypeCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ParentBuyingAgentName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ParentBuyingAgentCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_TicketSaleCostDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketSaleCostDetails_ExternalApiRawResponses_RawResponseId",
                        column: x => x.RawResponseId,
                        principalTable: "ExternalApiRawResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketSaleCostDetails_JobRunItems_SourceJobRunItemId",
                        column: x => x.SourceJobRunItemId,
                        principalTable: "JobRunItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketSaleCostDetails_JobRuns_SourceJobRunId",
                        column: x => x.SourceJobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketSaleCostDetails_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketSaleCostDetails_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketSaleCostDetails_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TicketSaleCostDetails",
                columns: new[] { "Id", "BookingCode", "BusinessDate", "BuyingAgentCode", "BuyingAgentName", "CostAmount", "CreatedAtUtc", "CreatedByUserId", "ExternalLineId", "ParentBuyingAgentCode", "ParentBuyingAgentName", "ParkCodeSnapshot", "ParkId", "ParkNameSnapshot", "PaymentType", "Quantity", "RawResponseId", "SalesAmount", "SellingAgentCode", "SellingAgentName", "SourceJobRunId", "SourceJobRunItemId", "SourceType", "Subtotal", "TicketGroupName", "TicketTypeCode", "TicketTypeName", "UnitPrice", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, "60023151211", "2025-09-01", "7034", "Oneinventory_SJC Thủy cung Lotte", 875000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14409473", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11810", null, "VinKE & Aquarium Times City ezCMT", "Prepaid", 5, null, 886000L, "7310", "Thủy cung VINKE-EZCMT", null, null, "Api", 886000L, "SJC- Thủy cung Timescity", "347015", "Người lớn (Trên 1.4m)-Cuối tuần T7-CN", 177200L, null, null },
                    { 2, "60023151212", "2025-09-01", "7034", "Oneinventory_SJC Thủy cung Lotte", 468000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14409474", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11810", null, "VinKE & Aquarium Times City ezCMT", "Prepaid", 4, null, 480000L, "7310", "Thủy cung VINKE-EZCMT", null, null, "Api", 480000L, "SJC- Thủy cung Timescity", "347016", "Trẻ em (1.0–1.4m)-Cuối tuần T7-CN", 120000L, null, null },
                    { 3, "60023158801", "2025-09-02", "8012", "Oneinventory_SunWorld HN", 1200000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14510230", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "6935", null, "Sunworld Hà Nội", "Prepaid", 5, null, 1225000L, "6935", "Sun Group - ezCloud", null, null, "Api", 1225000L, "Sun Group - Sunworld", "198432", "Người lớn-Ngày thường", 245000L, null, null },
                    { 4, "60023158802", "2025-09-02", "8012", "Oneinventory_SunWorld HN", 540000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14510231", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "6935", null, "Sunworld Hà Nội", "Prepaid", 3, null, 555000L, "6935", "Sun Group - ezCloud", null, null, "Api", 555000L, "Sun Group - Sunworld", "198433", "Trẻ em-Ngày thường", 185000L, null, null },
                    { 5, "60023165411", "2025-09-03", "9034", "Oneinventory_VIN CUA HOI", 945000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14611020", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11682", null, "Vinpearl Cửa Hội", "Prepaid", 3, null, 960000L, "9012", "Vinpearl - ezCloud", null, null, "Api", 960000L, "Vinpearl - Cửa Hội", "220110", "Người lớn-Cuối tuần", 320000L, null, null },
                    { 6, "60023165500", "2025-09-03", "9035", "Oneinventory_VIN NAM HOI AN", 1100000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14611100", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11684", null, "Vinpearl Nam Hội An", "Prepaid", 4, null, 1120000L, "9014", "Vinpearl - ezCloud", null, null, "Api", 1120000L, "Vinpearl - Nam Hội An", "220115", "Người lớn-Ngày thường", 280000L, null, null },
                    { 7, "60023172011", "2025-09-04", "9036", "Oneinventory_VIN PHU QUOC", 2070000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14712300", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11686", null, "Vinpearl Phú Quốc", "Prepaid", 5, null, 2100000L, "9016", "Vinpearl - ezCloud", null, null, "Api", 2100000L, "Vinpearl - Phú Quốc", "220200", "Người lớn-Ngày thường", 420000L, null, null },
                    { 8, "60023172500", "2025-09-04", "7034", "Oneinventory_SJC Thủy cung Lotte", 956250L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14712400", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11810", null, "Thủy cung Lotte Hà Nội", "Prepaid", 5, null, 975000L, "7310", "Thủy cung VINKE-EZCMT", null, null, "Api", 975000L, "Thủy cung Lotte", "347020", "Người lớn-Ngày thường", 195000L, null, null },
                    { 9, "60023179001", "2025-09-05", "7034", "Oneinventory_SJC Thủy cung Lotte", 695000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14813000", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11810", null, "VinKE & Aquarium Times City ezCMT", "Prepaid", 4, null, 708800L, "7310", "Thủy cung VINKE-EZCMT", null, null, "Api", 708800L, "SJC- Thủy cung Timescity", "347030", "Người lớn (Trên 1.4m)-Ngày thường", 177200L, null, null },
                    { 10, "60023199001", "2025-09-16", "5034", "Oneinventory_Son Tien", 415000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14300001", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "10360", null, "Sơn Tiên", "Debt", 5, null, 425000L, "5020", "Sơn Tiên - ezCloud", null, null, "Api", 425000L, "Sơn Tiên - Công nợ", "180001", "Người lớn-Ngày thường", 85000L, null, null },
                    { 11, "60023199002", "2025-09-16", "5036", "Oneinventory_Mikazuki", 468000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14300002", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11423", null, "Mikazuki", "Debt", 4, null, 480000L, "5022", "Mikazuki - ezCloud", null, null, "Api", 480000L, "Mikazuki - Công nợ", "180010", "Người lớn-Cuối tuần", 120000L, null, null },
                    { 12, "60023199003", "2025-09-16", "5038", "Oneinventory_Mekong", 278000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14300003", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11588", null, "Mekong", "Debt", 3, null, 285000L, "5024", "Mekong - ezCloud", null, null, "Api", 285000L, "Mekong - Công nợ", "180020", "Người lớn-Ngày thường", 95000L, null, null },
                    { 13, "60023205001", "2025-09-17", "5044", "Oneinventory_Ho Tram", 585000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14400010", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11483", null, "Hồ Tràm", "Debt", 4, null, 600000L, "5030", "Hồ Tràm - ezCloud", null, null, "Api", 600000L, "Hồ Tràm - Công nợ", "180050", "Người lớn-Cuối tuần", 150000L, null, null },
                    { 14, "60023205002", "2025-09-17", "5046", "Oneinventory_Nova", 880000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14400011", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11480", null, "Nova Phan Thiết", "Debt", 5, null, 900000L, "5032", "Nova - ezCloud", null, null, "Api", 900000L, "Nova Phan Thiết - Công nợ", "180055", "Người lớn-Ngày thường", 180000L, null, null },
                    { 15, "60023211001", "2025-09-18", "5054", "Oneinventory_Sealinks", 322000L, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "14500020", "5129", "Oneinventory_Q R_ezCloud Mua_PL", "11807", null, "Sealinks", "Debt", 3, null, 330000L, "5040", "Sealinks - ezCloud", null, null, "Api", 330000L, "Sealinks - Công nợ", "180090", "Người lớn-Ngày thường", 110000L, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketSaleCostDetails_BookingCode",
                table: "TicketSaleCostDetails",
                column: "BookingCode");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSaleCostDetails_BusinessDate",
                table: "TicketSaleCostDetails",
                column: "BusinessDate");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSaleCostDetails_CreatedByUserId",
                table: "TicketSaleCostDetails",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSaleCostDetails_ParkId",
                table: "TicketSaleCostDetails",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSaleCostDetails_PaymentType",
                table: "TicketSaleCostDetails",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSaleCostDetails_RawResponseId",
                table: "TicketSaleCostDetails",
                column: "RawResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSaleCostDetails_SourceJobRunId",
                table: "TicketSaleCostDetails",
                column: "SourceJobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSaleCostDetails_SourceJobRunItemId",
                table: "TicketSaleCostDetails",
                column: "SourceJobRunItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSaleCostDetails_SourceType",
                table: "TicketSaleCostDetails",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSaleCostDetails_UpdatedByUserId",
                table: "TicketSaleCostDetails",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketSaleCostDetails");
        }
    }
}
