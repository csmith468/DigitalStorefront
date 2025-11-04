using System.Net;
using API.Configuration;
using API.Database;
using API.Models.Constants;
using API.Models.DsfTables;
using API.Models.Dtos;
using API.Services;
using API.Utils;
using Api.Models.DsfTables;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IQueryExecutor> _mockQueryExecutor;
    private readonly Mock<ICommandExecutor> _mockCommandExecutor;
    private readonly Mock<ITransactionManager> _mockTransactionManager;
    private readonly PasswordHasher _passwordHasher;
    private readonly Mock<IUserService> _mockUserService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockQueryExecutor = new Mock<IQueryExecutor>();
        _mockCommandExecutor = new Mock<ICommandExecutor>();
        _mockTransactionManager = new Mock<ITransactionManager>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        _mockUserService = new Mock<IUserService>();

        var securityOptions = Microsoft.Extensions.Options.Options.Create(new SecurityOptions
        {
            TokenKey = "test-token-key-minimum-32-characters-long-for-jwt-tokens",
            PasswordKey = "test-password-key-minimum-32-characters-long-for-jwt-tokens"
        });

        _passwordHasher = new PasswordHasher(securityOptions);
        var tokenGen = new TokenGenerator(securityOptions);

        _authService = new AuthService(
            mockLogger.Object,
            _mockQueryExecutor.Object,
            _mockCommandExecutor.Object,
            _mockTransactionManager.Object,
            tokenGen,
            _passwordHasher,
            _mockUserService.Object
        );
    }

    [Fact]
    public async Task RegisterUser_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var registerDto = new UserRegisterDto
        {
            Username = "testUser",
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        const int userId = 123;
        var roles = new List<string> { RoleNames.ProductWriter, RoleNames.ImageManager };

        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<User>("email", registerDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<User>("username", registerDto.Username, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockTransactionManager.Setup(d => d.WithTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Callback<Func<Task>, CancellationToken>((action, _) => action())
            .Returns(Task.CompletedTask);
        _mockCommandExecutor.Setup(d => d.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _mockCommandExecutor.Setup(d => d.InsertAsync(It.IsAny<Auth>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockQueryExecutor.Setup(d => d.QueryAsync<Role>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role>
            {
                  new() { RoleId = 1, RoleName = RoleNames.ProductWriter },
                  new() { RoleId = 2, RoleName = RoleNames.ImageManager }
            });
        _mockCommandExecutor.Setup(d => d.BulkInsertAsync(It.IsAny<IEnumerable<UserRole>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockQueryExecutor.Setup(d => d.QueryAsync<string>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(roles);

        // Act
        var result = await _authService.RegisterUserAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Data.Should().NotBeNull();
        result.Data.UserId.Should().Be(userId);
        result.Data.Username.Should().Be(registerDto.Username);
        result.Data.Token.Should().NotBeNullOrEmpty();
        result.Data.Roles.Should().Contain("ProductWriter");
        result.Data.Roles.Should().Contain("ImageManager");
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var registerDto = new UserRegisterDto
        {
            Username = "testUser",
            Email = "existing@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };

        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<User>("email", registerDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterUserAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Email already exists");
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateUsername_ReturnsFailure()
    {
        // Arrange
        var registerDto = new UserRegisterDto
        {
            Username = "existingUser",
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };

        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<User>("email", registerDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<User>("username", registerDto.Username, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterUserAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Username already exists");
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LoginUser_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var loginDto = new UserLoginDto { Username = "testUser", Password = "Test123!" };
        var user = new User { UserId = 123, Username = "testUser", Email = "test@example.com" };

        var (salt, hash) = _passwordHasher.HashPassword(loginDto.Password);
        var auth = new Auth { UserId = 123, PasswordSalt = salt, PasswordHash = hash };

        var roles = new List<string> { "ProductWriter" };

        _mockUserService.Setup(u => u.GetUserByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockQueryExecutor.Setup(d => d.GetByFieldAsync<Auth>("userId", user.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Auth> { auth });
        _mockQueryExecutor.Setup(d => d.QueryAsync<string>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(roles);

        // Act
        var result = await _authService.LoginUserAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();
        result.Data.UserId.Should().Be(user.UserId);
        result.Data.Username.Should().Be(user.Username);
        result.Data.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginUser_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto { Username = "testUser", Password = "WrongPassword123!" };
        var user = new User { UserId = 123, Username = "testUser", Email = "test@example.com" };

        var (salt, hash) = _passwordHasher.HashPassword("CorrectPassword123!");
        var auth = new Auth { UserId = 123, PasswordSalt = salt, PasswordHash = hash };

        _mockUserService.Setup(u => u.GetUserByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockQueryExecutor.Setup(d => d.GetByFieldAsync<Auth>("userId", user.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Auth> { auth });

        // Act
        var result = await _authService.LoginUserAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid username or password");
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginUser_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto { Username = "nonExistent", Password = "Test123!" };

        _mockUserService.Setup(u => u.GetUserByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginUserAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid username or password");
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithValidUserId_ReturnsNewToken()
    {
        // Arrange
        const string userIdStr = "123";
        var user = new User { UserId = 123, Username = "testUser", Email = "test@example.com" };

        var roles = new List<string> { "ProductWriter" };

        _mockUserService.Setup(u => u.GetUserByIdAsync(123, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockQueryExecutor.Setup(d => d.QueryAsync<string>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(roles);

        // Act
        var result = await _authService.RefreshTokenAsync(userIdStr);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_WithInvalidUserId_ReturnsUnauthorized()
    {
        // Arrange
        const string userIdStr = "invalid";

        // Act
        var result = await _authService.RefreshTokenAsync(userIdStr);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid username or password");
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

internal class TestServiceProvider(IConfiguration configuration) : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        return serviceType == typeof(IConfiguration) ? configuration : null;
    }
}