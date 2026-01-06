using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Store
{
    public class UpdateStoreDto
    {
        [Required]
        public long Id { get; set; }

        [Required]
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

        [Phone]
        [MaxLength(100)]
        public string? Phone { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CreditLimit { get; set; }

        [MaxLength(100)]
        public string? PreferredPaymentMethod { get; set; }
    }
}
