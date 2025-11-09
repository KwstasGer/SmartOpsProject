namespace SmartOps.Services
{
    public interface IDashboardService
    {
        Task<List<(string CustomerName, decimal Amount)>> GetTopCustomersAsync(int userId, int top = 5);
        Task<decimal[]> GetSalesByMonthAsync(int userId, int year);
        Task<List<(string CustomerName, decimal OpenAmount)>> GetOpenBalancesAsync(int userId);
    }
}
