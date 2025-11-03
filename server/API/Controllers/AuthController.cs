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

    public AuthController(IUserService userService, IAuthService authService)
    {
        _authService = authService;
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(UserRegisterDto userDto)
    {
        return (await _authService.RegisterUser(userDto)).ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto userDto)
    {
        return (await _authService.LoginUser(userDto)).ToActionResult();
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken()
    {
        var userIdStr = User.FindFirst("userId")?.Value;
        if (userIdStr == null) 
            return Unauthorized("Invalid token.");
        return (await _authService.RefreshToken(userIdStr)).ToActionResult();
    }
}