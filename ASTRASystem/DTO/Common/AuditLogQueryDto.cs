namespace ASTRASystem.DTO.Common
{
    public class AuditLogQueryDto
    {
        public string? UserId { get; set; }
        public string? Action { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
