using System.Net;
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
        client.DefaultRequestHeaders.Add("Test-Partition-Key", Guid.NewGuid().ToString());

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
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to register test user. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
            throw new InvalidOperationException("Registration succeeded but no token was returned");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        return (client, authResponse);
    }

    public static Task<HttpResponseMessage> PostWithIdempotencyAsync<T>(this HttpClient client, string url, T content)
        => SendWithIdempotencyAsync(client, HttpMethod.Post, url, JsonContent.Create(content));

    public static Task<HttpResponseMessage> PutWithIdempotencyAsync<T>(this HttpClient client, string url, T content)
        => SendWithIdempotencyAsync(client, HttpMethod.Put, url, JsonContent.Create(content));

    public static Task<HttpResponseMessage> DeleteWithIdempotencyAsync(this HttpClient client, string url)
        => SendWithIdempotencyAsync(client, HttpMethod.Delete, url);

    private static async Task<HttpResponseMessage> SendWithIdempotencyAsync(
        HttpClient client, HttpMethod method, string url, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }
}