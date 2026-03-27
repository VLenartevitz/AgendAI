using Microsoft.AspNetCore.Mvc;

namespace AgendAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    [HttpGet("message")]
    public ActionResult<string> GetMessage()
    {
        return "Texto vindo da API: bem-vindo ao AgendAIsssssssssssssssssssssss.";
    }
}
