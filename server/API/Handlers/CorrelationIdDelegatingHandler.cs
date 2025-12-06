using API.Infrastructure.Contexts;
using API.Models.Constants;

namespace API.Handlers;

public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public CorrelationIdDelegatingHandler(ICorrelationIdAccessor correlationIdAccessor)
    {
        _correlationIdAccessor = correlationIdAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = _correlationIdAccessor.CorrelationId;
        if (!string.IsNullOrEmpty(correlationId))
            request.Headers.Add(HeaderNames.CorrelationId, correlationId);
        
        return await base.SendAsync(request, cancellationToken);
    }
}