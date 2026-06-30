using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PpvRecon.Api.Services.Settings;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/connection-settings")]
public sealed class ConnectionSettingsController(
    IConnectionSettingsService connectionSettings,
    IAuditService auditService) : PpvControllerBase
{
    /// <summary>Lấy toàn bộ cấu hình kết nối hiệu lực (DB đè lên appsettings) cho màn cấu hình.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<ConnectionSettingsDto>>> Get(CancellationToken cancellationToken)
    {
        var dto = await connectionSettings.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<ConnectionSettingsDto>.Ok(dto));
    }

    /// <summary>Lưu cấu hình kết nối (áp dụng ngay cho các lần lấy dữ liệu sau đó).</summary>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<ConnectionSettingsDto>>> Save(
        ConnectionSettingsDto request,
        CancellationToken cancellationToken)
    {
        await connectionSettings.SaveAsync(request, CurrentUserId, cancellationToken);

        await auditService.LogAsync(new AuditLogEntry
        {
            UserId = CurrentUserId,
            Module = "System",
            EntityName = "ConnectionSettings",
            EntityId = "connection-settings",
            Action = AuditAction.Update,
        }, cancellationToken);

        var dto = await connectionSettings.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<ConnectionSettingsDto>.Ok(dto, "Đã lưu cấu hình kết nối."));
    }

    [HttpPost("test/mail")]
    public async Task<ActionResult<ApiResponse<ConnectionTestResult>>> TestMail(
        MailConnectionDto request,
        CancellationToken cancellationToken)
    {
        var result = await connectionSettings.TestMailAsync(request, cancellationToken);
        return Ok(ApiResponse<ConnectionTestResult>.Ok(result, result.Message));
    }

    [HttpPost("test/park-balance")]
    public async Task<ActionResult<ApiResponse<ConnectionTestResult>>> TestParkBalance(
        ParkBalanceConnectionDto request,
        CancellationToken cancellationToken)
    {
        var result = await connectionSettings.TestParkBalanceAsync(request, cancellationToken);
        return Ok(ApiResponse<ConnectionTestResult>.Ok(result, result.Message));
    }

    [HttpPost("test/oneinventory")]
    public async Task<ActionResult<ApiResponse<ConnectionTestResult>>> TestOneInventory(
        OneInventoryConnectionDto request,
        CancellationToken cancellationToken)
    {
        var result = await connectionSettings.TestOneInventoryAsync(request, cancellationToken);
        return Ok(ApiResponse<ConnectionTestResult>.Ok(result, result.Message));
    }
}
