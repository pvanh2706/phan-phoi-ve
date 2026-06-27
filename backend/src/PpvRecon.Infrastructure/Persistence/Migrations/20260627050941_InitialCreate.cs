using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PpvRecon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FailedLoginCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LockReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AvatarPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LastLoginAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PasswordChangedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Users_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserEmailSnapshot = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    UserRoleSnapshot = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Module = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BeforeJson = table.Column<string>(type: "TEXT", nullable: true),
                    AfterJson = table.Column<string>(type: "TEXT", nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: true),
                    TriggeredBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TriggeredByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TotalItems = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessItems = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedItems = table.Column<int>(type: "INTEGER", nullable: false),
                    SkippedItems = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    SummaryJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobRuns_Users_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EnableInApp = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableSound = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationRecipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NotificationType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    PaymentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SearchCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    BankAccount = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreditLimit = table.Column<long>(type: "INTEGER", nullable: true),
                    ApiSiteId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ApiProfileId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BalanceTransformRule = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ApiNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_Parks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parks_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parks_Users_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parks_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    ValueType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsSensitive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Key);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ThemeMode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    JwtId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByIp = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DeviceName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    LastUsedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUsedIp = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RevokedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    RevokeReason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ReplacedBySessionId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_UserSessions_ReplacedBySessionId",
                        column: x => x.ReplacedBySessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_RevokedByUserId",
                        column: x => x.RevokedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParkReconciliations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: false),
                    PreviousBusinessDate = table.Column<string>(type: "TEXT", nullable: true),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PreviousBalance = table.Column<long>(type: "INTEGER", nullable: true),
                    AdditionalAmount = table.Column<long>(type: "INTEGER", nullable: true),
                    UsedAmount = table.Column<long>(type: "INTEGER", nullable: true),
                    ExpectedBalance = table.Column<long>(type: "INTEGER", nullable: true),
                    ActualBalance = table.Column<long>(type: "INTEGER", nullable: true),
                    VarianceAmount = table.Column<long>(type: "INTEGER", nullable: true),
                    AdjustmentAmount = table.Column<long>(type: "INTEGER", nullable: true),
                    AdjustmentNote = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MissingPreviousBalance = table.Column<bool>(type: "INTEGER", nullable: false),
                    MissingActualBalance = table.Column<bool>(type: "INTEGER", nullable: false),
                    MissingTicketCost = table.Column<bool>(type: "INTEGER", nullable: false),
                    MissingBankTransaction = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResolvedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastBuiltJobRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    RebuildCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSourceHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ResolvedSourceHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    SourceChangedAfterResolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkReconciliations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkReconciliations_JobRuns_LastBuiltJobRunId",
                        column: x => x.LastBuiltJobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkReconciliations_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkReconciliations_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkReconciliations_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkReconciliations_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParkRefunds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookingCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RefundDate = table.Column<string>(type: "TEXT", nullable: false),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: true),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: true),
                    ParkCodeSnapshot = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ParkNameSnapshot = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    PaymentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TicketTypeCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TicketTypeName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ParkRefundAmount = table.Column<long>(type: "INTEGER", nullable: true),
                    CustomerRefundAmount = table.Column<long>(type: "INTEGER", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ParkRefundStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerRefundStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_ParkRefunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkRefunds_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkRefunds_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkRefunds_Users_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkRefunds_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParkTicketTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TicketTypeCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TicketGroupName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CostPrice = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_ParkTicketTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkTicketTypes_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkTicketTypes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkTicketTypes_Users_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkTicketTypes_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyBankTransactionSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: false),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TransactionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TotalDebitAmount = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalCreditAmount = table.Column<long>(type: "INTEGER", nullable: false),
                    TransactionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceJobRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceJobRunItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    RawResponseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ManualReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyBankTransactionSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyBankTransactionSummaries_JobRuns_SourceJobRunId",
                        column: x => x.SourceJobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyBankTransactionSummaries_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyBankTransactionSummaries_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyBankTransactionSummaries_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyParkBalanceSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: false),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AvailableBalance = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentDebt = table.Column<long>(type: "INTEGER", nullable: true),
                    BankAccountSnapshot = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceJobRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceJobRunItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    RawResponseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ManualReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyParkBalanceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyParkBalanceSnapshots_JobRuns_SourceJobRunId",
                        column: x => x.SourceJobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyParkBalanceSnapshots_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyParkBalanceSnapshots_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyParkBalanceSnapshots_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyTicketCostSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: false),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TotalTicketCost = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalSalesAmount = table.Column<long>(type: "INTEGER", nullable: true),
                    TotalQuantity = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceJobRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceJobRunItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    RawResponseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ManualReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyTicketCostSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyTicketCostSummaries_JobRuns_SourceJobRunId",
                        column: x => x.SourceJobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyTicketCostSummaries_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyTicketCostSummaries_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyTicketCostSummaries_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExternalApiRawResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: true),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: true),
                    JobRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    JobRunItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    RequestUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RequestPayloadJson = table.Column<string>(type: "TEXT", nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    ResponseBodyJson = table.Column<string>(type: "TEXT", nullable: true),
                    IsSuccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    DurationMs = table.Column<int>(type: "INTEGER", nullable: true),
                    ReceivedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalApiRawResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalApiRawResponses_JobRuns_JobRunId",
                        column: x => x.JobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExternalApiRawResponses_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobRunItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    BusinessDate = table.Column<string>(type: "TEXT", nullable: true),
                    ParkId = table.Column<int>(type: "INTEGER", nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FinishedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DurationMs = table.Column<int>(type: "INTEGER", nullable: true),
                    ErrorCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    RawResponseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ResolvedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ManualResolutionNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRunItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobRunItems_ExternalApiRawResponses_RawResponseId",
                        column: x => x.RawResponseId,
                        principalTable: "ExternalApiRawResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobRunItems_JobRuns_JobRunId",
                        column: x => x.JobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobRunItems_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobRunItems_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Key", "CreatedAtUtc", "Description", "IsSensitive", "UpdatedAtUtc", "UpdatedByUserId", "Value", "ValueType" },
                values: new object[,]
                {
                    { "Audit.RetentionDays", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), "So ngay giu audit log.", false, null, null, "365", "Int" },
                    { "Email.EnableSyncErrorSummary", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Bat gui email tong hop loi dong bo.", false, null, null, "true", "Bool" },
                    { "Jobs.ApiRetryCount", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), "So lan retry khi API loi.", false, null, null, "2", "Int" },
                    { "Jobs.ApiRetryDelaySeconds", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Thoi gian cho co dinh giua cac lan retry.", false, null, null, "5", "Int" },
                    { "Jobs.ApiTimeoutSeconds", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Timeout cho tung API call.", false, null, null, "30", "Int" },
                    { "Jobs.ScheduleTime", new DateTime(2026, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Gio chay job hang ngay theo mui gio Asia/Bangkok.", false, null, null, "23:59", "String" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Module",
                table: "AuditLogs",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredAtUtc",
                table: "AuditLogs",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyBankTransactionSummaries_BusinessDate_ParkId_TransactionType",
                table: "DailyBankTransactionSummaries",
                columns: new[] { "BusinessDate", "ParkId", "TransactionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyBankTransactionSummaries_CreatedByUserId",
                table: "DailyBankTransactionSummaries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyBankTransactionSummaries_ParkId",
                table: "DailyBankTransactionSummaries",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyBankTransactionSummaries_PaymentType",
                table: "DailyBankTransactionSummaries",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_DailyBankTransactionSummaries_RawResponseId",
                table: "DailyBankTransactionSummaries",
                column: "RawResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyBankTransactionSummaries_SourceJobRunId",
                table: "DailyBankTransactionSummaries",
                column: "SourceJobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyBankTransactionSummaries_SourceJobRunItemId",
                table: "DailyBankTransactionSummaries",
                column: "SourceJobRunItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyBankTransactionSummaries_SourceType",
                table: "DailyBankTransactionSummaries",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_DailyBankTransactionSummaries_UpdatedByUserId",
                table: "DailyBankTransactionSummaries",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyParkBalanceSnapshots_BusinessDate_ParkId",
                table: "DailyParkBalanceSnapshots",
                columns: new[] { "BusinessDate", "ParkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyParkBalanceSnapshots_CreatedByUserId",
                table: "DailyParkBalanceSnapshots",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyParkBalanceSnapshots_ParkId",
                table: "DailyParkBalanceSnapshots",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyParkBalanceSnapshots_PaymentType",
                table: "DailyParkBalanceSnapshots",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_DailyParkBalanceSnapshots_RawResponseId",
                table: "DailyParkBalanceSnapshots",
                column: "RawResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyParkBalanceSnapshots_SourceJobRunId",
                table: "DailyParkBalanceSnapshots",
                column: "SourceJobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyParkBalanceSnapshots_SourceJobRunItemId",
                table: "DailyParkBalanceSnapshots",
                column: "SourceJobRunItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyParkBalanceSnapshots_SourceType",
                table: "DailyParkBalanceSnapshots",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_DailyParkBalanceSnapshots_UpdatedByUserId",
                table: "DailyParkBalanceSnapshots",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTicketCostSummaries_BusinessDate_ParkId",
                table: "DailyTicketCostSummaries",
                columns: new[] { "BusinessDate", "ParkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyTicketCostSummaries_CreatedByUserId",
                table: "DailyTicketCostSummaries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTicketCostSummaries_ParkId",
                table: "DailyTicketCostSummaries",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTicketCostSummaries_PaymentType",
                table: "DailyTicketCostSummaries",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTicketCostSummaries_RawResponseId",
                table: "DailyTicketCostSummaries",
                column: "RawResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTicketCostSummaries_SourceJobRunId",
                table: "DailyTicketCostSummaries",
                column: "SourceJobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTicketCostSummaries_SourceJobRunItemId",
                table: "DailyTicketCostSummaries",
                column: "SourceJobRunItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTicketCostSummaries_SourceType",
                table: "DailyTicketCostSummaries",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTicketCostSummaries_UpdatedByUserId",
                table: "DailyTicketCostSummaries",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalApiRawResponses_JobRunId",
                table: "ExternalApiRawResponses",
                column: "JobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalApiRawResponses_JobRunItemId",
                table: "ExternalApiRawResponses",
                column: "JobRunItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalApiRawResponses_ParkId",
                table: "ExternalApiRawResponses",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalApiRawResponses_ReceivedAtUtc",
                table: "ExternalApiRawResponses",
                column: "ReceivedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalApiRawResponses_Source_BusinessDate_ParkId",
                table: "ExternalApiRawResponses",
                columns: new[] { "Source", "BusinessDate", "ParkId" });

            migrationBuilder.CreateIndex(
                name: "IX_JobRunItems_BusinessDate_ParkId_Source",
                table: "JobRunItems",
                columns: new[] { "BusinessDate", "ParkId", "Source" });

            migrationBuilder.CreateIndex(
                name: "IX_JobRunItems_JobRunId",
                table: "JobRunItems",
                column: "JobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRunItems_ParkId",
                table: "JobRunItems",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRunItems_RawResponseId",
                table: "JobRunItems",
                column: "RawResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRunItems_ResolvedAtUtc",
                table: "JobRunItems",
                column: "ResolvedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_JobRunItems_ResolvedByUserId",
                table: "JobRunItems",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRunItems_Status",
                table: "JobRunItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobRuns_JobName_BusinessDate",
                table: "JobRuns",
                columns: new[] { "JobName", "BusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_JobRuns_StartedAtUtc",
                table: "JobRuns",
                column: "StartedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_JobRuns_Status",
                table: "JobRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobRuns_TriggeredByUserId",
                table: "JobRuns",
                column: "TriggeredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId_EventType",
                table: "NotificationPreferences",
                columns: new[] { "UserId", "EventType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_CreatedByUserId",
                table: "NotificationRecipients",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_IsActive",
                table: "NotificationRecipients",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_NotificationType_Email",
                table: "NotificationRecipients",
                columns: new[] { "NotificationType", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_UpdatedByUserId",
                table: "NotificationRecipients",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkReconciliations_BusinessDate_ParkId",
                table: "ParkReconciliations",
                columns: new[] { "BusinessDate", "ParkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParkReconciliations_CreatedByUserId",
                table: "ParkReconciliations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkReconciliations_LastBuiltJobRunId",
                table: "ParkReconciliations",
                column: "LastBuiltJobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkReconciliations_ParkId",
                table: "ParkReconciliations",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkReconciliations_PaymentType",
                table: "ParkReconciliations",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_ParkReconciliations_ResolvedAtUtc",
                table: "ParkReconciliations",
                column: "ResolvedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ParkReconciliations_ResolvedByUserId",
                table: "ParkReconciliations",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkReconciliations_Status",
                table: "ParkReconciliations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ParkReconciliations_UpdatedByUserId",
                table: "ParkReconciliations",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkRefunds_BookingCode",
                table: "ParkRefunds",
                column: "BookingCode");

            migrationBuilder.CreateIndex(
                name: "IX_ParkRefunds_CreatedByUserId",
                table: "ParkRefunds",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkRefunds_CustomerRefundStatus",
                table: "ParkRefunds",
                column: "CustomerRefundStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ParkRefunds_DeletedByUserId",
                table: "ParkRefunds",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkRefunds_IsDeleted",
                table: "ParkRefunds",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ParkRefunds_ParkId",
                table: "ParkRefunds",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkRefunds_ParkRefundStatus",
                table: "ParkRefunds",
                column: "ParkRefundStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ParkRefunds_PaymentType",
                table: "ParkRefunds",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_ParkRefunds_RefundDate",
                table: "ParkRefunds",
                column: "RefundDate");

            migrationBuilder.CreateIndex(
                name: "IX_ParkRefunds_UpdatedByUserId",
                table: "ParkRefunds",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_Code",
                table: "Parks",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parks_CreatedByUserId",
                table: "Parks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_DeletedByUserId",
                table: "Parks",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_IsDeleted",
                table: "Parks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_Name",
                table: "Parks",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_PaymentType",
                table: "Parks",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_Status",
                table: "Parks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_UpdatedByUserId",
                table: "Parks",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkTicketTypes_CreatedByUserId",
                table: "ParkTicketTypes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkTicketTypes_DeletedByUserId",
                table: "ParkTicketTypes",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkTicketTypes_IsDeleted",
                table: "ParkTicketTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ParkTicketTypes_ParkId",
                table: "ParkTicketTypes",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkTicketTypes_ParkId_Code",
                table: "ParkTicketTypes",
                columns: new[] { "ParkId", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_ParkTicketTypes_ParkId_TicketTypeCode",
                table: "ParkTicketTypes",
                columns: new[] { "ParkId", "TicketTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParkTicketTypes_Status",
                table: "ParkTicketTypes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ParkTicketTypes_UpdatedByUserId",
                table: "ParkTicketTypes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_UpdatedByUserId",
                table: "SystemSettings",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedByUserId",
                table: "Users",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletedByUserId",
                table: "Users",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted",
                table: "Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Status",
                table: "Users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UpdatedByUserId",
                table: "Users",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ExpiresAtUtc",
                table: "UserSessions",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_RefreshTokenHash",
                table: "UserSessions",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ReplacedBySessionId",
                table: "UserSessions",
                column: "ReplacedBySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_RevokedAtUtc",
                table: "UserSessions",
                column: "RevokedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_RevokedByUserId",
                table: "UserSessions",
                column: "RevokedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyBankTransactionSummaries_ExternalApiRawResponses_RawResponseId",
                table: "DailyBankTransactionSummaries",
                column: "RawResponseId",
                principalTable: "ExternalApiRawResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyBankTransactionSummaries_JobRunItems_SourceJobRunItemId",
                table: "DailyBankTransactionSummaries",
                column: "SourceJobRunItemId",
                principalTable: "JobRunItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyParkBalanceSnapshots_ExternalApiRawResponses_RawResponseId",
                table: "DailyParkBalanceSnapshots",
                column: "RawResponseId",
                principalTable: "ExternalApiRawResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyParkBalanceSnapshots_JobRunItems_SourceJobRunItemId",
                table: "DailyParkBalanceSnapshots",
                column: "SourceJobRunItemId",
                principalTable: "JobRunItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyTicketCostSummaries_ExternalApiRawResponses_RawResponseId",
                table: "DailyTicketCostSummaries",
                column: "RawResponseId",
                principalTable: "ExternalApiRawResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyTicketCostSummaries_JobRunItems_SourceJobRunItemId",
                table: "DailyTicketCostSummaries",
                column: "SourceJobRunItemId",
                principalTable: "JobRunItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalApiRawResponses_JobRunItems_JobRunItemId",
                table: "ExternalApiRawResponses",
                column: "JobRunItemId",
                principalTable: "JobRunItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobRunItems_Users_ResolvedByUserId",
                table: "JobRunItems");

            migrationBuilder.DropForeignKey(
                name: "FK_JobRuns_Users_TriggeredByUserId",
                table: "JobRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_Parks_Users_CreatedByUserId",
                table: "Parks");

            migrationBuilder.DropForeignKey(
                name: "FK_Parks_Users_DeletedByUserId",
                table: "Parks");

            migrationBuilder.DropForeignKey(
                name: "FK_Parks_Users_UpdatedByUserId",
                table: "Parks");

            migrationBuilder.DropForeignKey(
                name: "FK_JobRunItems_ExternalApiRawResponses_RawResponseId",
                table: "JobRunItems");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DailyBankTransactionSummaries");

            migrationBuilder.DropTable(
                name: "DailyParkBalanceSnapshots");

            migrationBuilder.DropTable(
                name: "DailyTicketCostSummaries");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "NotificationRecipients");

            migrationBuilder.DropTable(
                name: "ParkReconciliations");

            migrationBuilder.DropTable(
                name: "ParkRefunds");

            migrationBuilder.DropTable(
                name: "ParkTicketTypes");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ExternalApiRawResponses");

            migrationBuilder.DropTable(
                name: "JobRunItems");

            migrationBuilder.DropTable(
                name: "JobRuns");

            migrationBuilder.DropTable(
                name: "Parks");
        }
    }
}
