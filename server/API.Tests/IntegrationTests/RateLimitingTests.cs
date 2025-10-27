using System.Net;
  using API.Tests.Helpers;
  using FluentAssertions;
  using Xunit;

  namespace API.Tests.IntegrationTests;

  public class RateLimitingTests : IClassFixture<CustomWebApplicationFactory>
  {
      private readonly HttpClient _client;

      public RateLimitingTests(CustomWebApplicationFactory factory)
      {
          _client = factory.CreateClient();
      }

      [Fact]
      public async Task RateLimiter_WhenExceeding100Requests_Returns429()
      {
          // Arrange
          const string url = "/common/product-types";
          var rateLimitExceeded = false;

          // Act 
          for (var i = 0; i < 105; i++)
          {
              var response = await _client.GetAsync(url);

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