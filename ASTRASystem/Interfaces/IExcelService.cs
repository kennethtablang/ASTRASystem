using ASTRASystem.Models;

namespace ASTRASystem.Interfaces
{
    public interface IExcelService
    {
        byte[] ExportOrdersToExcel(List<Order> orders);
        byte[] ExportPaymentsToExcel(List<Payment> payments);
        byte[] ExportStoresToExcel(List<Store> stores);
        byte[] ExportProductsToExcel(List<Product> products);
    }
}
