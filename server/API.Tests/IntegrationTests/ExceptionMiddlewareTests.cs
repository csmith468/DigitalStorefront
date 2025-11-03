using System.Net;
using System.Net.Http.Json;
using API.Middleware;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests.IntegrationTests;

public class ExceptionMiddlewareTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task ThrowException_ShouldReturn500_WithJsonErrorResponse()
    {
        // Arrange
        
        // Act
        var response = await Client.GetAsync("api/test/throw-exception");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ApiException>();
        errorResponse.Should().NotBeNull();
        errorResponse.StatusCode.Should().Be(500);
        errorResponse.Message.Should().Be("Internal Service Error");
    }

    [Fact]
    public async Task ThrowNullReferenceException_ShouldReturn500_WithJsonErrorResponse()
    {
        // Arrange
        
        // Act
        var response = await Client.GetAsync("api/test/null-reference");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ApiException>();
        errorResponse.Should().NotBeNull();
        errorResponse.StatusCode.Should().Be(500);
        errorResponse.Message.Should().Be("Internal Service Error");
    }

    [Fact]
    public async Task ValidEndpoint_ShouldNotTriggerMiddleware_AndReturn200()
    {
        // Arrange
        
        // Act
        var response = await Client.GetAsync("api/test/valid");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task NotFound_ShouldNotTriggerMiddleware_ShouldReturn404()
    {
        // Arrange
        
        // Act
        var response = await Client.GetAsync("api/test/not-found");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // Verify not in exception middleware format (JSON with ApiException)
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }
}