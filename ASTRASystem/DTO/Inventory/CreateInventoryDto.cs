using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Inventory
{
    public class CreateInventoryDto
    {
        [Required]
        public long ProductId { get; set; }

        [Required]
        public long WarehouseId { get; set; }

        [Range(0, int.MaxValue)]
        public int InitialStock { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int MaxStock { get; set; } = 99999;
    }
}
