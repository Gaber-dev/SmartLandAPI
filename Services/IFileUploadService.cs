namespace SmartLandAPI.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string[] allowedExtensions, string uploadFolder);
        bool DeleteFile(string fileUrl, string uploadFolder);
    }
}
