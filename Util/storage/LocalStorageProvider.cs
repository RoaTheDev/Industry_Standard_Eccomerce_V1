using Ecommerce_site.Model;

namespace Ecommerce_site.Util.storage;

public class LocalStorageProvider : IStorageProvider
{
    private readonly IWebHostEnvironment _environment;
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    public LocalStorageProvider(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<IList<string>> UploadFilesAsync<T>(Guid guid, List<IFormFile> files)
    {
        string basePath = _environment.WebRootPath;
        IList<string> validFiles = new List<string>();

        string relativePath = typeof(T) == typeof(ProductImage) ? "upload/product/" :
            typeof(T) == typeof(User) ? "upload/profile/" : string.Empty;

        if (string.IsNullOrEmpty(relativePath))
            throw new InvalidOperationException("Unsupported entity type for image upload.");

        string fullPath = Path.Combine(basePath, relativePath);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        foreach (var file in files)
        {
            string extension = Path.GetExtension(file.FileName).ToLower();

            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Invalid file type: {extension}");
            }

            string uniqueFileName = $"{guid}_{Path.GetRandomFileName()}{extension}";
            string fileFullPath = Path.Combine(fullPath, uniqueFileName); // Full path
            string fileRelativePath = Path.Combine(relativePath, uniqueFileName); // Relative path for DB

            await using (var stream = new FileStream(fileFullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            validFiles.Add(fileRelativePath);
        }

        return validFiles;
    }

    public Task DeleteFileAsync(string filePath)
    {
        string fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}