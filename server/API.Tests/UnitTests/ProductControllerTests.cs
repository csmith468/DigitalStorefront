using API.Controllers;
using API.Database;
using API.Models.DboTables;
using API.Setup;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace API.Tests.UnitTests;

public class ProductControllerTests
{
    private readonly Mock<IDataContextDapper> _mockDapper;
    private readonly ProductController _controller;

    private readonly List<Product> _testProducts =
    [
        new() { ProductId = 1, Name = "Product 1", Slug = "product-1" },
        new() { ProductId = 2, Name = "Product 2", Slug = "product-2" }
    ];

    public ProductControllerTests()
    {
        _mockDapper = new Mock<IDataContextDapper>();
        var mockContainer = new MockSharedContainer(_mockDapper.Object);
        _controller = new ProductController(mockContainer);
    }
    
    private class MockSharedContainer(IDataContextDapper dapper) : ISharedContainer
    {
        public IDataContextDapper Dapper => dapper;
        public IConfiguration Config => null!; // Not used in ProductController
        public T? DepInj<T>() where T : class => null;
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnAllProducts()
    {
        // Arrange
        _mockDapper.Setup(d => d.GetAllAsync<Product>()).ReturnsAsync(_testProducts);
        
        // Act
        var result = await _controller.GetAllProducts();
        
        // Assert
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Should().BeEquivalentTo(_testProducts);
        result.Value.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var product = _testProducts[0];
        _mockDapper.Setup(d => d.GetByIdAsync<Product>(product.ProductId)).ReturnsAsync(product);
        
        // Act
        var result = await _controller.GetProduct(product.ProductId);
        
        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        const int invalidId = -1;
        _mockDapper.Setup(d => d.GetByIdAsync<Product>(invalidId)).ReturnsAsync((Product?)null);
        
        // Act
        var result = await _controller.GetProduct(invalidId);
        
        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be($"Product ID {invalidId} not found.");
    }

    [Fact]
    public async Task GetProductsBySubcategory_WithValidSlug_ShouldReturnProducts()
    {
        // Arrange
        const string subcategorySlug = "test-subcategory";
        _mockDapper.Setup(d => d.ExistsByFieldAsync<Subcategory>("slug", subcategorySlug)).ReturnsAsync(true);
        _mockDapper.Setup(d => d.QueryAsync<Product>(
            It.IsAny<string>(),
            It.IsAny<object>()))
            .ReturnsAsync(_testProducts);
        
        // Act
        var result = await _controller.GetProductsBySubcategory(subcategorySlug);
        
        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(_testProducts);
    }

    [Fact]
    public async Task GetProductsBySubcategory_WithInvalidSlug_ShouldReturnNotFound()
    {
        // Arrange
        const string invalidSlug = "invalid-slug";
        _mockDapper.Setup(d => d.ExistsByFieldAsync<Subcategory>("slug", invalidSlug)).ReturnsAsync(false);
        
        // Act
        var result = await _controller.GetProductsBySubcategory(invalidSlug);
        
        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be($"Subcategory slug {invalidSlug} not found.");
    }

    [Fact]
    public async Task GetProductsBySubcategory_WithNoProducts_ShouldReturnNotFound()
    {
        // Arrange
        const string subcategorySlug = "empty-subcategory";
        _mockDapper.Setup(d => d.ExistsByFieldAsync<Subcategory>("slug", subcategorySlug)).ReturnsAsync(true);
        _mockDapper.Setup(d => d.QueryAsync<Product>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(new List<Product>());
        
        // Act
        var result = await _controller.GetProductsBySubcategory(subcategorySlug);
        
        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be($"No products found for {subcategorySlug}.");
    }
}