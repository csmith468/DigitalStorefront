using System.Collections.Concurrent;

namespace API.Infrastructure.Viewers;

public interface IViewerTrackingService
{
    ViewerJoinResult TrackViewer(string connectionId, string productSlug);
    ViewerLeaveResult? UntrackViewer(string connectionId);
}

public record ViewerJoinResult(string ProductSlug, int ViewerCount, ViewerLeaveResult? PreviousProduct);
public record ViewerLeaveResult(string ProductSlug, int ViewerCount);


public class ViewerTrackingService : IViewerTrackingService
{
    private readonly ConcurrentDictionary<string, int> _viewerCounts = new();
    private readonly ConcurrentDictionary<string, string> _connectionToProduct = new();

    public ViewerJoinResult TrackViewer(string connectionId, string productSlug)
    {
        var previousResult = UntrackViewer(connectionId);
        _connectionToProduct[connectionId] = productSlug;
        
        var count = _viewerCounts.AddOrUpdate(productSlug, 1, (_, c) => c + 1);
        return new ViewerJoinResult(productSlug, count, previousResult);
    }

    public ViewerLeaveResult? UntrackViewer(string connectionId)
    {
        if (!_connectionToProduct.TryRemove(connectionId, out var productSlug))
            return null;

        var count = _viewerCounts.AddOrUpdate(productSlug, 0, (_, c) => Math.Max(0, c - 1));

        if (count == 0) _viewerCounts.TryRemove(productSlug, out _);

        return new ViewerLeaveResult(productSlug, count);
    }
}