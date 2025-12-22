using API.Infrastructure.Viewers;
using FluentAssertions;

namespace API.Tests.UnitTests;

public class ViewerTrackingServiceTests
{
    private readonly ViewerTrackingService _service = new();
    private const string Connection1 = "connection-1";
    private const string Connection2 = "connection-2";
    private const string Product1 = "product-1";
    private const string Product2 = "product-2";

    [Fact]
    public void TrackViewer_FirstViewer_ReturnsCountOfOne()
    {
        // Arrange
        
        // Act
        var result = _service.TrackViewer(Connection1, Product1);
        
        // Assert
        result.ProductSlug.Should().Be(Product1);
        result.ViewerCount.Should().Be(1);
        result.PreviousProduct.Should().BeNull();
    }

    [Fact]
    public void TrackViewer_SecondViewer_ReturnsCountOfTwo()
    {
        // Arrange
        _service.TrackViewer(Connection1, Product1);
        
        // Act
        var result = _service.TrackViewer(Connection2, Product1);
        
        // Assert
        result.ViewerCount.Should().Be(2);
    }

    [Fact]
    public void TrackViewer_SwitchingProducts_DecrementsOldAndIncrementsNew()
    {
        // Arrange
        _service.TrackViewer(Connection1, Product1);
        
        // Act
        var result = _service.TrackViewer(Connection1, Product2);
        
        // Assert
        result.ProductSlug.Should().Be(Product2);
        result.ViewerCount.Should().Be(1);
        result.PreviousProduct.Should().NotBeNull();
        result.PreviousProduct.ProductSlug.Should().Be(Product1);
        result.PreviousProduct.ViewerCount.Should().Be(0);
    }

    [Fact]
    public void UntrackViewer_ExistingConnection_ReturnsDecrementedCount()
    {
        // Arrange
        _service.TrackViewer(Connection1, Product1);
        _service.TrackViewer(Connection2, Product1);
        
        // Act
        var result = _service.UntrackViewer(Connection1);

        // Assert
        result.Should().NotBeNull();
        result.ProductSlug.Should().Be(Product1);
        result.ViewerCount.Should().Be(1);
    }

    [Fact]
    public void UntrackViewer_LastViewer_ReturnsCountOfZero()
    {
        // Arrange
        _service.TrackViewer(Connection1, Product1);
        
        // Act
        var result = _service.UntrackViewer(Connection1);

        // Assert
        result.Should().NotBeNull();
        result.ViewerCount.Should().Be(0);
    }

    [Fact]
    public void UntrackViewer_UnknownConnection_ReturnsNull()
    {
        // Arrange
        
        // Act
        var result = _service.UntrackViewer(Connection1);
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TrackViewer_SameConnectionSameProduct_DoesNotDoubleCount()
    {
        // Arrange
        _service.TrackViewer(Connection1, Product1);
        
        // Act
        var result = _service.TrackViewer(Connection1, Product1);

        // Assert
        result.Should().NotBeNull();
        result.ViewerCount.Should().Be(1);
    }
}