namespace ASTRASystem.DTO.User
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovalMessage { get; set; }
        public List<string> Roles { get; set; } = new();
        public long? DistributorId { get; set; }
        public string? DistributorName { get; set; }
        public long? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
    }
}
