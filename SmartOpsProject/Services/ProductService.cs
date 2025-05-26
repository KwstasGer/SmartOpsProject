using System.Collections.Generic;
using System.Linq; // ✅ Required for .Any()
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartOps.Models;
using SmartOps.Data;

namespace SmartOps.Services
{
    public class ProductService
    {
        private readonly SmartOpsDbContext _context;

        public ProductService(SmartOpsDbContext context)
        {
            _context = context;
        }

        // Λήψη όλων των προϊόντων
        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Product.ToListAsync();
        }

        // Λήψη ενός προϊόντος βάσει ID
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Product.FindAsync(id);
        }

        // Προσθήκη νέου προϊόντος
        public async Task AddAsync(Product product)
        {
            _context.Product.Add(product);
            await _context.SaveChangesAsync();
        }

        // Ενημέρωση προϊόντος
        public async Task UpdateAsync(Product product)
        {
            _context.Product.Update(product);
            await _context.SaveChangesAsync();
        }

        // Διαγραφή προϊόντος βάσει ID
        public async Task DeleteAsync(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        // Έλεγχος αν υπάρχει προϊόν με συγκεκριμένο ID
        public bool Exists(int id)
        {
            return _context.Product.Any(p => p.Id == id);
        }
    }
}
