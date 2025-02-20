using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Ecommerce_site.Util.storage;

public class AzureBlobStorageProvider : IStorageProvider
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    public AzureBlobStorageProvider(string connectionString, string containerName)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerName = containerName;
    }

    public async Task<IList<string>> UploadFilesAsync<T>(Guid guid, List<IFormFile> files)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        IList<string> uploadedUrls = new List<string>();

        foreach (var file in files)
        {
            string extension = Path.GetExtension(file.FileName).ToLower();

            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Invalid file type: {extension}");
            }

            string uniqueFileName = $"{guid}_{Path.GetRandomFileName()}{extension}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // Upload the file to Azure Blob Storage
            await using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            uploadedUrls.Add(blobClient.Uri.ToString());
        }

        return uploadedUrls;
    }

    public async Task DeleteFileAsync(string filePath)
    {
        var blobClient = new BlobClient(new Uri(filePath));
        await blobClient.DeleteIfExistsAsync();
    }
}