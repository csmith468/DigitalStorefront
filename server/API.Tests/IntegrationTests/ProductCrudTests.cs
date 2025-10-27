using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using API.Models.Dtos;
using API.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace API.Tests.IntegrationTests;

public class ProductCrudTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductCrudTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, AuthResponseDto auth)> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();

        var registerDto = new UserRegisterDto
        {
            Username = $"testUser_{Guid.NewGuid():N}",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var response = await client.PostAsJsonAsync("/auth/register", registerDto);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        return (client, authResponse);
    }

    [Fact]
    public async Task CreateProduct_UpdateProduct_DeleteProduct_FullFlow()
    {
        // Arrange
        var (client, auth) = await CreateAuthenticatedClientAsync();

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
        var createResponse = await client.PostAsJsonAsync("/products", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDetailDto>();
        created.Should().NotBeNull();
        created.Name.Should().Be(createDto.Name);

        // Act & Assert - Get by ID
        var getResponse = await client.GetAsync($"/products/{created.ProductId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - Update
        createDto.Name = "Updated Product Name";
        var updateResponse = await client.PutAsJsonAsync($"/products/{created.ProductId}", createDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProductDetailDto>();
        updated!.Name.Should().Be("Updated Product Name");

        // Act & Assert - Delete
        var deleteResponse = await client.DeleteAsync($"/products/{created.ProductId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify Deleted
        var getDeletedResponse = await client.GetAsync($"/products/{created.ProductId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();  // No authentication
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
        var response = await client.PostAsJsonAsync("/products", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}