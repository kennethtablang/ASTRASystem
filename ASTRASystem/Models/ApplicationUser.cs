using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public long? DistributorId { get; set; }
        public long? WarehouseId { get; set; }

        public string? TwoFactorCodeHash { get; set; }

        public DateTime? TwoFactorCodeExpiry { get; set; }

        public int TwoFactorAttempts { get; set; } = 0;

        public bool IsApproved { get; set; } = false;

        public string? ApprovalMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string FullName
            => string.Join(" ", new[] { FirstName, MiddleName, LastName }
                                .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
