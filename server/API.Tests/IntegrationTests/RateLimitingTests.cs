using System.Net;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests.IntegrationTests;

[Collection("Database")]
[Trait("Category", "Integration")]
public class RateLimitingTests(DatabaseFixture fixture) : IntegrationTestBase(fixture) 
{
    [Fact]
    public async Task RateLimiter_WhenExceeding100Requests_Returns429()
    {
        // Arrange
        const string url = "/common/product-types";
        var rateLimitExceeded = false;

        // Act 
        for (var i = 0; i < 105; i++)
        {
            var response = await Client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitExceeded = true;
                break;
            }
        }

        // Assert
        rateLimitExceeded.Should().BeTrue("Rate limiter should block requests after 100");
    }
}