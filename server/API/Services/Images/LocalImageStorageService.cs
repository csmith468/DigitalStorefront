namespace API.Services.Images;

public class LocalImageStorageService(ILogger<LocalImageStorageService> logger, IWebHostEnvironment env)
    : ImageStorageServiceBase(logger)
{
    private const string LocalFolder = "images";

    public override async Task<string> SaveImageAsync(IFormFile file, string subfolder, string? prefix = null)
    {
        ValidateFile(file);
        
        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = GenerateUniqueFileName(fileExtension, prefix);
        
        var directory = Path.Combine(env.WebRootPath, LocalFolder, subfolder);
        Directory.CreateDirectory(directory);
        
        var filePath = Path.Combine(directory, fileName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);
        
        return $"{subfolder}/{fileName}";
    }

    public override string GetImageUrl(string fileName)
    {
        // fileName includes subfolder: '/products/filename.jpg' 
        return $"/{LocalFolder}/{fileName}";
    }

    public override async Task<bool> DeleteImageAsync(string fileName)
    {
        var filePath = Path.Combine(env.WebRootPath, LocalFolder, fileName);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                logger.LogInformation("Deleted Image: {FilePath}", filePath);
            }
            else logger.LogWarning("Image Not Found for Deletion: {FilePath}", filePath);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to Delete Image: {FilePath}", filePath);
            return false;
        }
    }
}