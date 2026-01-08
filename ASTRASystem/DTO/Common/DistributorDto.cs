namespace ASTRASystem.DTO.Common
{
    public class DistributorDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
