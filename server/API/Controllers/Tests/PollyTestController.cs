using API.Setup;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Tests;

[ApiController]
[Route("polly-test")]
[ApiVersion("1.0")]
public class PollyTestController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    // Test 1: Without Polly - hang for 15 seconds
    [HttpGet("without-polly")]
    public async Task<ActionResult> TestWithoutPolly()
    {
        var client =  httpClientFactory.CreateClient();
        try
        {
            await client.GetAsync(CreateLink(200, 15));
            return Ok("Request completed successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
    
    // Test 2: Timeout after 10 seconds
    [HttpGet("timeout")]
    public async Task<ActionResult> TestTimeout()
    {
        var client = httpClientFactory.CreateClient("PollyClient");

        try
        {
            await client.GetAsync(CreateLink(200, 15));
            return Ok("Request completed - should have errored");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Timeout as Expected: {ex.Message}");
        }
    }
    
    // Test 3: Retry (should see 3 attempts in logs)
    [HttpGet("retry")]
    public async Task<ActionResult> TestRetry()
    {
        var client = httpClientFactory.CreateClient("PollyClient");

        try
        {
            // 503 (Service Unavailable) triggers retry
            var response = await client.GetAsync(CreateLink(503));

            return !response.IsSuccessStatusCode 
                ? StatusCode(500, $"Failed After Retries: HTTP {(int)response.StatusCode} {response.ReasonPhrase}") 
                : Ok("Request Succeeded (Unexpected)");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed After Retries: {ex.Message}");
        }
    }
    // Test 4: Test Circuit Breaker (loops through 5 times to open circuit, fails on 6th)
    [HttpGet("circuit-breaker")]
    public async Task<ActionResult> TestCircuitBreaker()
    {
        var client = httpClientFactory.CreateClient("PollyClient");
        var results = new List<string>();

        for (var i = 1; i <= 6; i++)
        {
            try
            {
                Console.WriteLine($"\n--- Attempt {i} ---");
                var response = await client.GetAsync(CreateLink(500));

                if (!response.IsSuccessStatusCode)
                {
                    results.Add($"Attempt {i}: HTTP {(int)response.StatusCode} (Failed as Expected)");
                    Console.WriteLine($"Attempt {i}: HTTP {(int)response.StatusCode} (Failed as Expected)");
                }
                else
                {
                    results.Add($"Attempt {i}: Success (Unexpected)");
                    Console.WriteLine($"Attempt {i}: Success (Unexpected)");
                }
            }
            catch (Exception ex)
            {
                results.Add($"Attempt {i}: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Attempt {i}: Exception - {ex.Message}");
            }

            // Delay to better separate logs between attempts
            await Task.Delay(500);
        }

        return Ok(new
        {
            Message = "Circuit breaker test complete",
            Results = results,
            ExpectedBehavior = "First 5 attempts should retry and fail. Sixth should fail immediately (circuit open)."
        });
    }

    private static string CreateLink(int statusCode, int? seconds = null)
    {
        return seconds.HasValue 
            ? $"https://httpbin.org/delay/{seconds}" 
            : $"https://httpbin.org/status/{statusCode}";
    }
}