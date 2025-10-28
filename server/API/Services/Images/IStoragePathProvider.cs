namespace API.Services.Images;

/// <summary>
/// Abstraction for getting the storage root path without needing IWebHostEnvironment in LocalImageStorageService
/// Also allows image services to work in both web and console environments.
/// </summary>
public interface IStoragePathProvider
{
    /// wwwroot folder locally or configured directory in console apps
    string StorageRootPath { get; }
}