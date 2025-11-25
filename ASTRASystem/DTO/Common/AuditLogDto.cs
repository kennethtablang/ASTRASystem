namespace ASTRASystem.DTO.Common
{
    public class AuditLogDto
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; }
        public string? Meta { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
