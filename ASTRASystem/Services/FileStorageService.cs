using ASTRASystem.Interfaces;

namespace ASTRASystem.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _uploadPath;

        public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
            _uploadPath = Path.Combine(_environment.ContentRootPath, "Assets", "Uploads");

            // Ensure directory exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(_uploadPath, uniqueFileName);

                // Save file
                using (var outputStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(outputStream);
                }

                // Return relative URL
                var fileUrl = $"/assets/uploads/{uniqueFileName}";
                _logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);

                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", fileName);
                throw;
            }
        }

        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType)
        {
            using var memoryStream = new MemoryStream(fileBytes);
            return await UploadFileAsync(memoryStream, fileName, contentType);
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Extract filename from URL
                var fileName = Path.GetFileName(fileUrl);
                var filePath = Path.Combine(_uploadPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File deleted successfully: {FileName}", fileName);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileUrl}", fileUrl);
                return false;
            }
        }

        public async Task<byte[]> DownloadFileAsync(string fileUrl)
        {
            try
            {
                var fileName = Path.GetFileName(fileUrl);
                var filePath = Path.Combine(_uploadPath, fileName);

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {fileName}");
                }

                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileUrl}", fileUrl);
                throw;
            }
        }

        public Task<string> GetSignedUrlAsync(string fileUrl, int expirationMinutes = 60)
        {
            // For local file storage, just return the URL
            // In production with cloud storage (S3, Azure Blob), implement proper signed URLs
            return Task.FromResult(fileUrl);
        }
    }
}
