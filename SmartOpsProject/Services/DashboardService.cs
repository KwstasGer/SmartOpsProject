using Microsoft.EntityFrameworkCore;
using SmartOps.Data;

namespace SmartOps.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly SmartOpsDbContext _ctx;
        public DashboardService(SmartOpsDbContext ctx) { _ctx = ctx; }

        public async Task<List<(string CustomerName, decimal Amount)>> GetTopCustomersAsync(int userId, int top = 5)
        {
            var data = await _ctx.Invoices
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .GroupBy(i => i.CustomerId)
                .Select(g => new { CustomerId = g.Key, Amount = g.Sum(x => x.TotalGross) })
                .OrderByDescending(x => x.Amount)
                .Take(top)
                .ToListAsync();

            var ids = data.Select(x => x.CustomerId).ToList();
            var names = await _ctx.Customers
                .Where(c => ids.Contains(c.Id))
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            var dict = names.ToDictionary(x => x.Id, x => x.Name);

            return data
                .Select(x => (dict.GetValueOrDefault(x.CustomerId, $"Customer {x.CustomerId}"), x.Amount))
                .ToList();
        }

        public async Task<decimal[]> GetSalesByMonthAsync(int userId, int year)
        {
            var perMonth = await _ctx.Invoices
                .AsNoTracking()
                .Where(i => i.UserId == userId && i.Year == year)
                .GroupBy(i => i.IssueDate.Month)
                .Select(g => new { Month = g.Key, Amount = g.Sum(x => x.TotalGross) })
                .ToListAsync();

            var result = new decimal[12];
            foreach (var x in perMonth)
            {
                if (x.Month is >= 1 and <= 12)
                    result[x.Month - 1] = x.Amount;
            }

            return result;
        }

        // ⭐ ΝΕΟ: Top είδη σε ποσότητα
        public async Task<List<TopItemRow>> GetTopItemsAsync(int userId, int year, int top = 5)
        {
            return await _ctx.InvoiceLines
                .AsNoTracking()
                .Include(l => l.Invoice)
                .Include(l => l.Item)
                .Where(l => l.Invoice.UserId == userId
                            && l.Invoice.IssueDate.Year == year
                            && l.ItemId != null)              // μόνο Είδη, όχι Υπηρεσίες
                .GroupBy(l => new { l.ItemId, l.Item.ItemCode, l.Item.Description })
                .Select(g => new TopItemRow
                {
                    ItemId = g.Key.ItemId!.Value,
                    ItemCode = g.Key.ItemCode,
                    ItemDescription = g.Key.Description,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(top)
                .ToListAsync();
        }
    }
}
