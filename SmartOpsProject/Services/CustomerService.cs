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

        public async Task<List<Customers>> GetAllAsync() =>
            await _context.Customers.ToListAsync();

        public async Task<Customers?> GetByIdAsync(int id) =>
            await _context.Customers.FindAsync(id);

        public async Task AddAsync(Customers client)
        {
            _context.Customers.Add(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customers client)
        {
            _context.Customers.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var client = await _context.Customers.FindAsync(id);
            if (client != null)
            {
                _context.Customers.Remove(client);
                await _context.SaveChangesAsync();
            }
        }

        internal async Task DeleteAsync(Customers customer)
        {

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            throw new NotImplementedException();
        }
    }
}
