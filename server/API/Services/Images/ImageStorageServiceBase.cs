namespace API.Services.Images;

public abstract class ImageStorageServiceBase(ILogger<ImageStorageServiceBase> logger) : IImageStorageService
{
    public abstract Task<string> SaveImageAsync(IFormFile file, string subfolder, string? prefix = null);
    public abstract string GetImageUrl(string fileName);
    public abstract Task<bool> DeleteImageAsync(string fileName);
    
    protected string GenerateUniqueFileName(string fileExtension, string? prefix = null)
    {
        var guid = Guid.NewGuid().ToString();
        return string.IsNullOrEmpty(prefix) 
            ? $"{guid}{fileExtension}" 
            : $"{prefix}_{guid}{fileExtension}";
    }

    protected void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required");
    }
}