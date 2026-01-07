using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Store
{
    public class UpdateCreditLimitDto
    {
        [Required]
        public long StoreId { get; set; }

        [Required]
        [Range(0, 999999999999.99)]
        public decimal NewCreditLimit { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
