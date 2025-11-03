using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

// NOTE: Required is only checked in DependencyInjectionExtensions if useAzureStorage is true
// Could have done manual validation but wanted it to fail-fast and have declarative attributes
public class AzureBlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    [Required(ErrorMessage = "Azure Blob Storage connection string is required")]
    public string ConnectionString { get; set; } = "";

    [Required(ErrorMessage = "Azure Blob Storage container name is required")]
    public string ContainerName { get; set; } = "";
}