using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Store
{
    public class CreateStoreDto
    {
        [Required(ErrorMessage = "Store name is required")]
        [MaxLength(250)]
        public string Name { get; set; }

        public long? BarangayId { get; set; }

        public long? CityId { get; set; }

        [MaxLength(250)]
        public string? AddressLine1 { get; set; }

        [MaxLength(250)]
        public string? AddressLine2 { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [MaxLength(200)]
        public string? OwnerName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(100)]
        public string? Phone { get; set; }

        [Range(0, 999999999999.99)]
        public decimal CreditLimit { get; set; } = 0m;

        [MaxLength(100)]
        public string? PreferredPaymentMethod { get; set; }
    }
}
