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
        string webRootPath = Path.GetFullPath(env.WebRootPath);
        string fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        string relativeToWebRoot = Path.GetRelativePath(webRootPath, fullPath);

        // Safety: never delete anything outside wwwroot.
        if (!relativeToWebRoot.StartsWith("..", StringComparison.Ordinal)
            && !Path.IsPathRooted(relativeToWebRoot)
            && File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}