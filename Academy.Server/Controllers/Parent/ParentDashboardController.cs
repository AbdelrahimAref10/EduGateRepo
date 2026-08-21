using Academy.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Parent;

[ApiController]
[Authorize(Roles = AppRoles.Parent)]
[Route("api/parent")]
public sealed class ParentDashboardController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetDashboard() =>
        Ok(new { message = "Parent dashboard" });
}
