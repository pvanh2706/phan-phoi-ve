using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PpvRecon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowColumns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ColumnKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    HeadTone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CardStatusLabel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CardTone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    VisibleFields = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PermittedUserIds = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowColumns_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowColumns_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    PaymentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: true),
                    BankAccount = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Amount = table.Column<long>(type: "INTEGER", nullable: false),
                    ExecuteDate = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ColumnId = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowColumns_ColumnId",
                        column: x => x.ColumnId,
                        principalTable: "WorkflowColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "WorkflowColumns",
                columns: new[] { "Id", "CardStatusLabel", "CardTone", "ColumnKey", "CreatedAtUtc", "CreatedByUserId", "HeadTone", "PermittedUserIds", "SortOrder", "Title", "UpdatedAtUtc", "UpdatedByUserId", "VisibleFields" },
                values: new object[,]
                {
                    { 1, "Lập phiếu", "gray", "lap-phieu", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "gray", "", 1, "Kế toán / NVKD lập phiếu", null, null, "title,desc,amount,date,tag" },
                    { 2, "Chờ duyệt", "blue", "truong-bo-phan-duyet", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "sky", "", 2, "Trưởng bộ phận duyệt", null, null, "title,desc,amount,date,tag" },
                    { 3, "Chuyển khoản", "indigo", "kiem-tra-chuyen-khoan", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "indigo", "", 3, "Kế toán kiểm tra & chuyển khoản", null, null, "title,desc,amount,date,tag" },
                    { 4, "Hoàn thành", "green", "hoan-thanh", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "green", "", 4, "Hoàn thành", null, null, "title,desc,amount,date,tag" },
                    { 5, "Thất bại", "red", "that-bai", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "red", "", 5, "Thất bại", null, null, "title,desc,amount,date,tag" }
                });

            migrationBuilder.InsertData(
                table: "WorkflowTasks",
                columns: new[] { "Id", "Amount", "BankAccount", "BankName", "ColumnId", "CreatedAtUtc", "CreatedByUserId", "ExecuteDate", "Note", "ParkId", "PaymentType", "SortOrder", "Title", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 9001, 50000000L, "19139932758899", "Techcombank", 1, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "2026-06-24", "Cần ezCloud Key tạy tiền trên hệ thống", null, "Prepaid", 1, "Nạp tiền - Vin Nha Trang", null, null },
                    { 9002, 100000000L, "0091000593278", "Vietcombank", 2, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "2026-04-28", "Cần ezCloud Key tạy tiền trên hệ thống", 9003, "Prepaid", 1, "Nạp tiền - Vin Phú Quốc", null, null },
                    { 9003, 50000000L, "1029876329", "Vietcombank", 2, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "2026-04-28", "Cần ezCloud Key tạy tiền trên hệ thống", 9002, "Prepaid", 2, "Nạp tiền - Vin Nam Hội An", null, null },
                    { 9004, 365625000L, "700029610000", "Shinhan Bank", 3, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "2026-04-29", "Bộ phận công tác: Phân Phối Vé · Kế Toán", 9004, "Prepaid", 1, "Nạp tiền - Thủy Cung Lotte (Lần 1)", null, null },
                    { 9005, 237375000L, "700029610000", "Shinhan Bank", 3, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "2026-04-29", "Bộ phận công tác: Phân Phối Vé · Kế Toán", 9004, "Prepaid", 2, "Nạp tiền - Thủy Cung Lotte (Lần 2)", null, null },
                    { 9006, 490000000L, "1213776969", "NCB", 4, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "2025-11-14", "Bộ phận công tác: Phân Phối Vé · Trưởng bộ", 9001, "Prepaid", 1, "Nạp tiền - Bản Mòng", null, null },
                    { 9007, 490000000L, "1SB2B24", "NCB", 4, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "2026-04-28", "Bộ phận công tác: Phân Phối Vé · Kế Toán", null, "Prepaid", 2, "Nạp tiền - Sunworld", null, null },
                    { 9008, 50000000L, "19139932758899", "Techcombank", 4, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "2026-04-28", "Bộ phận công tác: Phân Phối Vé · Kế Toán", 9005, "Prepaid", 3, "Nạp tiền - Vin Cửa Hội", null, null },
                    { 9009, 50000000L, null, null, 5, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "2026-06-15", "Sai số tài khoản, cần kiểm tra lại", null, "Prepaid", 1, "Nạp tiền - Vin Vũ Yên (Lần 2)", null, null },
                    { 9010, 8110000L, "1100030038237", "Vietcombank", 5, new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "2025-09-16", "Sai thông tin tài khoản thụ hưởng", 9006, "Debt", 2, "Thanh toán Công nợ - Sealinks", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowColumns_ColumnKey",
                table: "WorkflowColumns",
                column: "ColumnKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowColumns_CreatedByUserId",
                table: "WorkflowColumns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowColumns_SortOrder",
                table: "WorkflowColumns",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowColumns_UpdatedByUserId",
                table: "WorkflowColumns",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_ColumnId",
                table: "WorkflowTasks",
                column: "ColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_CreatedByUserId",
                table: "WorkflowTasks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_ParkId",
                table: "WorkflowTasks",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_PaymentType",
                table: "WorkflowTasks",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_UpdatedByUserId",
                table: "WorkflowTasks",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowTasks");

            migrationBuilder.DropTable(
                name: "WorkflowColumns");
        }
    }
}
