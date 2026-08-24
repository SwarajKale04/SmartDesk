using Microsoft.AspNetCore.Mvc;

namespace SmartDesk.API.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("info")]
    public IActionResult GetInfo() => Ok(new { name = "SmartDesk API", version = "1.0.0", environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production" });
}
