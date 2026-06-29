using Microsoft.EntityFrameworkCore;
using PpvRecon.Domain.Entities.Workflow;

namespace PpvRecon.Infrastructure.Persistence;

/// <summary>
/// Đảm bảo 5 cột cố định của bảng "Quy trình nạp tiền KVC" luôn tồn tại.
/// Idempotent: chỉ thêm những cột còn thiếu (so theo <see cref="WorkflowColumn.ColumnKey"/>),
/// không động vào cấu hình người dùng đã chỉnh trên các cột đang có.
/// </summary>
public static class WorkflowBoardSeeder
{
    public static async Task EnsureColumnsAsync(
        PpvReconDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var existingKeys = await dbContext.WorkflowColumns
            .Select(c => c.ColumnKey)
            .ToListAsync(cancellationToken);

        var existing = existingKeys.ToHashSet();

        var missing = WorkflowBoardDefaults.Columns
            .Where(c => !existing.Contains(c.ColumnKey))
            .Select(c => new WorkflowColumn
            {
                // Không gán Id để tránh xung đột khóa chính với cột đang có;
                // ColumnKey (unique) mới là định danh ổn định của cột.
                ColumnKey = c.ColumnKey,
                Title = c.Title,
                HeadTone = c.HeadTone,
                CardStatusLabel = c.CardStatusLabel,
                CardTone = c.CardTone,
                SortOrder = c.SortOrder,
                VisibleFields = c.VisibleFields,
                PermittedUserIds = c.PermittedUserIds,
                CreatedAtUtc = c.CreatedAtUtc,
            })
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        dbContext.WorkflowColumns.AddRange(missing);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
