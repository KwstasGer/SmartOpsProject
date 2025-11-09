using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartOps.Data;
using SmartOpsProject.Models;

namespace SmartOps.Services
{
    public class InvoiceService
    {
        private readonly SmartOpsDbContext _context;
        public InvoiceService(SmartOpsDbContext context) => _context = context;

        public async Task<int> GetNextNumberAsync(int userId, string series, int year)
        {
            var max = await _context.Invoices
                .Where(i => i.UserId == userId && i.Series == series && i.Year == year)
                .Select(i => (int?)i.Number)
                .MaxAsync();
            return (max ?? 0) + 1;
        }

        public async Task AddAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
        }

        // ✅ ΒΑΣΙΚΗ ΜΕΘΟΔΟΣ (όπως την έχεις)
        public async Task<Invoice?> GetByIdWithItemsAsync(int id, int userId)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Include(i => i.Items).ThenInclude(li => li.Item)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
        }

        // ✅ ALIAS για παλιές κλήσεις που ζητούν "...ForUser..."
        public Task<Invoice?> GetByIdWithItemsForUserAsync(int id, int userId)
        {
            return GetByIdWithItemsAsync(id, userId);
        }

        public async Task<List<Invoice>> GetAllByUserAsync(int userId)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .Include(i => i.Customer)
                .OrderByDescending(i => i.IssueDate)
                .ThenByDescending(i => i.Number)
                .ToListAsync();
        }

    }
}
