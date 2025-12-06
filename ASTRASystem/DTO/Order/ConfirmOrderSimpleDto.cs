using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class ConfirmOrderSimpleDto
    {
        public string? Notes { get; set; }
    }

    public class SimpleNotesDto
    {
        public string? Notes { get; set; }
    }

    public class SimpleReasonDto
    {
        [Required]
        public string Reason { get; set; }
    }
}
