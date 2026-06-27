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
}
