using API.Database;
using API.Models.DboTables;
using API.Services.Images;

namespace DatabaseCleanup;

public class DatabaseCleaner(IDataContextDapper dapper, IImageStorageService imageService)
{
    private const string Indent = "   ";

    public bool Confirm()
    {
        Console.WriteLine("=== Database Cleanup Utility ===");
        Console.WriteLine("This will delete all images and database records.");
        Console.WriteLine("WARNING: This action cannot be undone!");
        Console.Write("\nType 'yes' to continue: ");

        var confirmation = Console.ReadLine();
        return confirmation?.ToLower() == "yes";
    }
    
    public async Task ExecuteAsync()
    {
        Console.WriteLine("\n1. Fetching all product images from database...");
        var images = (await dapper.QueryAsync<ProductImage>("SELECT * FROM dbo.productImage")).ToList();
        Console.WriteLine($"{Indent}Found {images.Count} images to delete.");

        Console.WriteLine("\n2. Clearing database tables (in transaction)...");
        await dapper.WithTransactionAsync(async () =>
        {
            await dapper.ExecuteAsync("""
                                       DELETE FROM dbo.productImage;
                                       DELETE FROM dbo.productSubcategory;
                                       DELETE FROM dbo.product;
                                       DELETE FROM dbo.subcategory;
                                       DELETE FROM dbo.category;
                                       DELETE FROM dbo.productType;
                                       DELETE FROM dsf.auth;
                                       DELETE FROM dsf.[user]
                                       """);
        });
        Console.WriteLine($"{Indent}All tables cleared.");

        Console.WriteLine("\n3. Deleting images from storage...");
        var deletedCount = 0;
        foreach (var image in images)
        {
            try
            {
                var deleted = await imageService.DeleteImageAsync(image.ImageUrl);
                if (deleted) deletedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Indent}Warning: Failed to delete {image.ImageUrl}: {ex.Message}");
            }
        }
        Console.WriteLine($"{Indent}Deleted {deletedCount}/{images.Count} image files.");
    }
}