namespace ASTRASystem.DTO.Store
{
    public class StoreDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Barangay { get; set; }
        public string? City { get; set; }
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        public decimal CreditLimit { get; set; }
        public string? PreferredPaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
