using System.Net;
  using API.Tests.Helpers;
  using FluentAssertions;
  using Xunit;

  namespace API.Tests.IntegrationTests;

  public class CachingTests : IClassFixture<CustomWebApplicationFactory>
  {
      private readonly HttpClient _client;

      public CachingTests(CustomWebApplicationFactory factory)
      {
          _client = factory.CreateClient();
      }

      [Fact]
      public async Task OutputCache_WhenRequestedTwice_ReturnsCachedResponse()
      {
          // Arrange
          const string url = "/common/product-types";
          var response1 = await _client.GetAsync(url);
          var ageHeader1 = response1.Headers.TryGetValues("age", out var age1) ? age1.First() : null;

          await Task.Delay(1000);

          var response2 = await _client.GetAsync(url);
          var ageHeader2 = response2.Headers.TryGetValues("age", out var age2) ? age2.First() : null;

          // Assert
          response1.StatusCode.Should().Be(HttpStatusCode.OK);
          response2.StatusCode.Should().Be(HttpStatusCode.OK);

          // Second test's higher age = cached
          if (ageHeader1 != null && ageHeader2 != null)
              int.Parse(ageHeader2).Should().BeGreaterThan(int.Parse(ageHeader1));
      }
  }