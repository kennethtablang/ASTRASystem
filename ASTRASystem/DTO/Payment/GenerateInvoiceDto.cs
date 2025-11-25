using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Payment
{
    public class GenerateInvoiceDto
    {
        [Required]
        public long OrderId { get; set; }

        public decimal? TaxRate { get; set; } = 0.12m;
    }
}
