using System.Net.Http.Headers;
using System.Net.Http.Json;
using API.Models.Dtos;

namespace API.Tests.Helpers;

public static class TestAuthHelpers
{
    public static async Task<(HttpClient client, AuthResponseDto auth)> CreateAuthenticatedClientAsync(
        CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();

        var registerDto = new UserRegisterDto
        {
            Username = $"testUser_{Guid.NewGuid():N}",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", registerDto);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        return (client, authResponse);
    }
}