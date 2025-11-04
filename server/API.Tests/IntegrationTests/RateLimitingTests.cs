using System.Net;
using System.Net.Http.Json;
using API.Models.Dtos;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests.IntegrationTests;

[Collection("Database")]
[Trait("Category", "Integration")]
public class RateLimitingTests(DatabaseFixture fixture) : IntegrationTestBase(fixture) 
{
    [Fact]
    public async Task AnonymousPolicy_WhenExceeding60Requests_Returns429()
    {
        // Arrange
        const string url = "/api/metadata/product-types";
        var rateLimitExceeded = false;
        IsolateRateLimitingPerTest();
        
        // Act (anonymous policy allows 60 requests/minute)
        for (var i = 0; i < 65; i++)
        {
            var response = await Client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitExceeded = true;
                
                // Assert
                i.Should().Be(60, "Rate limit should apply on 60th request");
                response.Headers.Should().ContainKey("Retry-After");
                break;
            }
        }
        
        rateLimitExceeded.Should().BeTrue("Anonymous rate limit should block requests after 60");
    }

    [Fact]
    public async Task AuthPolicy_WhenExceeding10LoginAttempts_Returns429()
    {
        // Arrange
        const string url = "/api/auth/login";
        var loginDto = new UserLoginDto { Username = "nonexistent", Password = "wrong" };
        var rateLimitExceeded = false;
        var successfulAttempts = 0;
        IsolateRateLimitingPerTest();
        
        // Act (auth policy allows 10 requests/minute)
        for (var i = 0; i < 15; i++)
        {
            var response = await Client.PostAsJsonAsync(url, loginDto);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitExceeded = true;
                response.Headers.Should().ContainKey("Retry-After");
                break;
            }
            
            successfulAttempts++;
        }
        
        successfulAttempts.Should().Be(10, "Rate limit should apply on 10th request");
        rateLimitExceeded.Should().BeTrue("Anonymous rate limit should block requests after 10");
    }

    [Fact]
    public async Task AuthenticatedPolicy_AllowsBurstThenThrottles()
    {
        // Arrange
        IsolateRateLimitingPerTest();
        var (client, _) = await TestAuthHelpers.CreateAuthenticatedClientAsync(Factory);
        
        // Act (token bucket allows burst of 100 requests)
        var successfulRequests = 0;
        var rateLimitExceeded = false;

        for (var i = 0; i < 110; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/refresh-token", new { });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitExceeded = true;
                break;
            }
            
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized)
                successfulRequests++;
        }
        
        // Assert
        successfulRequests.Should().BeGreaterThanOrEqualTo(100,
            $"Token bucket should allow burst of 100 request, got {successfulRequests}");
        rateLimitExceeded.Should().BeTrue("Should eventually hit rate limit after token bucket is exhausted");
    }

    private void IsolateRateLimitingPerTest()
    {
        Client.DefaultRequestHeaders.Add("Test-Partition-Key", Guid.NewGuid().ToString());
    }
}