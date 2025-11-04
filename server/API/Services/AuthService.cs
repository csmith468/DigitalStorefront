using System.Net;
using API.Database;
using API.Extensions;
using API.Models;
using API.Models.Constants;
using Api.Models.DsfTables;
using API.Models.DsfTables;
using API.Models.Dtos;
using API.Utils;

namespace API.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterUser(UserRegisterDto userDto);
    Task<Result<AuthResponseDto>> LoginUser(UserLoginDto userDto);
    Task<Result<AuthResponseDto>> RefreshToken(string userIdStr);
}

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ITransactionManager _transactionManager;
    private readonly TokenGenerator _tokenGen;
    private readonly PasswordHasher _passwordHasher;
    private readonly IUserService _userService;
    
    public AuthService(ILogger<AuthService> logger,
        IQueryExecutor queryExecutor,
        ICommandExecutor commandExecutor,
        ITransactionManager transactionManager,
        TokenGenerator tokenGen,
        PasswordHasher passwordHasher,
        IUserService userService)
    {
        _logger = logger;
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _transactionManager = transactionManager;
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
        await _transactionManager.WithTransactionAsync(async () =>
        {
            userId = await _commandExecutor.InsertAsync(new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
            });
            await CreateAuthAsync(userId, userDto.Password);

            var roles = (await _queryExecutor.QueryAsync<Role>(
                "SELECT * FROM dsf.role WHERE roleName IN (@ProductWriter, @ImageManager)",
                new { RoleNames.ProductWriter, RoleNames.ImageManager }
            )).ToList();

            if (roles.Count != 0)
            {
                var userRoles = roles.Select(r => new UserRole { UserId = userId, RoleId = r.RoleId }).ToList();
                await _commandExecutor.BulkInsertAsync(userRoles);
            }
            
        });
        if (userId == 0)
            return Result<AuthResponseDto>.Failure(ErrorMessages.Auth.RegistrationFailed);

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
        var user = await _userService.GetUserByUsernameAsync(userDto.Username);
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", userDto.Username);
            return Result<AuthResponseDto>.Failure(ErrorMessages.Auth.InvalidCredentials);
        }
        var userAuth = (await _queryExecutor.GetByFieldAsync<Auth>("userId", user.UserId)).FirstOrDefault();
        if (userAuth == null || !_passwordHasher.VerifyPassword(userDto.Password, userAuth.PasswordSalt, userAuth.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", userDto.Username);
            return Result<AuthResponseDto>.Failure(ErrorMessages.Auth.InvalidCredentials);
        }
        
        _logger.LogInformation("User Logged In: UserId: {UserId} Username: {Username}", user.UserId, user.Username);
        return Result<AuthResponseDto>.Success(await CreateAuthResponseDtoFromUser(user));
    }

    public async Task<Result<AuthResponseDto>> RefreshToken(string userIdStr)
    {
        if (!int.TryParse(userIdStr, out var userId))
            return Result<AuthResponseDto>.Failure(ErrorMessages.Auth.InvalidCredentials);
        
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) 
            return Result<AuthResponseDto>.Failure(ErrorMessages.Auth.InvalidCredentials);

        return Result<AuthResponseDto>.Success(await CreateAuthResponseDtoFromUser(user));
    }
    
    private async Task<Result<bool>> ValidateRegistrationAsync(UserRegisterDto user)
    {
        if (user.Email is not null && await _queryExecutor.ExistsByFieldAsync<User>("email", user.Email))
            return Result<bool>.Failure(ErrorMessages.Auth.EmailExists);
        if (await _queryExecutor.ExistsByFieldAsync<User>("username", user.Username))
            return Result<bool>.Failure(ErrorMessages.Auth.UsernameExists);
        return Result<bool>.Success(true);
    }
    
    private async Task CreateAuthAsync(int userId, string password)
    {
        var (passwordSalt, passwordHash) = _passwordHasher.HashPassword(password);
        await _commandExecutor.InsertAsync(new Auth
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
        return (await _queryExecutor.QueryAsync<string>(sql, new { userId })).ToList();
    }
}