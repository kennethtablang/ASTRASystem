using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Inventory
{
    public class UpdateInventoryLevelsDto
    {
        [Required]
        public long InventoryId { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxStock { get; set; }
    }
}
