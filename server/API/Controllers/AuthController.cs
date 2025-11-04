using API.Extensions;
using API.Models.Dtos;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> RegisterAsync(UserRegisterDto userDto, CancellationToken ct)
    {
        return (await _authService.RegisterUserAsync(userDto, ct)).ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> LoginAsync(UserLoginDto userDto, CancellationToken ct)
    {
        return (await _authService.LoginUserAsync(userDto, ct)).ToActionResult();
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshTokenAsync(CancellationToken ct)
    {
        var userIdStr = User.FindFirst("userId")?.Value;
        if (userIdStr == null)
            return Unauthorized("Invalid token.");
        return (await _authService.RefreshTokenAsync(userIdStr, ct)).ToActionResult();
    }
}