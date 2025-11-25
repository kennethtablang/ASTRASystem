namespace ASTRASystem.DTO.User
{
    public class UserQueryDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? IsApproved { get; set; }
        public long? DistributorId { get; set; }
        public long? WarehouseId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
