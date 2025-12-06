using API.Models.Constants;

namespace API.Infrastructure.Contexts;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
}

public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string? CorrelationId => _httpContextAccessor.HttpContext?.Items[ContextKeys.CorrelationId]?.ToString();
}