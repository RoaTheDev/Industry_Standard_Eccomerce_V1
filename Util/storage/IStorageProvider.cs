namespace Ecommerce_site.Util.storage;

public interface IStorageProvider
{
    Task<IList<string>> UploadFilesAsync<T>(Guid guid, List<IFormFile> files);
    Task DeleteFileAsync(string filePath);
}