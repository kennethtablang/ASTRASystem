using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Store
{
    public class UpdateCreditLimitDto
    {
        [Required]
        public long StoreId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Credit limit cannot be negative")]
        public decimal NewCreditLimit { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
