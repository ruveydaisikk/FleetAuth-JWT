using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetAuth.API.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly() => Ok("Admin paneline hoş geldin.");

    [HttpGet("manager")]
    [Authorize(Roles = "Admin,FleetManager")]
    public IActionResult ManagerArea() => Ok("Filo yönetim alanı.");

    [HttpGet("driver")]
    [Authorize(Roles = "Admin,FleetManager,Driver")]
    public IActionResult DriverArea() => Ok("Sürücü paneli.");

    [HttpGet("public")]
    public IActionResult PublicArea() => Ok("Herkese açık endpoint.");
}