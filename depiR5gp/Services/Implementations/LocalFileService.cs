using depiR5gp.Services.Interfaces;

namespace depiR5gp.Services.Implementations;
public class LocalFileService(IWebHostEnvironment env) : IFileService
{
    private const string UploadsRoot = "uploads";

    public async Task<string> SaveAsync(IFormFile file, string folder, string? fileLabel = null)
    {
        string physicalFolder = Path.Combine(env.WebRootPath, UploadsRoot, folder);
        Directory.CreateDirectory(physicalFolder); // does nothing if it already exists

        string label = string.IsNullOrWhiteSpace(fileLabel) ? "" : $"_{fileLabel}";
        string uniqueFileName = $"{Guid.NewGuid()}{label}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(physicalFolder, uniqueFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        return $"/{UploadsRoot}/{folder}/{uniqueFileName}";
    }

    public Task DeleteAsync(string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return Task.CompletedTask;

        string relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        string fullPath = Path.GetFullPath(Path.Combine(env.WebRootPath, relativePath));

        // Safety: never delete anything outside wwwroot
        if (fullPath.StartsWith(Path.GetFullPath(env.WebRootPath)) && File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}