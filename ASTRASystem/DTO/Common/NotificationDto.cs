namespace ASTRASystem.DTO.Common
{
    public class NotificationDto
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
        public DateTime? SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
