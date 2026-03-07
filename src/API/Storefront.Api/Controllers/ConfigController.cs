using Microsoft.AspNetCore.Mvc;

namespace Storefront.Api.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _config;

    public ConfigController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("mode")]
    public IActionResult GetMode()
    {
        var mode = _config["AppMode"] ?? "B2B";
        return Ok(new { mode });
    }
}
