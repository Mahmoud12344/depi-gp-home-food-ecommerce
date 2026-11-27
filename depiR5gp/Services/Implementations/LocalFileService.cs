using depiR5gp.Services.Interfaces;

namespace depiR5gp.Services.Implementations;

public class LocalFileService(IWebHostEnvironment env) : IFileService
{
    private const string UploadsRoot = "uploads";
    private const long MaxUploadSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".pdf"
    };

    public async Task<string> SaveAsync(IFormFile file, string folder, string? fileLabel = null)
    {
        if (file is null || file.Length <= 0)
        {
            throw new InvalidOperationException("Uploaded file cannot be empty.");
        }

        if (file.Length > MaxUploadSizeBytes)
        {
            throw new InvalidOperationException("Uploaded file exceeds the 5 MB maximum size limit.");
        }

        string ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new InvalidOperationException("Only JPG, JPEG, PNG, WEBP, and PDF files are permitted.");
        }

        string safeFolder = folder.Trim().Trim('/', '\\');
        string physicalFolder = Path.GetFullPath(Path.Combine(env.WebRootPath, UploadsRoot, safeFolder));
        string uploadsBasePath = Path.GetFullPath(Path.Combine(env.WebRootPath, UploadsRoot));
        if (!physicalFolder.StartsWith(uploadsBasePath, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid upload folder.");
        }

        Directory.CreateDirectory(physicalFolder);

        string label = string.IsNullOrWhiteSpace(fileLabel) ? "" : $"_{fileLabel}";
        string uniqueFileName = $"{Guid.NewGuid()}{label}{ext.ToLowerInvariant()}";
        string filePath = Path.Combine(physicalFolder, uniqueFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        return $"/{UploadsRoot}/{safeFolder}/{uniqueFileName}";
    }

    public Task DeleteAsync(string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return Task.CompletedTask;

        string relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        string fullPath = Path.GetFullPath(Path.Combine(env.WebRootPath, relativePath));
        string uploadsRootPath = Path.GetFullPath(Path.Combine(env.WebRootPath, UploadsRoot)) + Path.DirectorySeparatorChar;

        // Safety: only delete files inside wwwroot/uploads/ (never delete app static icons/assets)
        if (fullPath.StartsWith(uploadsRootPath, StringComparison.Ordinal) && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
