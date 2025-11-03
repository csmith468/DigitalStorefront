using System.Net;
using API.Configuration;
using API.Database;
using API.Models.DsfTables;
using API.Models.Dtos;
using API.Services;
using API.Utils;
using Api.Models.DsfTables;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace API.Tests.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IQueryExecutor> _mockQueryExecutor;
    private readonly Mock<ICommandExecutor> _mockCommandExecutor;
    private readonly Mock<ITransactionManager> _mockTransactionManager;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly TokenGenerator _tokenGen;
    private readonly PasswordHasher _passwordHasher;
    private readonly Mock<IUserService> _mockUserService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockQueryExecutor = new Mock<IQueryExecutor>();
        _mockCommandExecutor = new Mock<ICommandExecutor>();
        _mockTransactionManager = new Mock<ITransactionManager>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _mockUserService = new Mock<IUserService>();

        var securityOptions = Microsoft.Extensions.Options.Options.Create(new SecurityOptions
        {
            TokenKey = "test-token-key-minimum-32-characters-long-for-jwt-tokens",
            PasswordKey = "test-password-key-minimum-32-characters-long-for-jwt-tokens"
        });

        _passwordHasher = new PasswordHasher(securityOptions);
        _tokenGen = new TokenGenerator(securityOptions);

        _authService = new AuthService(
            _mockLogger.Object,
            _mockQueryExecutor.Object,
            _mockCommandExecutor.Object,
            _mockTransactionManager.Object,
            _tokenGen,
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

        var userId = 123;
        var roles = new List<string> { "ProductWriter", "ImageManager" };

        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<User>("email", registerDto.Email)).ReturnsAsync(false);
        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<User>("username", registerDto.Username)).ReturnsAsync(false);
        _mockTransactionManager.Setup(d => d.WithTransactionAsync(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(async action => await action())
            .Returns(Task.CompletedTask);
        _mockCommandExecutor.Setup(d => d.InsertAsync(It.IsAny<User>())).ReturnsAsync(userId);
        _mockCommandExecutor.Setup(d => d.InsertAsync(It.IsAny<Auth>())).ReturnsAsync(1);
        _mockQueryExecutor.Setup(d => d.QueryAsync<Role>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(new List<Role>
            {
                  new() { RoleId = 1, RoleName = "ProductWriter" },
                  new() { RoleId = 2, RoleName = "ImageManager" }
            });
        _mockCommandExecutor.Setup(d => d.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(1);
        _mockQueryExecutor.Setup(d => d.QueryAsync<string>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(roles);

        // Act
        var result = await _authService.RegisterUser(registerDto);

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

        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<User>("email", registerDto.Email)).ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterUser(registerDto);

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

        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<User>("email", registerDto.Email)).ReturnsAsync(false);
        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<User>("username", registerDto.Username)).ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterUser(registerDto);

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

        _mockUserService.Setup(u => u.GetUserByUsernameAsync(loginDto.Username)).ReturnsAsync(user);
        _mockQueryExecutor.Setup(d => d.GetByFieldAsync<Auth>("userId", user.UserId)).ReturnsAsync(new List<Auth> { auth });
        _mockQueryExecutor.Setup(d => d.QueryAsync<string>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(roles);

        // Act
        var result = await _authService.LoginUser(loginDto);

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

        _mockUserService.Setup(u => u.GetUserByUsernameAsync(loginDto.Username)).ReturnsAsync(user);
        _mockQueryExecutor.Setup(d => d.GetByFieldAsync<Auth>("userId", user.UserId)).ReturnsAsync(new List<Auth> { auth });

        // Act
        var result = await _authService.LoginUser(loginDto);

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

        _mockUserService.Setup(u => u.GetUserByUsernameAsync(loginDto.Username)).ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginUser(loginDto);

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

        _mockUserService.Setup(u => u.GetUserByIdAsync(123)).ReturnsAsync(user);
        _mockQueryExecutor.Setup(d => d.QueryAsync<string>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(roles);

        // Act
        var result = await _authService.RefreshToken(userIdStr);

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
        var result = await _authService.RefreshToken(userIdStr);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid username or password");
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

internal class TestServiceProvider : IServiceProvider
{
    private readonly IConfiguration _configuration;

    public TestServiceProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public object? GetService(Type serviceType)
    {
        return serviceType == typeof(IConfiguration) ? _configuration : null;
    }
}