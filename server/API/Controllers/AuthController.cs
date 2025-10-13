using API.Extensions;
using API.Models.Dtos;
using API.Services;
using API.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("auth")]
public class AuthController(ISharedContainer container) : BaseController(container)
{
    private IUserService UserService => DepInj<IUserService>();
    private IAuthService AuthService => DepInj<IAuthService>();
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(UserRegisterDto userDto)
    {
        return (await AuthService.RegisterUser(userDto)).ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto userDto)
    {
        return (await AuthService.LoginUser(userDto)).ToActionResult();
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken()
    {
        var userIdStr = User.FindFirst("userId")?.Value;
        if (userIdStr == null) 
            return Unauthorized("Invalid token.");
        return (await AuthService.RefreshToken(userIdStr)).ToActionResult();
    }
}