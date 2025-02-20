using Ecommerce_site.Model;

namespace Ecommerce_site.Util;

public class ImageHandler(IWebHostEnvironment environment)
{
    private const string ProductImagePath = "upload/product/";
    private const string ProfileImagePath = "upload/profile/";

    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    public async Task<IList<string>> UploadImage<T>(Guid guid, List<IFormFile> files)
    {
        string basePath = environment.WebRootPath;

        IList<string> validFiles = new List<string>();
        string relativePath = typeof(T) == typeof(Product) ? ProductImagePath :
            typeof(T) == typeof(User) ? ProfileImagePath : string.Empty;

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

            // Save the file locally
            await using (var stream = new FileStream(fileFullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            validFiles.Add(fileRelativePath);
        }

        return validFiles;
    }
}