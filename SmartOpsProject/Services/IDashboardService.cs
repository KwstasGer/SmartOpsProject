using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartOps.Services
{
    public interface IDashboardService
    {
        // Top πελάτες (ποσό πωλήσεων)
        Task<List<(string CustomerName, decimal Amount)>> GetTopCustomersAsync(int userId, int top = 5);

        // Πωλήσεις ανά μήνα (ποσά)
        Task<decimal[]> GetSalesByMonthAsync(int userId, int year);

        // ⭐ ΝΕΟ: Top είδη σε ποσότητα
        Task<List<TopItemRow>> GetTopItemsAsync(int userId, int year, int top = 5);
    }

    // DTO για τα Top Είδη
    public class TopItemRow
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; } = "";
        public string ItemDescription { get; set; } = "";
        public decimal Quantity { get; set; }
    }
}
