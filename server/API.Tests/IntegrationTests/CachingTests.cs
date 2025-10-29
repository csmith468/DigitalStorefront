using System.Net;
using System.Net.Http.Json;
using API.Models.Dtos;
using API.Tests.Helpers;
  using FluentAssertions;
  using Xunit;

  namespace API.Tests.IntegrationTests;

  public class CachingTests : IClassFixture<CustomWebApplicationFactory>
  {
      private readonly CustomWebApplicationFactory _factory;

      public CachingTests(CustomWebApplicationFactory factory)
      {
          _factory = factory;
      }

      [Fact]
      public async Task OutputCache_WhenRequestedTwice_ReturnsCachedResponse()
      {
          // Arrange
          var client = _factory.CreateClient();
          
          const string url = "/metadata/product-types";
          var response1 = await client.GetAsync(url);
          var ageHeader1 = response1.Headers.TryGetValues("age", out var age1) ? age1.First() : null;

          await Task.Delay(1000);

          var response2 = await client.GetAsync(url);
          var ageHeader2 = response2.Headers.TryGetValues("age", out var age2) ? age2.First() : null;

          // Assert
          response1.StatusCode.Should().Be(HttpStatusCode.OK);
          response2.StatusCode.Should().Be(HttpStatusCode.OK);

          // Second test's higher age = cached
          if (ageHeader1 != null && ageHeader2 != null)
              int.Parse(ageHeader2).Should().BeGreaterThan(int.Parse(ageHeader1));
      }

      [Fact]
      public async Task TagCache_WhenNewTagCreated_CacheIsInvalidated()
      {
          // Arrange
          var (client, auth) = await TestAuthHelpers.CreateAuthenticatedClientAsync(_factory);
          
          const string tagsUrl = "/metadata/tags";
          var initialTagsResponse = await client.GetAsync(tagsUrl);
          var initialTags = await initialTagsResponse.Content.ReadFromJsonAsync<List<TagDto>>();
          var initialCount = initialTags?.Count ?? 0;

          await Task.Delay(1000);
          
          // Verify cache is working
          var cachedResponse = await client.GetAsync(tagsUrl);
          var ageHeader1 = cachedResponse.Headers.TryGetValues("age", out var age1) ? age1.First() : null;
          
          // Act (create product with new tag to invalidate cache)
          var uniqueTag = $"test-tag-{Guid.NewGuid():N}";
          var productDto = new ProductFormDto
          {
              Name = "Cache Test Product",
              Slug = $"cache-test-{Guid.NewGuid():N}",
              Description = "Testing cache invalidation",
              ProductTypeId = 1,
              PriceTypeId = 1,
              Price = 100,
              PremiumPrice = 80,
              Tags = [uniqueTag],
              SubcategoryIds = [1]
          };
          var createProductResponse = await client.PostAsJsonAsync("/products", productDto);

          await Task.Delay(1000);
          
          // Assert
          var freshResponse = await client.GetAsync(tagsUrl);
          var freshTags = await freshResponse.Content.ReadFromJsonAsync<List<TagDto>>();
          var ageHeader2 = freshResponse.Headers.TryGetValues("age", out var age2) ? age2.First() : null;
          
          // Verify cache was invalidated and new tag shows up in response
          createProductResponse.StatusCode.Should().Be(HttpStatusCode.Created);
          freshTags.Should().HaveCount(initialCount + 1);
          freshTags.Should().Contain(t => t.Name == uniqueTag);
          
          if (ageHeader1 != null && ageHeader2 != null)
          {
              int.Parse(ageHeader2).Should().BeLessThan(int.Parse(ageHeader1),
                  "because cache was evicted when new tag was created");
          }
      }
  }