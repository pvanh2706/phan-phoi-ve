using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PpvRecon.Api.Controllers;

public abstract class PpvControllerBase : ControllerBase
{
    protected int? CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }

    protected int? CurrentSessionId
    {
        get
        {
            var value = User.FindFirstValue("session_id");
            return int.TryParse(value, out var sessionId) ? sessionId : null;
        }
    }

    /// <summary>Ngày hiện tại theo giờ Việt Nam — mốc "ngày nghiệp vụ" chung của hệ thống.</summary>
    protected static DateOnly GetVietnamToday()
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        }
        catch (TimeZoneNotFoundException)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        }
    }
}
