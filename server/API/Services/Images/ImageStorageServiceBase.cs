using System.Security;

namespace API.Services.Images;

public abstract class ImageStorageServiceBase(ILogger logger) : IImageStorageService
{
    public abstract Task<string> SaveImageAsync(IFormFile file, string subfolder, string? prefix = null, CancellationToken ct = default);
    public abstract string GetImageUrl(string fileName);
    public abstract Task<bool> DeleteImageAsync(string fileName, CancellationToken ct = default);
    
    private static string GenerateUniqueFileName(string fileExtension, string? prefix = null)
    {
        var guid = Guid.NewGuid().ToString();
        return string.IsNullOrEmpty(prefix) 
            ? $"{guid}{fileExtension}" 
            : $"{prefix}_{guid}{fileExtension}";
    }

    protected static string PrepareAndValidateFile(IFormFile file, string? prefix = null)
    {
        ValidateFile(file);
        var fileExtension = Path.GetExtension(file.FileName);
        return GenerateUniqueFileName(fileExtension, prefix);
    }

    private static void ValidateFile(IFormFile file)
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

    protected void ValidateFileNameStructure(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required");
        
        if (fileName.StartsWith('/') || fileName.StartsWith('\\'))
        {
            logger.LogWarning("Security: Absolute path attempt in file name: {FileName}", fileName);
            throw new SecurityException($"File name ({fileName}) cannot start with a path separator");
        }
        if (fileName.Contains(".."))
        {
            logger.LogWarning("Security: Path traversal attempt in filename: {FileName}", fileName);
            throw new SecurityException($"File name ({fileName}) contains path traversal");
        }
        var invalidChars = new[] { '\\', '\0', ':', '*', '?', '"', '<', '>', '|' };
        if (fileName.Any(c => invalidChars.Contains(c)))
        {
            var invalidCharsFound = string.Join(", ", fileName.Where(c => invalidChars.Contains(c)).Distinct());
            logger.LogWarning("Security: Invalid characters found in file name: {FileName} includes {InvalidChars}",
                fileName, invalidCharsFound);
            throw new SecurityException($"File name ({fileName}) contains invalid characters: {invalidCharsFound}");
        }
        
    }
}