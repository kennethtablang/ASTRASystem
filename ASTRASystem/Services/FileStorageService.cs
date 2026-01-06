using ASTRASystem.Interfaces;
using SkiaSharp;

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

                // Compress image
                using var compressedStream = CompressImage(fileStream, fileName);

                // Save file
                using (var outputStream = new FileStream(filePath, FileMode.Create))
                {
                    await compressedStream.CopyToAsync(outputStream);
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
        private Stream CompressImage(Stream inputStream, string fileName)
        {
            try
            {
                // Reset stream position if possible
                if (inputStream.CanSeek)
                {
                    inputStream.Position = 0;
                }

                using var originalBitmap = SKBitmap.Decode(inputStream);
                if (originalBitmap == null)
                {
                    _logger.LogWarning("Could not decode image {FileName}, saving as is.", fileName);
                    if (inputStream.CanSeek) inputStream.Position = 0;
                    return inputStream;
                }

                // Resize if needed
                int maxWidth = 800;
                int maxHeight = 800;
                var resizedBitmap = originalBitmap;

                if (originalBitmap.Width > maxWidth || originalBitmap.Height > maxHeight)
                {
                    float ratioX = (float)maxWidth / originalBitmap.Width;
                    float ratioY = (float)maxHeight / originalBitmap.Height;
                    float ratio = Math.Min(ratioX, ratioY);

                    int newWidth = (int)(originalBitmap.Width * ratio);
                    int newHeight = (int)(originalBitmap.Height * ratio);

                    var info = new SKImageInfo(newWidth, newHeight);
                    resizedBitmap = originalBitmap.Resize(info, SKFilterQuality.High);
                }

                // Encode to image
                using var image = SKImage.FromBitmap(resizedBitmap);
                var data = image.Encode(SKEncodedImageFormat.Jpeg, 75); // Quality 75

                // Dispose resized bitmap if it's different from original
                if (resizedBitmap != originalBitmap)
                {
                    resizedBitmap.Dispose();
                }

                return data.AsStream();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compressing image {FileName}, saving as is.", fileName);
                if (inputStream.CanSeek) inputStream.Position = 0;
                return inputStream;
            }
        }
    }
}
