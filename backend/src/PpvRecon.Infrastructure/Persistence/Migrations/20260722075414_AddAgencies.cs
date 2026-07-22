using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PpvRecon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ParentCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ParentName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agencies_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agencies_Users_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agencies_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_Code",
                table: "Agencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_CreatedByUserId",
                table: "Agencies",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_DeletedByUserId",
                table: "Agencies",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_IsDeleted",
                table: "Agencies",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_Name",
                table: "Agencies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_UpdatedByUserId",
                table: "Agencies",
                column: "UpdatedByUserId");

            // Nguồn: "Danh sach dai ly dang su dung (01-20.07.2026).xlsx",
            // sheet "Danh sách đại lý", lọc mã đại lý mua cấp trên = 5129.
            var agencySeed = new (int Id, string Code, string Name, string Source)[]
            {
                (1, "3786", "Oneinventory_API_ZaloPay", "OneInventory"),
                (2, "5222", "Oneinventory_QR_ b2b.oneinventory.com", "OneInventory"),
                (3, "5267", "Oneinventory_ETC", "OneInventory"),
                (4, "5303", "Oneinventory_Hani Travel", "OneInventory"),
                (5, "5306", "Oneinventory_VETAUTHAMVINHHALONG.COM", "OneInventory"),
                (6, "5360", "Oneinventory_Thanh Long", "OneInventory"),
                (7, "5375", "Oneinventory_Mega View Homestay", "OneInventory"),
                (8, "5387", "Oneinventory_LLC", "OneInventory"),
                (9, "5666", "Oneinventory_360GO", "OneInventory"),
                (10, "5669", "Oneinventory_Web_1i.com.vn_PL", "OneInventory"),
                (11, "5681", "Oneinventory_ DN TRIP TRAVEL", "OneInventory"),
                (12, "5720", "Oneinventory_ETICKET247", "OneInventory"),
                (13, "5771", "Oneinventory_BaDen Travel", "OneInventory"),
                (14, "5786", "Oneinventory_Ngọc Phúc Bana Tour", "OneInventory"),
                (15, "5879", "Oneinventory_Fantravel", "OneInventory"),
                (16, "5909", "Oneinventory_ALO TICKET", "OneInventory"),
                (17, "5939", "Oneinventory_Vân Thiên", "OneInventory"),
                (18, "5954", "Oneinventory_GoGo Phú Quốc", "OneInventory"),
                (19, "5987", "Oneinventory_E-Travel", "OneInventory"),
                (20, "5996", "Oneinventory_Silk Travel", "OneInventory"),
                (21, "6035", "Oneinventory_Đại lý vé Sun World Thanh Thủy", "OneInventory"),
                (22, "6122", "Oneinventory_Hanh BANA", "OneInventory"),
                (23, "6125", "Oneinventory_NamQuanTicket_SWSS", "OneInventory"),
                (24, "6161", "Oneinventory_HIEP TRINH", "OneInventory"),
                (25, "6173", "Oneinventory_HV", "OneInventory"),
                (26, "6185", "Oneinventory_Sol Travel", "OneInventory"),
                (27, "6224", "Oneinventory_Hyper Team", "OneInventory"),
                (28, "6266", "Oneinventory_Tam Sinh Duyên", "OneInventory"),
                (29, "6281", "Oneinventory_WEB_VIETTEL_VDS", "OneInventory"),
                (30, "6284", "Oneinventory_DU LỊCH MẶT TRỜI_SUNTOUR", "OneInventory"),
                (31, "6344", "Oneinventory_Hoàng Kim Phát", "OneInventory"),
                (32, "6347", "Oneinventory_Kim Hương ( HKP)", "OneInventory"),
                (33, "6362", "Oneinventory_THẾ HỆ MỚI G - ONE TRAVEL", "OneInventory"),
                (34, "6392", "Oneinventory_Getgo Travel", "OneInventory"),
                (35, "6419", "Oneinventory_WEB_RM_PL", "OneInventory"),
                (36, "6473", "Oneinventory_ Silk Journey", "OneInventory"),
                (37, "6635", "Oneinventory_Anh Thư", "OneInventory"),
                (38, "6653", "Oneinventory_Viettraco", "OneInventory"),
                (39, "6680", "Oneinventory_QR__B2B.1i.com.vn", "OneInventory"),
                (40, "6695", "Oneinventory_Green Tree", "OneInventory"),
                (41, "6698", "Oneinventory_Phương Lan", "OneInventory"),
                (42, "6701", "Oneinventory_Eholiday", "OneInventory"),
                (43, "6728", "Oneinventory_Trí Đức", "OneInventory"),
                (44, "6737", "Oneinventory_WEB_GMBR_12", "OneInventory"),
                (45, "6797", "Oneinventory_Sao Mai Sa Pa", "OneInventory"),
                (46, "6818", "Oneinventory_WEB_Đại Hà_PL", "OneInventory"),
                (47, "6833", "Oneinventory_Minh Thanh Hoàng", "OneInventory"),
                (48, "6875", "Oneinventory_Web_ETICKET247 (VeCapTreoNuiBaDen.com)", "OneInventory"),
                (49, "6878", "Oneinventory_Web_ETICKET247 (VeCapTreoBaNaHills.com)", "OneInventory"),
                (50, "6923", "Oneinventory_API_Vietsmart", "OneInventory"),
                (51, "6968", "Oneinventory_Đại lý Vé Nova 1", "OneInventory"),
                (52, "6971", "Oneinventory_Đại lý Vé Nova 2", "OneInventory"),
                (53, "6974", "Oneinventory_ Đại lý vé Nova", "OneInventory"),
                (54, "6977", "Oneinventory_ Đại lý Villa Nova", "OneInventory"),
                (55, "6986", "Oneinventory_Sunny Travel", "OneInventory"),
                (56, "6992", "Oneinventory_HẢI MINH ANH", "OneInventory"),
                (57, "7007", "Oneinventory_Web_Quang Thắng Cát Bà", "OneInventory"),
                (58, "7085", "Oneinventory_API_GoStream", "OneInventory"),
                (59, "7088", "Oneinventory_VNPlus Travel", "AR & OneInventory"),
                (60, "7100", "Oneinventory_ HVW", "OneInventory"),
                (61, "7112", "Oneinventory_Đại lý vé Công viên thủy tinh", "OneInventory"),
                (62, "7157", "Oneinventory_Passion Hotel", "OneInventory"),
                (63, "7160", "Oneinventory_Lê Hạnh", "OneInventory"),
                (64, "7190", "Oneinventory_NhatKimYenTicket", "OneInventory"),
                (65, "7202", "Oneinventory_Đại lý vé NoVa World", "OneInventory"),
                (66, "7211", "Oneinventory_HG", "OneInventory"),
                (67, "7214", "Oneinventory_QR_b2b4.1i", "OneInventory"),
                (68, "7217", "Oneinventory_QR_b2b3.1i", "OneInventory"),
                (69, "7220", "Oneinventory_QR_b2b2.1i", "OneInventory"),
                (70, "7223", "Oneinventory_QR_b2b.1i", "OneInventory"),
                (71, "7229", "Oneinventory_QR_b2b5.1i", "OneInventory"),
                (72, "7280", "Oneinventory_Cát Bà Vi Vu", "OneInventory"),
                (73, "7313", "Oneinventory_Thu vé Sunworld", "OneInventory"),
                (74, "7322", "Oneinventory_NKY Thủy Cung", "OneInventory"),
                (75, "7355", "Oneinventory_VeDoiRong.VN", "OneInventory"),
                (76, "7361", "Oneinventory_Web_Novatouris_PL", "OneInventory"),
                (77, "7364", "Oneinventory_DEPSJC", "OneInventory"),
                (78, "7391", "Oneinventory_API_Klook", "OneInventory"),
                (79, "7394", "Oneinventory_AGSAPA", "OneInventory"),
                (80, "7421", "Oneinventory_ Bùi Ngân", "OneInventory"),
                (81, "7424", "Oneinventory_WEB_RM 2_PL", "OneInventory"),
                (82, "7475", "Oneinventory_Delight Park", "OneInventory"),
                (83, "7493", "Oneinventory_Web", "OneInventory"),
                (84, "7526", "Oneinventory_Đăng Bình", "OneInventory"),
                (85, "7532", "Oneinventory_BBooking", "OneInventory"),
                (86, "7541", "Oneinventory_API_WEB_ABBANK", "OneInventory"),
                (87, "7547", "Oneinventory_Web_Eholiday.1i.com.vn", "OneInventory"),
                (88, "7565", "Oneinventory_Web_Vé vui chơi 247", "OneInventory"),
                (89, "7574", "Oneinventory_Meetup Travel", "OneInventory"),
                (90, "7580", "Oneinventory_Feria Travel", "OneInventory"),
                (91, "7586", "Oneinventory_Amazingo", "OneInventory"),
                (92, "7589", "Oneinventory_Smart Travel Vietnam", "OneInventory"),
                (93, "7598", "Oneinventory_298 BOOKING", "OneInventory"),
                (94, "7601", "Oneinventory_ EZGIANG", "OneInventory"),
                (95, "7628", "Oneinventory_Web_ETICKET247_SUN WORLD VŨNG TÀU", "OneInventory"),
                (96, "7664", "Oneinventory_Thắng Dubai Baden", "OneInventory"),
                (97, "7667", "Oneinventory_GOLDEN LIFE", "OneInventory"),
                (98, "7685", "Oneinventory_SSDL", "OneInventory"),
                (99, "7697", "Oneinventory_ Dấu Ấn Đông Dương", "OneInventory"),
                (100, "7715", "Oneinventory_VNLINK", "OneInventory"),
                (101, "7718", "Oneinventory_Cobalt House", "OneInventory"),
                (102, "7757", "Oneinventory_Web_MT Sun World", "OneInventory"),
                (103, "7763", "Oneinventory_Du lịch Hạ Long Tours", "OneInventory"),
                (104, "7775", "Oneinventory_KIOT268", "OneInventory"),
                (105, "7793", "Oneinventory_ Xuân Quỳnh", "OneInventory"),
                (106, "7817", "Oneinventory_Nga Lee Voucher", "OneInventory"),
                (107, "7895", "Oneinventory_Vé Công Viên DK", "OneInventory"),
                (108, "7901", "Oneinventory_Web_vevuichoi24h.com", "OneInventory"),
                (109, "7904", "Oneinventory_Web_VEDOIRONG", "OneInventory"),
                (110, "7922", "Oneinventory_Sao Vàng", "OneInventory"),
                (111, "7934", "Oneinventory_BEGINNING", "OneInventory"),
                (112, "7952", "Oneinventory_Sao Vàng 1", "OneInventory"),
                (113, "8045", "Oneinventory_Thành Công Hotel Hạ Long", "OneInventory"),
                (114, "8072", "Oneinventory_Hải Yến", "OneInventory"),
            };

            var seedCreatedAtUtc = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);
            foreach (var agency in agencySeed)
            {
                migrationBuilder.InsertData(
                    table: "Agencies",
                    columns: new[] { "Id", "Code", "Name", "ParentCode", "ParentName", "Source", "CreatedAtUtc", "IsDeleted" },
                    values: new object[] { agency.Id, agency.Code, agency.Name, "5129", "Đại Lý ezCloud Mua_PL", agency.Source, seedCreatedAtUtc, false });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agencies");
        }
    }
}
