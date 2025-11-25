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

        [MaxLength(200)]
        public string? Barangay { get; set; }

        [MaxLength(200)]
        public string? City { get; set; }

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
