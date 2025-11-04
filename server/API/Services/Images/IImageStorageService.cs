namespace API.Services.Images;

public interface IImageStorageService
{
    /// <summary>
    /// Saves an uploaded file with optional prefix for organization
    /// </summary>
    /// <param name="file">Uploaded file</param>
    /// <param name="subfolder">Subfolder in wwwroot/images ("products", "subcategories", etc.)</param>
    /// <param name="prefix">Optional prefix for filename (productId, subcategoryId, etc.)</param>
    /// <returns>Filepath ($"{subfolder}/{uniqueFileName}")</returns>
    Task<string> SaveImageAsync(IFormFile file, string subfolder, string? prefix = null, CancellationToken ct = default);
    string GetImageUrl(string fileName);
    Task<bool> DeleteImageAsync(string fileName, CancellationToken ct = default);
}