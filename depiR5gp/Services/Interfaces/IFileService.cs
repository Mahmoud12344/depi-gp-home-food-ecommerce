namespace depiR5gp.Services.Interfaces;

public interface IFileService {
     Task<string> SaveAsync(IFormFile file, string folder, string? fileLabel = null);

     Task DeleteAsync(string? fileUrl);
}