namespace ASTRASystem.DTO.Store
{
    public class StoreDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? BarangayId { get; set; }
        public string? BarangayName { get; set; }
        public long? CityId { get; set; }
        public string? CityName { get; set; }
        public string? Province { get; set; }
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        public decimal CreditLimit { get; set; }
        public string? PreferredPaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
