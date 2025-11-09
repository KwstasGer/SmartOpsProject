using SmartOps.Models;
using SmartOps.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartOps.Services
{
    public class CustomerService
    {
        private readonly SmartOpsDbContext _context;

        public CustomerService(SmartOpsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Customer customer)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }

        // 🔹 Βελτιώσεις για user-scoped queries

        public async Task<List<Customer>> GetAllByUserAsync(int userId)
        {
            return await _context.Customers
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdForUserAsync(int id, int userId)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<bool> ExistsForUserAsync(int id, int userId)
        {
            return await _context.Customers
                .AnyAsync(c => c.Id == id && c.UserId == userId);
        }
    }
}
