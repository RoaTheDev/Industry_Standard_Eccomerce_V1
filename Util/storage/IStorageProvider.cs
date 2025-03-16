namespace Ecommerce_site.Util.storage;

public interface IStorageProvider
{
    Task<IList<string>> UploadFileAsync<T>(Guid guid, List<IFormFile> files);
    Task<string> UploadFileAsync<T>(Guid guid, IFormFile file);
    
    Task DeleteFileAsync(string filePath);
}