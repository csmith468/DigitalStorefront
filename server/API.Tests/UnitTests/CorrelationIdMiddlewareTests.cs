using API.Middleware;
using API.Models.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace API.Tests.UnitTests;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task Middleware_PreservesClientCorrelationId()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(next: (_) => Task.CompletedTask);
        var context = new DefaultHttpContext();
        var expectedId = Guid.NewGuid().ToString();
        context.Request.Headers[HeaderNames.CorrelationId] = expectedId;
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        context.Items["CorrelationId"].Should().Be(expectedId);
        context.Response.Headers[HeaderNames.CorrelationId].ToString().Should().Be(expectedId);
    }
}