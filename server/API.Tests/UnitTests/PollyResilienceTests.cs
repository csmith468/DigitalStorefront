using System.Diagnostics;
using System.Net;
using API.Extensions;
using API.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace API.Tests.UnitTests;

[Collection("PollyTests")]
[Trait("Category", "Unit")]
public class PollyResilienceTests
{
    private const string MockUrl = "http://mock";
    private const bool UseRealDelays = false;

    private static (HttpClient client, MockHttpMessageHandler mockHandler) CreatePollyClient(bool useRealDelays = UseRealDelays)
    {
        var mockHandler = new MockHttpMessageHandler();
        
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        
        var sharedPolicy = ResilienceExtensions.BuildPollyPolicy(
            services.BuildServiceProvider(),
            retryDelay: useRealDelays ? null : TimeSpan.Zero,
            timeout: useRealDelays ? null : TimeSpan.FromMinutes(1)); // 1 minute basically disables it

        services.AddHttpClient("PollyClient")
            .ConfigurePrimaryHttpMessageHandler(() => mockHandler)
            .AddPolicyHandler(sharedPolicy);
        
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var pollyClient = httpClientFactory.CreateClient("PollyClient");
        
        return (pollyClient, mockHandler);
    }
    
    [Fact]
    public async Task PollyClient_ShouldTimeout_After10Seconds()
    {
        // Arrange
        var (pollyClient, mockHandler) = CreatePollyClient(useRealDelays: true);
        mockHandler.EnqueueResponse(HttpStatusCode.OK, delay: TimeSpan.FromSeconds(15));
        
        var stopwatch = Stopwatch.StartNew();

        // Act
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            // Note: timeout is 10s so should time out before 15
            await pollyClient.GetAsync(MockUrl);
        });
        stopwatch.Stop();
        
        // Assert - Should time out around 10s, not wait for full 15s response)
        Assert.True(stopwatch.Elapsed.TotalSeconds is >= 9 and < 12,
            $"Expected timeout around 10s. Took {stopwatch.Elapsed.TotalSeconds}s.");
        
        // Assert - Should only attempt once (timeout happens before retry logic)
        Assert.Equal(1, mockHandler.CallCount);
    }

    [Fact]
    public async Task PollyClient_ShouldRetry_OnServerError()
    {
        // Arrange
        var (pollyClient, mockHandler) = CreatePollyClient(useRealDelays: true);

        for (var i = 0; i < 4; i++) // 1 initial + 3 retries
            mockHandler.EnqueueResponse(HttpStatusCode.ServiceUnavailable);
        
        var stopwatch = Stopwatch.StartNew();
        
        // Act (trigger retries)
        var response = await pollyClient.GetAsync(MockUrl);
        stopwatch.Stop();
        
        // Assert 
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(4, mockHandler.CallCount);
        
        // Exponential backoff: 2^1 + 2^2 + 2^3 = 2s + 4s + 8s = 14s minimum
        Assert.True(stopwatch.Elapsed.TotalSeconds >= 14,
            $"Expected retry backoff to take >=14s. Took {stopwatch.Elapsed.TotalSeconds}s.");
    }

    [Fact]
    public async Task PollyClient_WithRetry_TakesLongerThan_WithoutRetry()
    {
        // Arrange (regular client without Polly)
        var regularMockHandler = new MockHttpMessageHandler();
        regularMockHandler.EnqueueResponse(HttpStatusCode.ServiceUnavailable);
        var regularClient = new HttpClient(regularMockHandler);
        
        // Arrange (client with Polly)
        var (pollyClient, pollyMockHandler) = CreatePollyClient(useRealDelays: true);
        for (var i = 0; i < 4; i++) // 1 initial + 3 retries
            pollyMockHandler.EnqueueResponse(HttpStatusCode.ServiceUnavailable);
        
        // Act (regular client - no retries)
        var regularStopwatch = Stopwatch.StartNew();
        var regularResponse = await regularClient.GetAsync(MockUrl);
        regularStopwatch.Stop();
        
        // Act (Polly client - retries)
        var pollyStopWatch = Stopwatch.StartNew();
        var pollyResponse = await pollyClient.GetAsync(MockUrl);
        pollyStopWatch.Stop();
        
        // Assert (Polly should take longer than regular due to retries)
        Assert.Equal(HttpStatusCode.ServiceUnavailable, regularResponse.StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, pollyResponse.StatusCode);
        Assert.Equal(1, regularMockHandler.CallCount); // no retries (not using Polly)
        Assert.Equal(4, pollyMockHandler.CallCount); // 1+3 retries
        
        Assert.True(pollyStopWatch.Elapsed.TotalSeconds > 10,
            $"Polly client should retry with backoff (expected >10s, actual: {pollyStopWatch.Elapsed.TotalSeconds}s)");
        Assert.True(pollyStopWatch.Elapsed.TotalSeconds > regularStopwatch.Elapsed.TotalSeconds,
            $"Polly client should take longer than regular client (Polly: {pollyStopWatch.Elapsed.TotalSeconds}s, Regular: {regularStopwatch.Elapsed.TotalSeconds}s)");
    }

    [Fact]
    public async Task CircuitBreaker_ShouldOpen_After5Failures()
    {
        // Arrange
        var (pollyClient, mockHandler) = CreatePollyClient();
        
        // Queue enough failures (5 failures * (1 attempt + 3 retries each) = at least 20 responses needed)
        for (var i = 0; i < 30; i++)
            mockHandler.EnqueueResponse(HttpStatusCode.InternalServerError);

        var callsPerRequest = new List<int>();

        // Act - make 10 requests (circuit opens after 5 failures so first 5 should be 4 calls, rest should be 0 calls)
        for (var i = 1; i <= 10; i++)
        {
            var callsBefore = mockHandler.CallCount;
            try
            {
                await pollyClient.GetAsync(MockUrl);
            }
            catch
            {
                // Circuit breaker throws when open
            }
            
            var callsMadeByThisRequest = mockHandler.CallCount - callsBefore;
            callsPerRequest.Add(callsMadeByThisRequest);

            await Task.Delay(10); // Delay to better separate logs between attempts
        }
        
        // Assert
        Assert.All(callsPerRequest.Take(5), calls => 
            Assert.True(calls == 4, $"Expected 4 calls per request (1 request + 3 retries), got {calls}"));
        Assert.All(callsPerRequest.Skip(5), calls => 
            Assert.True(calls == 0, $"Expected 0 calls when circuit open, got {calls}"));
    }

    [Fact]
    public async Task PollyClient_DoesNotTimeout_OnFastSuccessfulRequest()
    {
        // Arrange
        var (pollyClient, mockHandler) = CreatePollyClient();
        mockHandler.EnqueueResponse(HttpStatusCode.OK, delay: TimeSpan.FromSeconds(1));
        
        // Act
        var response = await pollyClient.GetAsync(MockUrl);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, mockHandler.CallCount); // no retries
    }
}


