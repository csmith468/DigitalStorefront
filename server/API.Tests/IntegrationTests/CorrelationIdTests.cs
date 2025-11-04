using API.Models.Constants;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests.IntegrationTests;

[Collection("Database")]
[Trait("Category", "Integration")]
public class CorrelationIdTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Request_WithoutCorrelationId_GeneratesNewId()
    {
        // Act
        var response = await Client.GetAsync("api/metadata/categories");

        // Assert
        response.Headers.Should().ContainKey(HeaderNames.CorrelationId);
        var correlationId = response.Headers.GetValues(HeaderNames.CorrelationId).First();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue("Correlation ID should be a valid GUID");
    }

    [Fact]
    public async Task Error_Response_IncludesCorrelationIdInHeader()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "api/products/9999");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey(HeaderNames.CorrelationId);
        var correlationId = response.Headers.GetValues(HeaderNames.CorrelationId).First();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue("Correlation ID should be a valid GUID");
    }

    [Fact]
    public async Task MultipleRequests_GenerateDifferentCorrelationIds()
    {
        // Act
        var response1 = await Client.GetAsync("/api/metadata/categories");
        var response2 = await Client.GetAsync("/api/metadata/tags");

        // Assert
        var correlationId1 = response1.Headers.GetValues(HeaderNames.CorrelationId).First();
        var correlationId2 = response2.Headers.GetValues(HeaderNames.CorrelationId).First();

        correlationId1.Should().NotBe(correlationId2, "Each request should have a unique correlation ID");
    }
}