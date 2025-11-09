using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartOps.Data;
using SmartOpsProject.Models;

namespace SmartOpsProject.Services
{
    public class SupplierService
    {
        private readonly SmartOpsDbContext _context;

        public SupplierService(SmartOpsDbContext context)
        {
            _context = context;
        }

        // -------------------- Βασικές μέθοδοι --------------------

        public async Task<List<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers.ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task AddAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Supplier supplier)
        {
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
        }

        // -------------------- User-scoped μέθοδοι --------------------

        public async Task<List<Supplier>> GetAllByUserAsync(int userId)
        {
            return await _context.Suppliers
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdForUserAsync(int id, int userId)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        }

        public async Task<bool> ExistsForUserAsync(int id, int userId)
        {
            return await _context.Suppliers
                .AnyAsync(s => s.Id == id && s.UserId == userId);
        }
    }
}
