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

            return data.Select(x => (dict.GetValueOrDefault(x.CustomerId, $"Customer {x.CustomerId}"), x.Amount)).ToList();
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
                if (x.Month is >= 1 and <= 12)
                    result[x.Month - 1] = x.Amount;

            return result;
        }

        // Σημ.: εδώ υπολογίζουμε ανοικτό υπόλοιπο ως TotalGross - PaidAmount (αν έχεις τέτοιο πεδίο).
        public async Task<List<(string CustomerName, decimal OpenAmount)>> GetOpenBalancesAsync(int userId)
        {
            var rows = await _ctx.Invoices
                .AsNoTracking()
                .Where(i => i.UserId == userId && (i.TotalGross - i.PaidAmount) > 0m)
                .Select(i => new { i.CustomerId, Open = i.TotalGross - i.PaidAmount })
                .ToListAsync();

            var grouped = rows
                .GroupBy(x => x.CustomerId)
                .Select(g => new { CustomerId = g.Key, OpenAmount = g.Sum(z => z.Open) })
                .OrderByDescending(x => x.OpenAmount)
                .ToList();

            var ids = grouped.Select(x => x.CustomerId).ToList();
            var names = await _ctx.Customers
                .Where(c => ids.Contains(c.Id))
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            var dict = names.ToDictionary(x => x.Id, x => x.Name);

            return grouped
                .Select(x => (dict.GetValueOrDefault(x.CustomerId, $"Customer {x.CustomerId}"), x.OpenAmount))
                .ToList();
        }

    }
}
