using System.Security.Cryptography;
using API.Models;
using API.Models.DsfTables;
using API.Models.Dtos;
using API.Setup;
using API.Utils;

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
            return Result<AuthResponseDto>.Failure(validateResult.Error!);
        
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
        
        return Result<AuthResponseDto>.Success(response, statusCode: 201);
    }

    public async Task<Result<AuthResponseDto>> LoginUser(UserLoginDto userDto)
    {
        const string errorMessage = "Invalid username or password";
        const int unauthorizedStatusCode = 401;
        
        var user = await UserService.GetUserByUsernameAsync(userDto.Username);
        if (user == null)
            return Result<AuthResponseDto>.Failure(errorMessage, statusCode: unauthorizedStatusCode);
        
        var userAuth = (await Dapper.GetByFieldAsync<Auth>("userId", user.UserId)).FirstOrDefault();
        if (userAuth == null || !PasswordHasher.VerifyPassword(userDto.Password, userAuth.PasswordSalt, userAuth.PasswordHash))
            return Result<AuthResponseDto>.Failure(errorMessage, statusCode: unauthorizedStatusCode);

        return Result<AuthResponseDto>.Success(CreateAuthResponseDtoFromUser(user));
    }

    public async Task<Result<AuthResponseDto>> RefreshToken(string userIdStr)
    {
        if (!int.TryParse(userIdStr, out var userId))
            return Result<AuthResponseDto>.Failure("Invalid token.", statusCode: 401);
        
        var user = await UserService.GetUserByIdAsync(userId);
        if (user == null) 
            return Result<AuthResponseDto>.Failure("Invalid token.", statusCode: 401);

        return Result<AuthResponseDto>.Success(CreateAuthResponseDtoFromUser(user));
    }
    
    private async Task<Result<bool>> ValidateRegistrationAsync(UserRegisterDto user)
    {
        if (user.Password != user.ConfirmPassword)
            return Result<bool>.Failure("Passwords do not match.", statusCode: 400);
        if (user.Email is not null && await Dapper.ExistsByFieldAsync<User>("email", user.Email))
            return Result<bool>.Failure("Email already exists.", statusCode: 400);
        if (await Dapper.ExistsByFieldAsync<User>("username", user.Username))
            return Result<bool>.Failure("Username already exists.", statusCode: 400);
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