using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace API.Services.Images;

public class AzureBlobStorageService : ImageStorageServiceBase
{
    private readonly ILogger _logger;
    private readonly BlobContainerClient _blobContainerClient;

    public AzureBlobStorageService(ILogger<AzureBlobStorageService> logger, 
        IOptions<Configuration.AzureBlobStorageOptions> options) : base(logger)
    {
        _logger = logger;
        var config = options.Value;
        
        var blobServiceClient = new BlobServiceClient(config.ConnectionString);
        _blobContainerClient = blobServiceClient.GetBlobContainerClient(config.ContainerName);
    } 
    
    public override async Task<string> SaveImageAsync(IFormFile file, string subfolder, string? prefix = null)
    {
        var fileName = PrepareAndValidateFile(file, prefix);
        
        var blobPath = $"{subfolder}/{fileName}";
        ValidateFileNameStructure(blobPath);
        
        var blobClient = _blobContainerClient.GetBlobClient(blobPath);
        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = file.ContentType,
        };
        
        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        });
        
        _logger.LogInformation("Uploaded image to Azure Blob Storage: {BlobPath}", blobPath);
        return blobPath;
    }

    public override string GetImageUrl(string fileName)
    {
        ValidateFileNameStructure(fileName);
        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        return blobClient.Uri.ToString();
    }

    public override async Task<bool> DeleteImageAsync(string fileName)
    {
        ValidateFileNameStructure(fileName);

        try
        {
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            var response = await blobClient.DeleteIfExistsAsync();
            
            if (response.Value)
                _logger.LogInformation("Deleted image from Azure Blob Storage: {BlobPath}", fileName);
            else 
                _logger.LogWarning("Image not found for deletion in Azure Blob Storage: {BlobPath}", fileName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image from Azure Blob Storage: {BlobPath}", fileName);
            return false;
        }
    }
}