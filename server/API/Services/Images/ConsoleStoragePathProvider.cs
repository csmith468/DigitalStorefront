namespace API.Services.Images;

public class ConsoleStoragePathProvider(string storagePath) : IStoragePathProvider
{
    public string StorageRootPath { get; } = storagePath;
}