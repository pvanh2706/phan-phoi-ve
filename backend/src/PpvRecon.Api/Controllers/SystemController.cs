using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PpvRecon.Api.Services;
using PpvRecon.Application.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/system")]
public sealed class SystemController(IDataResetService dataResetService) : PpvControllerBase
{
    public const string ResetConfirmPhrase = "XÓA TOÀN BỘ DỮ LIỆU";

    [HttpPost("reset-data")]
    public async Task<ActionResult<ApiResponse<ResetDataResultDto>>> ResetData(
        ResetDataRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(request.ConfirmText?.Trim(), ResetConfirmPhrase, StringComparison.Ordinal))
        {
            return BadRequest(ApiResponse<ResetDataResultDto>.Fail(
                $"Vui lòng gõ chính xác cụm từ xác nhận: \"{ResetConfirmPhrase}\"."));
        }

        var result = await dataResetService.ResetKeepingAdminsAsync(CurrentUserId, cancellationToken);
        return Ok(ApiResponse<ResetDataResultDto>.Ok(
            result,
            $"Đã xóa {result.TotalDeleted} bản ghi, giữ lại {result.KeptAdminCount} tài khoản Admin và cấu hình hệ thống."));
    }
}
