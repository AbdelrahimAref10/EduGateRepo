using Academy.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.SuperAdmin;

[ApiController]
[Authorize(Roles = AppRoles.SuperAdmin)]
[Route("api/super-admin")]
public sealed class SuperAdminDashboardController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetDashboard() =>
        Ok(new { message = "SuperAdmin dashboard" });
}
