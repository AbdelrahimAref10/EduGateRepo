using Academy.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Teacher;

[ApiController]
[Authorize(Roles = AppRoles.Teacher)]
[Route("api/teacher")]
public sealed class TeacherDashboardController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetDashboard() =>
        Ok(new { message = "Teacher dashboard" });
}
