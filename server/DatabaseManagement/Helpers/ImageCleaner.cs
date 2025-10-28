using API.Database;
using API.Models.DboTables;
using API.Services.Images;

namespace DatabaseManagement.Helpers;

public class ImageCleaner
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly IImageStorageService _imageService;
    public ImageCleaner(IQueryExecutor queryExecutor, IImageStorageService imageService)
    {
        _queryExecutor = queryExecutor;
        _imageService = imageService;
    }
    
    public async Task DeleteImagesOnlyAsync()
    {
        Console.WriteLine("   Fetching all product images from database...");
        var images = (await _queryExecutor.QueryAsync<ProductImage>("SELECT * FROM dbo.productImage")).ToList();
        Console.WriteLine($"   Found {images.Count} images to delete.");

        Console.WriteLine("   Deleting images from storage...");
        var deletedCount = 0;
        foreach (var image in images)
        {
            try
            {
                var deleted = await _imageService.DeleteImageAsync(image.ImageUrl);
                if (deleted) deletedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Warning: Failed to delete {image.ImageUrl}: {ex.Message}");
            }
        }
        Console.WriteLine($"   Deleted {deletedCount}/{images.Count} image files.");
    }

}