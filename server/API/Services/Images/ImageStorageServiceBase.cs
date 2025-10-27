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
        
        const long maxFileSize = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxFileSize)
            throw new ArgumentException($"File is too large ({maxFileSize / 1024 / 1024} MB)");
        
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException($"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");

        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedMimeTypes.Contains(file.ContentType))
            throw new ArgumentException($"Invalid file content type: {file.ContentType}");
    }
}