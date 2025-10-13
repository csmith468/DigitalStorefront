using API.Models.DboTables;

namespace API.Services.Images;

public interface IImageStorageService
{
    /// <summary>
    /// Saves an uploaded file with optional prefix for oganization
    /// </summary>
    /// <param name="file">Uploaded file</param>
    /// <param name="subfolder">Subfolder in wwwroot/images ("products", "subcategories", etc.)</param>
    /// <param name="prefix">Optional prefix for filename (productId, subcategoryId, etc.)</param>
    /// <returns>Filepath ($"{subfolder}/{uniqueFileName}")</returns>
    Task<string> SaveImageAsync(IFormFile file, string subfolder, string? prefix = null);
    string GetImageUrl(string fileName);
    Task<bool> DeleteImageAsync(string fileName);
}