using ASTRASystem.Models;

namespace ASTRASystem.Interfaces
{
    public interface IThermalReceiptService
    {
        string GenerateThermalReceipt(Order order, int paperWidth = 58);
        string GenerateSimplifiedReceipt(Order order);
        string GenerateFullReceipt(Order order);
        string GenerateBase64Receipt(Order order, int paperWidth = 58);
        byte[] GenerateReceiptBytes(Order order, int paperWidth = 58);
    }
}
