namespace ASTRASystem.DTO.Auth
{
    public class AuthResponseDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovalMessage { get; set; }
        public long? DistributorId { get; set; }
        public long? WarehouseId { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}
