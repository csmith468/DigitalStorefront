using System.Net;

namespace API.Tests.Helpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<(HttpStatusCode statusCode, TimeSpan delay)> _responses = new();
    public int CallCount { get; private set; }

    // Clean API for setting up responses
    public void EnqueueResponse(HttpStatusCode statusCode, TimeSpan delay = default)
    {
        _responses.Enqueue((statusCode, delay));
    }

    public void Reset()
    {
        _responses.Clear();
        CallCount = 0;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        
        if (_responses.Count == 0)
            throw new InvalidOperationException("No responses enqueued. Call EnqueueResponse() before making requests.");

        var (statusCode, delay) = _responses.Dequeue();
        
        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, cancellationToken);
        
        return new HttpResponseMessage(statusCode);
    }
}