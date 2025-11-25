using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Common
{
    public class MarkNotificationReadDto
    {
        [Required]
        public long NotificationId { get; set; }
    }
}
