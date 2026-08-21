using Academy.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Student;

[ApiController]
[Authorize(Roles = AppRoles.Student)]
[Route("api/student")]
public sealed class StudentDashboardController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetDashboard() =>
        Ok(new { message = "Student dashboard" });
}
