using System.Net;
using API.Database;
using API.Extensions;
using API.Models;
using API.Models.DsfTables;
using API.Models.Dtos;
using API.Utils;
using Api.Models.DsfTables;

namespace API.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterUser(UserRegisterDto userDto);
    Task<Result<AuthResponseDto>> LoginUser(UserLoginDto userDto);
    Task<Result<AuthResponseDto>> RefreshToken(string userIdStr);
}

public class AuthService : IAuthService
{
    private readonly IDataContextDapper _dapper;
    private readonly ILogger<AuthService> _logger;
    private readonly TokenGenerator _tokenGen;
    private readonly PasswordHasher _passwordHasher;
    private readonly IUserService _userService;
    
    public AuthService(IDataContextDapper dapper,
        ILogger<AuthService> logger,
        TokenGenerator tokenGen,
        PasswordHasher passwordHasher,
        IUserService userService)
    {
        _dapper = dapper;
        _logger = logger;
        _tokenGen = tokenGen;
        _passwordHasher = passwordHasher;
        _userService = userService;
    }
    
    public async Task<Result<AuthResponseDto>> RegisterUser(UserRegisterDto userDto)
    {
        var validateResult = await ValidateRegistrationAsync(userDto);
        if (!validateResult.IsSuccess)
            return validateResult.ToFailure<bool, AuthResponseDto>();
        
        var userId = 0;
        await _dapper.WithTransactionAsync(async () =>
        {
            userId = await _dapper.InsertAsync(new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
            });
            await CreateAuthAsync(userId, userDto.Password);

            var sql = "SELECT * FROM dsf.role WHERE roleName in ('ProductWriter', 'ImageManager')";
            var roles = await _dapper.QueryAsync<Role>(sql);
            foreach (var role in roles)
            {
                var userRole = new UserRole { UserId = userId, RoleId = role.RoleId };
                await _dapper.InsertAsync(userRole);
            }
            
        });
        if (userId == 0)
            return Result<AuthResponseDto>.Failure("Failed to register user.");

        var roles = await GetUserRolesAsync(userId);
        var token = _tokenGen.GenerateToken(userId, roles);
        var response = new AuthResponseDto
        {
            UserId = userId,
            Username = userDto.Username,
            Roles = roles,
            Token = token
        };
        
        _logger.LogInformation("User Registered: UserId: {UserId} Username: {Username}", userId, userDto.Username);
        return Result<AuthResponseDto>.Success(response, HttpStatusCode.Created);
    }

    public async Task<Result<AuthResponseDto>> LoginUser(UserLoginDto userDto)
    {
        const string errorMessage = "Invalid username or password";
        
        var user = await _userService.GetUserByUsernameAsync(userDto.Username);
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", userDto.Username);
            return Result<AuthResponseDto>.Failure(errorMessage, HttpStatusCode.Unauthorized);
        }
        var userAuth = (await _dapper.GetByFieldAsync<Auth>("userId", user.UserId)).FirstOrDefault();
        if (userAuth == null || !_passwordHasher.VerifyPassword(userDto.Password, userAuth.PasswordSalt, userAuth.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", userDto.Username);
            return Result<AuthResponseDto>.Failure(errorMessage, HttpStatusCode.Unauthorized);
        }
        
        _logger.LogInformation("User Registered: UserId: {UserId} Username: {Username}", user.UserId, user.Username);
        return Result<AuthResponseDto>.Success(await CreateAuthResponseDtoFromUser(user));
    }

    public async Task<Result<AuthResponseDto>> RefreshToken(string userIdStr)
    {
        if (!int.TryParse(userIdStr, out var userId))
            return Result<AuthResponseDto>.Failure("Invalid token.", HttpStatusCode.Unauthorized);
        
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) 
            return Result<AuthResponseDto>.Failure("Invalid token.", HttpStatusCode.Unauthorized);

        return Result<AuthResponseDto>.Success(await CreateAuthResponseDtoFromUser(user));
    }
    
    private async Task<Result<bool>> ValidateRegistrationAsync(UserRegisterDto user)
    {
        if (user.Email is not null && await _dapper.ExistsByFieldAsync<User>("email", user.Email))
            return Result<bool>.Failure("Email already exists.", HttpStatusCode.BadRequest);
        if (await _dapper.ExistsByFieldAsync<User>("username", user.Username))
            return Result<bool>.Failure("Username already exists.", HttpStatusCode.BadRequest);
        return Result<bool>.Success(true);
    }
    
    private async Task CreateAuthAsync(int userId, string password)
    {
        var (passwordSalt, passwordHash) = _passwordHasher.HashPassword(password);
        await _dapper.InsertAsync(new Auth
        {
            UserId = userId,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        });
    }

    private async Task<AuthResponseDto> CreateAuthResponseDtoFromUser(User user)
    {
        var roles = await GetUserRolesAsync(user.UserId);
        
        return new AuthResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Roles = roles,
            Token = _tokenGen.GenerateToken(user.UserId, roles)
        };
    }

    private async Task<List<string>> GetUserRolesAsync(int userId)
    {
        var sql = """
                  SELECT r.roleName
                  FROM dsf.userRole ur
                  JOIN dsf.role r ON ur.roleId = r.roleId
                  WHERE ur.userId = @userId
                  """;
        return (await _dapper.QueryAsync<string>(sql, new { userId })).ToList();
    }
}