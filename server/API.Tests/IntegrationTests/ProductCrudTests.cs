using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using API.Models.Dtos;
using API.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace API.Tests.IntegrationTests;

[Collection("Database")]
[Trait("Category", "Integration")]
public class ProductCrudTests(DatabaseFixture fixture) : IntegrationTestBase(fixture) 
{
    [Fact]
    public async Task CreateProduct_UpdateProduct_DeleteProduct_FullFlow()
    {
        // Arrange
        var (client, auth) = await TestAuthHelpers.CreateAuthenticatedClientAsync(Factory);

        var createDto = new ProductFormDto
        {
            Name = $"Test Product {Guid.NewGuid():N}",
            Slug = $"test-product-{Guid.NewGuid():N}",
            Description = "Test description",
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        // Act & Assert - Create
        var createResponse = await client.PostAsJsonAsync("/api/products", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDetailDto>();
        created.Should().NotBeNull();
        created.Name.Should().Be(createDto.Name);

        // Act & Assert - Get by ID
        var getResponse = await client.GetAsync($"/api/products/{created.ProductId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - Update
        createDto.Name = "Updated Product Name";
        var updateResponse = await client.PutAsJsonAsync($"/api/products/{created.ProductId}", createDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProductDetailDto>();
        updated!.Name.Should().Be("Updated Product Name");

        // Act & Assert - Delete
        var deleteResponse = await client.DeleteAsync($"/api/products/{created.ProductId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify Deleted
        var getDeletedResponse = await client.GetAsync($"/api/products/{created.ProductId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();  // No authentication
        var createDto = new ProductFormDto
        {
            Name = "Test Product",
            Slug = "test-product",
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/products", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidSubcategory_RollsBackTransaction()
    {
        // Arrange
        var (client, auth) = await TestAuthHelpers.CreateAuthenticatedClientAsync(Factory);
        var uniqueName = $"Test Product {Guid.NewGuid():N}";
        var uniqueSlug = $"test-product-{Guid.NewGuid():N}";

        var createDto = new ProductFormDto
        {
            Name = uniqueName,
            Slug = uniqueSlug,
            Description = "Test description",
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [999999]  // Invalid subcategory ID - should trigger rollback
        };

        // Act
        var createResponse = await client.PostAsJsonAsync("/api/products", createDto);

        // Assert - Request should fail
        createResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await createResponse.Content.ReadAsStringAsync();
        errorContent.Should().ContainEquivalentOf("subcategor");

        var getBySlugResponse = await client.GetAsync($"/api/products/slug/{uniqueSlug}");
        getBySlugResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "Product should not exist in database due to transaction rollback");
    }
}