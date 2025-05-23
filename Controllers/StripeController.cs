namespace BlazorStripeExample.Controllers;

using Microsoft.AspNetCore.Mvc;


[Route("api/[controller]")]
[ApiController]
public class StripeController : ControllerBase
{
    [HttpGet("test")]
    public IActionResult GetTest()
    {
        return Ok("Stripe test endpoint is working.");
    }

}
