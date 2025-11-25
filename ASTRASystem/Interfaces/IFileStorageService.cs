namespace ASTRASystem.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<byte[]> DownloadFileAsync(string fileUrl);
        Task<string> GetSignedUrlAsync(string fileUrl, int expirationMinutes = 60);
    }
}
