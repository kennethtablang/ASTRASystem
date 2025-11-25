namespace ASTRASystem.DTO.User
{
    public class UserListItemDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsApproved { get; set; }
        public List<string> Roles { get; set; } = new();
        public string? DistributorName { get; set; }
        public string? WarehouseName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
