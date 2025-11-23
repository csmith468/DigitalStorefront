using System.Net;
using System.Net.Http.Json;
using API.Models.Dtos;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests.IntegrationTests;

[Collection("Database")]
[Trait("Category", "Integration")]
public class AuthFlowTests(DatabaseFixture fixture) : IntegrationTestBase(fixture) 
{

    [Fact]
    public async Task RegisterUser_WithValidData_ReturnsCreatedWithToken()
    {
        // Arrange
        IsolateRateLimitingPerTest();
        var registerDto = new UserRegisterDto
        {
            Username = $"testUser_{Guid.NewGuid():N}",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",  // ✅ ADDED
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.Username.Should().Be(registerDto.Username);
        authResponse.Roles.Should().Contain("ProductWriter");
        authResponse.Roles.Should().Contain("ImageManager");
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        IsolateRateLimitingPerTest();
        var username = $"duplicate_{Guid.NewGuid():N}";
        const string password = "SecurePass123!";
        var registerDto1 = new UserRegisterDto
        {
            Username = username,
            Email = $"test1_{Guid.NewGuid():N}@example.com",
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User"
        };
        var registerDto2 = new UserRegisterDto
        {
            Username = username,
            Email = $"test2_{Guid.NewGuid():N}@example.com",
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        await Client.PostAsJsonAsync("/api/auth/register", registerDto1);
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("already exists");
    }

    [Fact]
    public async Task LoginUser_WithValidCredentials_ReturnsToken()
    {
        // Arrange - Register user first
        IsolateRateLimitingPerTest();
        var username = $"loginTest_{Guid.NewGuid():N}";
        const string password = "CorrectPassword123!";

        var registerDto = new UserRegisterDto
        {
            Username = username,
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User"
        };

        // Make sure registration completes successfully before attempting to log in
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerAuthResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        registerAuthResponse.Should().NotBeNull();

        var loginDto = new UserLoginDto
        {
            Username = username,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.Username.Should().Be(loginDto.Username);
    }

    [Fact]
    public async Task LoginUser_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange - Register user with one password, try to log in with another
        IsolateRateLimitingPerTest();
        var username = $"pwdtest_{Guid.NewGuid():N}";
        const string correctPassword = "CorrectPassword123!";
        const string wrongPassword = "WrongPassword123!";

        var registerDto = new UserRegisterDto
        {
            Username = username,
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = correctPassword,
            ConfirmPassword = correctPassword,
            FirstName = "Test",
            LastName = "User"
        };
        await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new UserLoginDto
        {
            Username = username,
            Password = wrongPassword 
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
