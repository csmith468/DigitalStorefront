using API.Infrastructure.Viewers;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public class ProductViewerHub : Hub
{
    private readonly IViewerTrackingService _viewerTrackingService;
    public ProductViewerHub(IViewerTrackingService viewerTrackingService)
    {
        _viewerTrackingService = viewerTrackingService;
    }

    public async Task JoinProductAsync(string productSlug)
    {
        var result = _viewerTrackingService.TrackViewer(Context.ConnectionId, productSlug);

        if (result.PreviousProduct != null)
        {
            await BroadcastViewerCountUpdateAsync(
                result.PreviousProduct.ProductSlug,
                result.PreviousProduct.ViewerCount);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, productSlug);
        await BroadcastViewerCountUpdateAsync(productSlug, result.ViewerCount);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var result = _viewerTrackingService.UntrackViewer(Context.ConnectionId);

        if (result != null)
            await BroadcastViewerCountUpdateAsync(result.ProductSlug, result.ViewerCount);

        await base.OnDisconnectedAsync(exception);
    }

    private async Task BroadcastViewerCountUpdateAsync(string productSlug, int count)
    {
        await Clients.Group(productSlug).SendAsync("ViewerCountUpdated", count);
    }
}