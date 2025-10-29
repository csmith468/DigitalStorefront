using API.Database;
using API.Models;
using API.Models.DboTables;
using API.Models.Dtos;
using API.Services;
using API.Services.Images;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<IQueryExecutor> _mockQueryExecutor;
    private readonly Mock<ICommandExecutor> _mockCommandExecutor;
    private readonly Mock<ITransactionManager> _mockTransactionManager;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly Mock<IProductImageService> _mockImageService;
    private readonly Mock<IImageStorageService> _mockStorageService;
    private readonly Mock<IProductAuthorizationService> _mockAuthService;
    private readonly ProductService _service;
    private readonly Mock<ITagService> _mockTagService;

    public ProductServiceTests()
    {
        _mockQueryExecutor = new Mock<IQueryExecutor>();
        _mockCommandExecutor = new Mock<ICommandExecutor>();
        _mockTransactionManager = new Mock<ITransactionManager>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _mockMapper = new Mock<IMapper>();
        _mockUserContext = new Mock<IUserContext>();
        _mockImageService = new Mock<IProductImageService>();
        _mockStorageService = new Mock<IImageStorageService>();
        _mockAuthService = new Mock<IProductAuthorizationService>();
        _mockTagService = new Mock<ITagService>();
        
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "DemoMode", "false" },
                { "AppSettings:TokenKey", "test-key-for-unit-tests-minimum-32-chars" },
                { "AppSettings:PasswordKey", "test-password-key" }
            }!)
            .Build();

        _service = new ProductService(
            _mockQueryExecutor.Object,
            _mockCommandExecutor.Object,
            _mockTransactionManager.Object,
            _mockLogger.Object,
            _mockMapper.Object,
            config,
            _mockUserContext.Object,
            _mockImageService.Object,
            _mockStorageService.Object,
            _mockAuthService.Object,
            _mockTagService.Object
        );
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ReturnsSuccess()
    {
        // Arrange
        const int productId = 1;
        var product = new Product { ProductId = productId, Name = "Test Product" };
        var productDto = new ProductDetailDto { ProductId = productId, Name = "Test Product" };

        _mockQueryExecutor.Setup(d => d.GetByIdAsync<Product>(productId)).ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<Product, ProductDetailDto>(product)).Returns(productDto);
        _mockImageService.Setup(i => i.GetAllProductImagesAsync(productId))
            .ReturnsAsync(Result<List<ProductImageDto>>.Success([]));
        _mockQueryExecutor.Setup(d => d.GetByFieldAsync<ProductSubcategory>("productId", productId))
            .ReturnsAsync(new List<ProductSubcategory>());
        _mockQueryExecutor.Setup(d => d.GetWhereInAsync<Subcategory>("subcategoryId", It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Subcategory>());

        // Act
        var result = await _service.GetProductByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductNotFound_ReturnsFailure()
    {
        // Arrange
        const int productId = 999;
        _mockQueryExecutor.Setup(d => d.GetByIdAsync<Product>(productId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetProductByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateName_ReturnsFailure()
    {
        // Arrange
        var dto = new ProductFormDto { Name = "Existing Product", Slug = "existing-product" };
        const int userId = 1;

        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<Product>("name", dto.Name)).ReturnsAsync(true);

        // Act
        var result = await _service.CreateProductAsync(dto, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateSlug_ReturnsFailure()
    {
        // Arrange
        var dto = new ProductFormDto { Name = "New Product", Slug = "existing-slug" };
        const int userId = 1;

        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<Product>("name", dto.Name)).ReturnsAsync(false);
        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<Product>("slug", dto.Slug)).ReturnsAsync(true);

        // Act
        var result = await _service.CreateProductAsync(dto, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateProductAsync_WhenPremiumPriceExceedsRegularPrice_ReturnsFailure()
    {
        // Arrange
        var dto = new ProductFormDto
        {
            Name = "Test Product",
            Slug = "test-product",
            Price = 100,
            PremiumPrice = 150  // Invalid: premium price > regular price
        };
        const int userId = 1;

        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<Product>("name", dto.Name)).ReturnsAsync(false);
        _mockQueryExecutor.Setup(d => d.ExistsByFieldAsync<Product>("slug", dto.Slug)).ReturnsAsync(false);

        // Act
        var result = await _service.CreateProductAsync(dto, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Premium price cannot exceed regular price");
    }
    
    [Fact]
    public async Task 
        CreateProductAsync_InDemoMode_WhenUserExceedsLimit_ReturnsUnauthorized()
    {
        // Arrange - DemoMode = true
        var demoConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "DemoMode", "true" }, 
                { "AppSettings:TokenKey", "test-key" }
            }!)
            .Build();
        
        var service = new ProductService(
            _mockQueryExecutor.Object,
            _mockCommandExecutor.Object,
            _mockTransactionManager.Object,
            _mockLogger.Object,
            _mockMapper.Object,
            demoConfig,
            _mockUserContext.Object,
            _mockImageService.Object,
            _mockStorageService.Object,
            _mockAuthService.Object,
            _mockTagService.Object
        );

        var dto = new ProductFormDto
        {
            Name = "New Product",
            Slug = "new-product",
            SubcategoryIds = [1]
        };
        const int userId = 1;

        _mockQueryExecutor.Setup(d => d.GetCountByFieldAsync<Product>("createdBy", userId))
            .ReturnsAsync(4);  // Demo mode allows up to 3 products created

        // Act
        var result = await service.CreateProductAsync(dto, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}