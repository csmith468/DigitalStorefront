using System.Net;
using API.Models;
using API.Models.DsfTables;
using API.Models.Dtos;
using API.Setup;
using API.Utils;
using API.Extensions;

namespace API.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterUser(UserRegisterDto userDto);
    Task<Result<AuthResponseDto>> LoginUser(UserLoginDto userDto);
    Task<Result<AuthResponseDto>> RefreshToken(string userIdStr);
}

public class AuthService(ISharedContainer container) : BaseService(container), IAuthService
{
    private IUserService UserService => DepInj<IUserService>();
    private TokenGenerator TokenGen => DepInj<TokenGenerator>();
    private PasswordHasher PasswordHasher => DepInj<PasswordHasher>();
    
    public async Task<Result<AuthResponseDto>> RegisterUser(UserRegisterDto userDto)
    {
        var validateResult = await ValidateRegistrationAsync(userDto);
        if (!validateResult.IsSuccess)
            return validateResult.ToFailure<bool, AuthResponseDto>();
        
        var userId = 0;
        await Dapper.WithTransactionAsync(async () =>
        {
            userId = await Dapper.InsertAsync(new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
            });
            await CreateAuthAsync(userId, userDto.Password);
        });

        var token = TokenGen.GenerateToken(userId);
        var response = new AuthResponseDto
        {
            UserId = userId,
            Username = userDto.Username,
            Token = token
        };
        
        return Result<AuthResponseDto>.Success(response, HttpStatusCode.Created);
    }

    public async Task<Result<AuthResponseDto>> LoginUser(UserLoginDto userDto)
    {
        const string errorMessage = "Invalid username or password";
        
        var user = await UserService.GetUserByUsernameAsync(userDto.Username);
        if (user == null)
            return Result<AuthResponseDto>.Failure(errorMessage, HttpStatusCode.Unauthorized);
        
        var userAuth = (await Dapper.GetByFieldAsync<Auth>("userId", user.UserId)).FirstOrDefault();
        if (userAuth == null || !PasswordHasher.VerifyPassword(userDto.Password, userAuth.PasswordSalt, userAuth.PasswordHash))
            return Result<AuthResponseDto>.Failure(errorMessage, HttpStatusCode.Unauthorized);

        return Result<AuthResponseDto>.Success(CreateAuthResponseDtoFromUser(user));
    }

    public async Task<Result<AuthResponseDto>> RefreshToken(string userIdStr)
    {
        if (!int.TryParse(userIdStr, out var userId))
            return Result<AuthResponseDto>.Failure("Invalid token.", HttpStatusCode.Unauthorized);
        
        var user = await UserService.GetUserByIdAsync(userId);
        if (user == null) 
            return Result<AuthResponseDto>.Failure("Invalid token.", HttpStatusCode.Unauthorized);

        return Result<AuthResponseDto>.Success(CreateAuthResponseDtoFromUser(user));
    }
    
    private async Task<Result<bool>> ValidateRegistrationAsync(UserRegisterDto user)
    {
        if (user.Password != user.ConfirmPassword)
            return Result<bool>.Failure("Passwords do not match.", HttpStatusCode.BadRequest);
        if (user.Email is not null && await Dapper.ExistsByFieldAsync<User>("email", user.Email))
            return Result<bool>.Failure("Email already exists.", HttpStatusCode.BadRequest);
        if (await Dapper.ExistsByFieldAsync<User>("username", user.Username))
            return Result<bool>.Failure("Username already exists.", HttpStatusCode.BadRequest);
        return  Result<bool>.Success(true);
    }
    
    private async Task CreateAuthAsync(int userId, string password)
    {
        var (passwordSalt, passwordHash) = PasswordHasher.HashPassword(password);
        await Dapper.InsertAsync(new Auth
        {
            UserId = userId,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        });
    }

    private AuthResponseDto CreateAuthResponseDtoFromUser(User user)
    {
        var response = new AuthResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Token = TokenGen.GenerateToken(user.UserId)
        };
        
        return response;
    }
}