using ASTRASystem.Models;

namespace ASTRASystem.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateInvoicePdf(Invoice invoice);
        byte[] GenerateTripManifestPdf(Trip trip);
        byte[] GeneratePackingSlipPdf(Order order);
        byte[] GeneratePickListPdf(List<Order> orders);
    }
}
