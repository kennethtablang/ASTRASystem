namespace ASTRASystem.DTO.Common
{
    public class FileUploadResponseDto
    {
        public string FileName { get; set; }
        public string Url { get; set; }
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
