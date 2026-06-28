using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Common;
using PpvRecon.Application.Workflow;
using PpvRecon.Domain.Entities.Workflow;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workflow")]
public sealed class WorkflowController(PpvReconDbContext dbContext) : PpvControllerBase
{
    // ── Board ──
    [HttpGet("board")]
    public async Task<ActionResult<ApiResponse<WorkflowBoardDto>>> GetBoard(CancellationToken cancellationToken)
    {
        var columns = await dbContext.WorkflowColumns.AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        var tasks = await dbContext.WorkflowTasks.AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var parkNames = await dbContext.Parks.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Select(p => new { p.Id, p.Name })
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        var users = await dbContext.Users.AsNoTracking()
            .Where(u => !u.IsDeleted && u.Status == UserStatus.Active)
            .OrderBy(u => u.FullName)
            .Select(u => new WorkflowUserOptionDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Initials = Initials(u.FullName),
                Role = u.Role,
            })
            .ToListAsync(cancellationToken);

        var board = new WorkflowBoardDto
        {
            Users = users,
            Columns = columns.Select(c => new WorkflowColumnDto
            {
                Id = c.Id,
                ColumnKey = c.ColumnKey,
                Title = c.Title,
                HeadTone = c.HeadTone,
                CardStatusLabel = c.CardStatusLabel,
                CardTone = c.CardTone,
                SortOrder = c.SortOrder,
                VisibleFields = SplitCsv(c.VisibleFields),
                PermittedUserIds = SplitCsvInts(c.PermittedUserIds),
                Tasks = tasks.Where(t => t.ColumnId == c.Id).Select(t => new WorkflowTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    PaymentType = t.PaymentType,
                    ParkId = t.ParkId,
                    ParkName = t.ParkId != null && parkNames.TryGetValue(t.ParkId.Value, out var name) ? name : null,
                    BankAccount = t.BankAccount,
                    BankName = t.BankName,
                    Amount = t.Amount,
                    ExecuteDate = t.ExecuteDate,
                    Note = t.Note,
                    ColumnId = t.ColumnId,
                    SortOrder = t.SortOrder,
                }).ToList(),
            }).ToList(),
        };

        return Ok(ApiResponse<WorkflowBoardDto>.Ok(board));
    }

    // ── Tasks ──
    [HttpPost("tasks")]
    public async Task<ActionResult<ApiResponse<WorkflowTaskDto>>> CreateTask(
        CreateWorkflowTaskRequest request,
        CancellationToken cancellationToken)
    {
        var columnId = request.ColumnId
            ?? await dbContext.WorkflowColumns.OrderBy(x => x.SortOrder).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);

        var columnExists = await dbContext.WorkflowColumns.AnyAsync(x => x.Id == columnId, cancellationToken);
        if (!columnExists)
        {
            return BadRequest(ApiResponse<WorkflowTaskDto>.Fail("Cột không hợp lệ."));
        }

        if (request.ParkId is int parkId && !await dbContext.Parks.AnyAsync(p => p.Id == parkId && !p.IsDeleted, cancellationToken))
        {
            return BadRequest(ApiResponse<WorkflowTaskDto>.Fail("KVC không tồn tại."));
        }

        var nextOrder = await NextSortOrder(columnId, cancellationToken);

        var task = new WorkflowTask
        {
            Title = request.Title.Trim(),
            PaymentType = request.PaymentType,
            ParkId = request.ParkId,
            BankAccount = Clean(request.BankAccount),
            BankName = Clean(request.BankName),
            Amount = request.Amount,
            ExecuteDate = request.ExecuteDate,
            Note = Clean(request.Note),
            ColumnId = columnId,
            SortOrder = nextOrder,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = CurrentUserId,
        };

        dbContext.WorkflowTasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<WorkflowTaskDto>.Ok(await ToDto(task, cancellationToken), "Tạo nhiệm vụ thành công."));
    }

    [HttpPut("tasks/{id:int}")]
    public async Task<ActionResult<ApiResponse<WorkflowTaskDto>>> UpdateTask(
        int id,
        UpdateWorkflowTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.WorkflowTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
        {
            return NotFound(ApiResponse<WorkflowTaskDto>.Fail("Không tìm thấy nhiệm vụ."));
        }

        if (request.ParkId is int parkId && !await dbContext.Parks.AnyAsync(p => p.Id == parkId && !p.IsDeleted, cancellationToken))
        {
            return BadRequest(ApiResponse<WorkflowTaskDto>.Fail("KVC không tồn tại."));
        }

        task.Title = request.Title.Trim();
        task.PaymentType = request.PaymentType;
        task.ParkId = request.ParkId;
        task.BankAccount = Clean(request.BankAccount);
        task.BankName = Clean(request.BankName);
        task.Amount = request.Amount;
        task.ExecuteDate = request.ExecuteDate;
        task.Note = Clean(request.Note);
        task.UpdatedAtUtc = DateTime.UtcNow;
        task.UpdatedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<WorkflowTaskDto>.Ok(await ToDto(task, cancellationToken), "Cập nhật nhiệm vụ thành công."));
    }

    [HttpPut("tasks/{id:int}/move")]
    public async Task<ActionResult<ApiResponse<WorkflowTaskDto>>> MoveTask(
        int id,
        MoveWorkflowTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.WorkflowTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
        {
            return NotFound(ApiResponse<WorkflowTaskDto>.Fail("Không tìm thấy nhiệm vụ."));
        }

        if (!await dbContext.WorkflowColumns.AnyAsync(x => x.Id == request.ColumnId, cancellationToken))
        {
            return BadRequest(ApiResponse<WorkflowTaskDto>.Fail("Cột đích không hợp lệ."));
        }

        if (task.ColumnId != request.ColumnId)
        {
            task.ColumnId = request.ColumnId;
            task.SortOrder = await NextSortOrder(request.ColumnId, cancellationToken);
            task.UpdatedAtUtc = DateTime.UtcNow;
            task.UpdatedByUserId = CurrentUserId;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(ApiResponse<WorkflowTaskDto>.Ok(await ToDto(task, cancellationToken), "Đã chuyển nhiệm vụ."));
    }

    [HttpDelete("tasks/{id:int}")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteTask(int id, CancellationToken cancellationToken)
    {
        var task = await dbContext.WorkflowTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy nhiệm vụ."));
        }

        dbContext.WorkflowTasks.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Đã xóa nhiệm vụ."));
    }

    // ── Column settings ──
    [HttpPut("columns/{id:int}/settings")]
    public async Task<ActionResult<ApiResponse<WorkflowColumnDto>>> UpdateColumnSettings(
        int id,
        UpdateWorkflowColumnSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var column = await dbContext.WorkflowColumns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (column is null)
        {
            return NotFound(ApiResponse<WorkflowColumnDto>.Fail("Không tìm thấy cột."));
        }

        var validUserIds = await dbContext.Users
            .Where(u => !u.IsDeleted && request.PermittedUserIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        column.VisibleFields = string.Join(',', request.VisibleFields.Select(f => f.Trim()).Where(f => f.Length > 0));
        column.PermittedUserIds = string.Join(',', validUserIds);
        column.UpdatedAtUtc = DateTime.UtcNow;
        column.UpdatedByUserId = CurrentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<WorkflowColumnDto>.Ok(new WorkflowColumnDto
        {
            Id = column.Id,
            ColumnKey = column.ColumnKey,
            Title = column.Title,
            HeadTone = column.HeadTone,
            CardStatusLabel = column.CardStatusLabel,
            CardTone = column.CardTone,
            SortOrder = column.SortOrder,
            VisibleFields = SplitCsv(column.VisibleFields),
            PermittedUserIds = SplitCsvInts(column.PermittedUserIds),
        }, "Đã lưu cấu hình cột."));
    }

    // ── Helpers ──
    private async Task<int> NextSortOrder(int columnId, CancellationToken cancellationToken)
    {
        var max = await dbContext.WorkflowTasks
            .Where(x => x.ColumnId == columnId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken);
        return (max ?? 0) + 1;
    }

    private async Task<WorkflowTaskDto> ToDto(WorkflowTask task, CancellationToken cancellationToken)
    {
        string? parkName = null;
        if (task.ParkId is int parkId)
        {
            parkName = await dbContext.Parks.AsNoTracking()
                .Where(p => p.Id == parkId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new WorkflowTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            PaymentType = task.PaymentType,
            ParkId = task.ParkId,
            ParkName = parkName,
            BankAccount = task.BankAccount,
            BankName = task.BankName,
            Amount = task.Amount,
            ExecuteDate = task.ExecuteDate,
            Note = task.Note,
            ColumnId = task.ColumnId,
            SortOrder = task.SortOrder,
        };
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<string> SplitCsv(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? new List<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static List<int> SplitCsvInts(string? csv) =>
        SplitCsv(csv).Select(x => int.TryParse(x, out var n) ? n : (int?)null)
            .Where(n => n.HasValue).Select(n => n!.Value).ToList();

    private static string Initials(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        if (parts.Length == 1) return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
        return $"{parts[^2][0]}{parts[^1][0]}".ToUpperInvariant();
    }
}
