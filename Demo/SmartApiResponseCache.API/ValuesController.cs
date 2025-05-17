using Microsoft.AspNetCore.Mvc;

namespace SmartApiResponseCache.API;
[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{

    [Middleware.Attributes.EnabledSmartCache()]
    public IActionResult Algo()
    {
        return Ok();
    }
}
