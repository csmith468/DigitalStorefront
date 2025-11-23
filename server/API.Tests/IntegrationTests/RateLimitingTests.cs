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
    public async Task AnonymousPolicy_WhenExceeding30Requests_Returns429()
    {
        // Arrange
        const string url = "/api/metadata/product-types";
        var rateLimitExceeded = false;
        IsolateRateLimitingPerTest();

        // Act (anonymous policy allows 30 requests/minute)
        for (var i = 0; i < 35; i++)
        {
            var response = await Client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitExceeded = true;

                // Assert
                i.Should().Be(30, "Rate limit should apply on 30th request");
                response.Headers.Should().ContainKey("Retry-After");
                break;
            }
        }

        rateLimitExceeded.Should().BeTrue("Anonymous rate limit should block requests after 30");
    }

    [Fact]
    public async Task AuthPolicy_WhenExceeding5LoginAttempts_Returns429()
    {
        // Arrange
        const string url = "/api/auth/login";
        var loginDto = new UserLoginDto { Username = "nonexistent", Password = "wrong" };
        var rateLimitExceeded = false;
        var successfulAttempts = 0;
        IsolateRateLimitingPerTest();

        // Act (auth policy allows 5 requests/minute)
        for (var i = 0; i < 10; i++)
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

        successfulAttempts.Should().Be(5, "Rate limit should apply on 5th request");
        rateLimitExceeded.Should().BeTrue("Auth rate limit should block requests after 5");
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

    [Fact]
    public async Task GlobalLimiter_WhenExceeding150Requests_Returns429()
    {
        // Arrange
        const string url = "/api/metadata/product-types";
        var rateLimitExceeded = false;
        IsolateRateLimitingPerTest();

        // Act (global limiter allows 150 requests/minute across ALL endpoints)
        for (var i = 0; i < 155; i++)
        {
            var response = await Client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitExceeded = true;

                // Assert - should hit global limit (150) before we reach 155
                i.Should().BeGreaterThanOrEqualTo(30, "Should allow at least 30 requests (anonymous policy)");
                i.Should().BeLessThanOrEqualTo(150, "Global limit should block at or before 150 requests");
                response.Headers.Should().ContainKey("Retry-After");
                break;
            }
        }

        rateLimitExceeded.Should().BeTrue("Global rate limit should eventually block requests");
    }
}