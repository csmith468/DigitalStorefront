using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Tests;

[Route("test")]
public class TestController : ControllerBase
{
    [HttpGet("valid")]
    public ActionResult GetValid()
    {
        return Ok("Success!");
    }
    
    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret()
    {
        return Ok("You are logged in.");
    }

    [HttpGet("not-found")]
    public ActionResult<string> GetNotFound()
    {
        return NotFound("Not Found");
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
        return BadRequest("Bad Request");
    }

    [HttpGet("unauthorized")]
    public ActionResult<string> GetUnauthorized()
    {
        return Unauthorized();
    }

    [HttpGet("throw-exception")]
    public ActionResult<string> ThrowException()
    {
        throw new Exception("Test Exception");
    }

    [HttpGet("null-reference")]
    public ActionResult<string> ThrowNullReference()
    {
        string? nullString = null;
        return Ok(nullString!.Length);
    }

    [HttpGet("get-bearer-string")] 
    public ActionResult<object> GetBearerString()
    {
        var bearerToken = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(bearerToken))
            return BadRequest("No authorization header found. Please try login endpoint first, then paste \"Bearer token\" into Authorize.");

        var userId = User.FindFirst("userId")?.Value;
        return Ok(new {
            bearerToken,
            isAuthenticated = User.Identity?.IsAuthenticated ?? false,
            userId
        });
    }
}