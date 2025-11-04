namespace API.Services.Images;

public class WebStoragePathProvider : IStoragePathProvider
{
    private readonly IWebHostEnvironment _environment;

    public WebStoragePathProvider(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public string StorageRootPath => _environment.WebRootPath;
    
}