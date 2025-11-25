using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Product
{
    public class GenerateBarcodeDto
    {
        [Required]
        public long ProductId { get; set; }

        [Required]
        public string Format { get; set; } = "QR"; // QR, Barcode

        public int Width { get; set; } = 300;
        public int Height { get; set; } = 300;
    }
}
