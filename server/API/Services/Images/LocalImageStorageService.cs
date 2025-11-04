using System.Security;

namespace API.Services.Images;

public class LocalImageStorageService : ImageStorageServiceBase
{
    private readonly ILogger _logger;
    private readonly IStoragePathProvider _storagePathProvider;

    public LocalImageStorageService(ILogger<LocalImageStorageService> logger, IStoragePathProvider storagePathProvider)
        : base(logger)
    {
        _logger = logger;
        _storagePathProvider = storagePathProvider;
    }
    
    private const string LocalFolder = "images";

    public override async Task<string> SaveImageAsync(IFormFile file, string subfolder, string? prefix = null, CancellationToken ct = default)
    {
        var fileName = PrepareAndValidateFile(file, prefix);
        
        var directory = Path.Combine(_storagePathProvider.StorageRootPath, LocalFolder, subfolder);
        Directory.CreateDirectory(directory);
        
        var filePath = Path.Combine(directory, fileName);
        ValidateFilePath(filePath, directory);
        
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream, ct);
        
        return $"{subfolder}/{fileName}";
    }

    public override string GetImageUrl(string fileName)
    {
        // fileName includes subfolder: '/products/filename.jpg' 
        return $"/{LocalFolder}/{fileName}";
    }

    public override Task<bool> DeleteImageAsync(string fileName, CancellationToken ct = default)
    {
        ValidateFileNameStructure(fileName);
        
        var filePath = Path.Combine(_storagePathProvider.StorageRootPath, LocalFolder, fileName);
        var directory = Path.Combine(_storagePathProvider.StorageRootPath, LocalFolder); // NOTE: subfolder is on filePath
        ValidateFilePath(filePath, directory);
        
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted Image: {FilePath}", filePath);
            }
            else _logger.LogWarning("Image Not Found for Deletion: {FilePath}", filePath);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to Delete Image: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    private void ValidateFilePath(string filePath, string expectedDirectory)
    {
        var fullPath = Path.GetFullPath(filePath);
        var fullExpectedPath = Path.GetFullPath(expectedDirectory);

        if (!fullPath.StartsWith(fullExpectedPath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path traversal detected: {FullPath} outside {ExpectedPath}", fullPath, fullExpectedPath);
            throw new SecurityException("Invalid file path (path traversal detected)");
        }
    }
    
    
}