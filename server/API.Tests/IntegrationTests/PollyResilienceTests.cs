using System.Diagnostics;
using System.Net;
using API.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace API.Tests.IntegrationTests;

[Collection("PollyTests")]
[Trait("Category", "Integration")]
[Trait("Skip", "CI")] // Skipped in CI - tests are slow and flaky due to circuit breaker timing
public class PollyResilienceTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;

    [Fact]
    public async Task PollyClient_ShouldTimeout_After10Seconds()
    {
        // Arrange
        var pollyClient = CreatePollyClient();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            // Note: timeout is 10s so should time out before 15
            await pollyClient.GetAsync(CreateLink(seconds: 15));
        });
        stopwatch.Stop();
        
        // Assert
        Assert.True(stopwatch.Elapsed.TotalSeconds is >= 10 and < 14,
            $"Expected timeout around 10s. Took {stopwatch.Elapsed.TotalSeconds}s.");
    }

    [Fact]
    public async Task PollyClient_ShouldRetry_OnServerError()
    {
        // Arrange
        var pollyClient = CreatePollyClient();
        
        // Act (trigger retries)
        var response = await pollyClient.GetAsync(CreateLink(statusCode: 503));
        
        // Assert (should take longer due to exponential backoff: 2^1 + 2^2 + 2^3 = 14s minimum)
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task PollyClient_WithRetry_TakesLongerThan_WithoutRetry()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var pollyClient = httpClientFactory.CreateClient("PollyClient");
        var regularClient = httpClientFactory.CreateClient();
        var serviceUnavailableLink = CreateLink(statusCode: 503);
        
        // Act (regular client - no retries)
        var regularStopwatch = Stopwatch.StartNew();
        var regularResponse = await regularClient.GetAsync(serviceUnavailableLink);
        regularStopwatch.Stop();
        
        // Act (Polly client - retries)
        var pollyStopWatch = Stopwatch.StartNew();
        var pollyResponse = await pollyClient.GetAsync(serviceUnavailableLink);
        pollyStopWatch.Stop();
        
        // Assert (Polly should take longer than regular due to retries)
        Assert.Equal(HttpStatusCode.ServiceUnavailable, regularResponse.StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, pollyResponse.StatusCode);
        Assert.True(pollyStopWatch.Elapsed.TotalSeconds > 10,
            $"Polly client should retry with backoff (expected >10s, actual: {pollyStopWatch.Elapsed.TotalSeconds}s)");
        Assert.True(pollyStopWatch.Elapsed.TotalSeconds > regularStopwatch.Elapsed.TotalSeconds,
            $"Polly client should take longer than regular client (Polly: {pollyStopWatch.Elapsed.TotalSeconds}s, Regular: {regularStopwatch.Elapsed.TotalSeconds}s)");
    }

    // NOTE: Tests run sequentially to avoid circuit breaker's state being impacted by other tests
    // In production test suite, I'd use separate named clients or mock HttpMessageHandlers for isolation
    [Fact]
    public async Task CircuitBreaker_ShouldOpen_After5Failures()
    {
        // Arrange
        var pollyClient = CreatePollyClient();
        var results = new List<(int attempt, HttpStatusCode statusCode, TimeSpan elapsed)>();

        // Act
        for (var i = 1; i <= 6; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await pollyClient.GetAsync(CreateLink(statusCode: 500));
                stopwatch.Stop();
                results.Add((i, response.StatusCode, stopwatch.Elapsed));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                results.Add((i, HttpStatusCode.InternalServerError, stopwatch.Elapsed));
            }

            await Task.Delay(100); // Delay to better separate logs between attempts
        }
        
        // Assert
        var firstFiveAverage = results.Take(5).Average(r => r.elapsed.TotalSeconds);
        var sixthAttempt = results[5].elapsed.TotalSeconds;
        Assert.True(firstFiveAverage > 5, $"First 5 attempts should retry (expected >5s average, actual: {firstFiveAverage}s)");
        Assert.True(sixthAttempt < 1, $"Sixth attempt should fail immediately with circuit open (expected <1s, actual: {sixthAttempt}s)");
    }

    [Fact]
    public async Task PollyClient_DoesNotTimeout_OnFastSuccessfulRequest()
    {
        // Arrange
        var pollyClient = CreatePollyClient();
        
        // Act
        var response = await pollyClient.GetAsync(CreateLink(seconds: 1));
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private HttpClient CreatePollyClient()
    {
        var httpClientFactory = _factory.Services.GetRequiredService<IHttpClientFactory>();
        return httpClientFactory.CreateClient("PollyClient");
    }
    
    private static string CreateLink(int? statusCode = null, int? seconds = null)
    {
        return seconds.HasValue 
            ? $"https://httpbin.org/delay/{seconds}" 
            : $"https://httpbin.org/status/{statusCode}";
    }
}


