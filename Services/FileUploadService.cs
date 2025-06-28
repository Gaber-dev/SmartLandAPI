namespace SmartLandAPI.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(IWebHostEnvironment webHostEnvironment, ILogger<FileUploadService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string[] allowedExtensions, string uploadFolder)
        {
            try
            {
                
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    throw new InvalidOperationException($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");
                }

                
                var webRootPath = _webHostEnvironment.WebRootPath;
                var fullUploadPath = Path.Combine(webRootPath, uploadFolder);

                if (!Directory.Exists(fullUploadPath))
                {
                    Directory.CreateDirectory(fullUploadPath);
                }

                
                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(fullUploadPath, uniqueFileName);

               
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                
                return $"/{uploadFolder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw;
            }
        }

        public bool DeleteFile(string fileUrl, string uploadFolder)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return true;

                var webRootPath = _webHostEnvironment.WebRootPath;
                var filePath = fileUrl.StartsWith($"/{uploadFolder}/")
                    ? Path.Combine(webRootPath, fileUrl.TrimStart('/'))
                    : Path.Combine(webRootPath, uploadFolder, fileUrl);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return false;
            }
        }
    }
}
