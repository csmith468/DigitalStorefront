using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public class ProductViewerHub : Hub
{
    private static readonly ConcurrentDictionary<string, int> _viewerCounts = new();
    private static readonly ConcurrentDictionary<string, string> _connectionToProduct = new();

    // NOTE: Joins or replaces existing connection
    public async Task JoinProductAsync(string productSlug)
    {
        if (_connectionToProduct.TryGetValue(Context.ConnectionId, out var previousSlug))
            await LeaveProductInternalAsync(previousSlug);

        _connectionToProduct[Context.ConnectionId] = productSlug;
        await Groups.AddToGroupAsync(Context.ConnectionId, productSlug);

        var count = _viewerCounts.AddOrUpdate(productSlug, 1, (_, c) => c + 1);
        await BroadcastViewerCountUpdateAsync(productSlug, count);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionToProduct.TryRemove(Context.ConnectionId, out var productSlug))
            await LeaveProductInternalAsync(productSlug);
        await base.OnDisconnectedAsync(exception);
    }

    private async Task LeaveProductInternalAsync(string productSlug)
    {
        var count = _viewerCounts.AddOrUpdate(productSlug, 0, (_, c) => Math.Max(0, c - 1));
        if (count == 0)
            _viewerCounts.TryRemove(productSlug, out _);
        
        await BroadcastViewerCountUpdateAsync(productSlug, count);
    }

    private async Task BroadcastViewerCountUpdateAsync(string productSlug, int count)
    {
        await Clients.Group(productSlug).SendAsync("ViewerCountUpdated", count);
    }
}