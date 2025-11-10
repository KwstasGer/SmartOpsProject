using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartOps.Models;
using SmartOps.Data;

namespace SmartOps.Services
{
    public class ItemService
    {
        private readonly SmartOpsDbContext _context;

        public ItemService(SmartOpsDbContext context)
        {
            _context = context;
        }

        // -------- Basic --------
        public async Task<List<Item>> GetAllAsync()
            => await _context.Items.ToListAsync();

        public async Task<Item?> GetByIdAsync(int id)
            => await _context.Items.FindAsync(id);

        public async Task AddAsync(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
        }

        // ✅ ΡΗΤΗ ενημέρωση πεδίων + ImagePath (πιάνει και null)
        public async Task UpdateAsync(Item item)
        {
            // Αν είναι ήδη tracked, μην κάνεις Attach δεύτερη φορά.
            var entry = _context.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                _context.Attach(item);
                entry = _context.Entry(item);
            }

            // Δεν αλλάζουμε: ItemCode, UserId (αν δεν θέλεις να αλλάζουν)
            entry.Property(x => x.Description).IsModified = true;
            entry.Property(x => x.Unit).IsModified = true;
            entry.Property(x => x.VAT).IsModified = true;
            entry.Property(x => x.RetailPrice).IsModified = true;
            entry.Property(x => x.WholesalePrice).IsModified = true;


            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public bool Exists(int id) => _context.Items.Any(i => i.Id == id);

        // -------- User-scoped --------
        public async Task<List<Item>> GetAllByUserAsync(int userId)
        {
            return await _context.Items
                .Where(i => i.UserId == userId)
                .OrderBy(i => i.Description)
                .ToListAsync();
        }

        public async Task<Item?> GetByIdForUserAsync(int id, int userId)
        {
            return await _context.Items
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
        }

        public async Task<bool> ExistsForUserAsync(int id, int userId)
        {
            return await _context.Items
                .AnyAsync(i => i.Id == id && i.UserId == userId);
        }

        public async Task DeleteAsync(Item item)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
