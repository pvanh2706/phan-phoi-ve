using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Auditing;
using PpvRecon.Domain.Entities.Workflow;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

public sealed class ResetDataRequest
{
    [Required(ErrorMessage = "Cụm từ xác nhận là bắt buộc.")]
    public string ConfirmText { get; set; } = string.Empty;
}

public sealed class ResetDataResultDto
{
    public int KeptAdminCount { get; set; }
    public int TotalDeleted { get; set; }
    public Dictionary<string, int> DeletedByTable { get; set; } = new();
}

public interface IDataResetService
{
    Task<ResetDataResultDto> ResetKeepingAdminsAsync(
        int? triggeredByUserId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Xóa toàn bộ dữ liệu nghiệp vụ, chỉ giữ lại tài khoản Admin (đang hoạt động)
/// và bảng cấu hình hệ thống <c>SystemSettings</c>.
/// </summary>
public sealed class DataResetService(
    PpvReconDbContext dbContext,
    IAuditService auditService) : IDataResetService
{
    // Khóa ngoại đều là DeleteBehavior.Restrict và có quan hệ vòng
    // (JobRunItems ↔ ExternalApiRawResponses), nên ta tắt kiểm tra FK trong
    // lúc xóa thay vì cố gắng sắp đúng thứ tự. Danh sách vẫn để theo thứ tự
    // con → cha cho dễ đọc.
    private static readonly string[] FullClearTables =
    [
        "AuditLogs",
        "WorkflowTasks",
        // CHÚ Ý: KHÔNG xóa "WorkflowColumns" ở đây. 5 cột là dữ liệu CẤU TRÚC của
        // "Quy trình nạp tiền KVC", phải luôn tồn tại để chức năng hoạt động.
        // Chúng được đưa về mặc định (factory) ở bước riêng bên dưới, không bị xóa.
        "ParkReconciliations",
        "BankTransactionDetails",
        "DailyBankTransactionSummaries",
        "DailyTicketCostSummaries",
        "DailyParkBalanceSnapshots",
        "TicketSaleCostDetails",
        "JobRunItems",
        "ExternalApiRawResponses",
        "JobRuns",
        "ParkRefunds",
        "ParkTicketTypes",
        "Parks",
        "NotificationRecipients",
    ];

    // Subquery xác định "Admin còn hiệu lực" — những user duy nhất được giữ lại.
    private const string AdminFilter =
        "(SELECT Id FROM Users WHERE Role = 'Admin' AND IsDeleted = 0)";

    public async Task<ResetDataResultDto> ResetKeepingAdminsAsync(
        int? triggeredByUserId,
        CancellationToken cancellationToken = default)
    {
        var result = new ResetDataResultDto();
        var connection = dbContext.Database.GetDbConnection();
        var wasClosed = connection.State != ConnectionState.Open;
        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            // PRAGMA phải đặt NGOÀI transaction mới có hiệu lực.
            await ExecuteAsync(connection, null, "PRAGMA foreign_keys = OFF", cancellationToken);

            await using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
            {
                foreach (var table in FullClearTables)
                {
                    await AccumulateAsync(result, connection, transaction, table, $"DELETE FROM {table}", cancellationToken);
                }

                // Cột quy trình KVC: GIỮ lại cột, chỉ đưa cấu hình về mặc định và gỡ
                // tham chiếu tới user sắp xóa (CreatedBy/UpdatedBy là FK tới Users).
                await ExecuteAsync(connection, transaction,
                    $"UPDATE WorkflowColumns SET VisibleFields = '{WorkflowBoardDefaults.DefaultVisibleFields}', " +
                    "PermittedUserIds = '', CreatedByUserId = NULL, UpdatedByUserId = NULL, UpdatedAtUtc = NULL",
                    cancellationToken);

                // Bảng gắn với user: chỉ giữ dòng thuộc về Admin còn hiệu lực.
                foreach (var table in new[] { "UserSessions", "UserPreferences", "NotificationPreferences" })
                {
                    await AccumulateAsync(result, connection, transaction, table,
                        $"DELETE FROM {table} WHERE UserId NOT IN {AdminFilter}", cancellationToken);
                }

                // SystemSettings được giữ lại nhưng có thể trỏ UpdatedByUserId tới user
                // sắp xóa → gỡ tham chiếu để không còn khóa ngoại "treo".
                await ExecuteAsync(connection, transaction,
                    $"UPDATE SystemSettings SET UpdatedByUserId = NULL " +
                    $"WHERE UpdatedByUserId IS NOT NULL AND UpdatedByUserId NOT IN {AdminFilter}",
                    cancellationToken);

                // Cuối cùng xóa mọi user không phải Admin (hoặc Admin đã bị soft-delete).
                await AccumulateAsync(result, connection, transaction, "Users",
                    "DELETE FROM Users WHERE NOT (Role = 'Admin' AND IsDeleted = 0)", cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }

            await ExecuteAsync(connection, null, "PRAGMA foreign_keys = ON", cancellationToken);
        }
        finally
        {
            if (wasClosed)
            {
                await connection.CloseAsync();
            }
        }

        // Các entity đang được EF theo dõi giờ đã lỗi thời sau khi xóa bằng SQL thô.
        dbContext.ChangeTracker.Clear();

        // Bổ sung lại bất kỳ cột quy trình nào còn thiếu (phòng khi DB đã từng bị
        // xóa mất cột trước đây), đảm bảo bảng KVC luôn đủ 5 cột sau khi reset.
        await WorkflowBoardSeeder.EnsureColumnsAsync(dbContext, cancellationToken);

        result.KeptAdminCount = await dbContext.Users
            .CountAsync(x => x.Role == UserRole.Admin && !x.IsDeleted, cancellationToken);

        await auditService.LogAsync(new AuditLogEntry
        {
            UserId = triggeredByUserId,
            Module = "System",
            EntityName = "Database",
            EntityId = "reset-data",
            Action = AuditAction.ResetData,
            After = result,
        }, cancellationToken);

        return result;
    }

    private static async Task AccumulateAsync(
        ResetDataResultDto result,
        DbConnection connection,
        DbTransaction transaction,
        string table,
        string sql,
        CancellationToken cancellationToken)
    {
        var deleted = await ExecuteAsync(connection, transaction, sql, cancellationToken);
        if (deleted > 0)
        {
            result.DeletedByTable[table] = deleted;
            result.TotalDeleted += deleted;
        }
    }

    private static async Task<int> ExecuteAsync(
        DbConnection connection,
        DbTransaction? transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
