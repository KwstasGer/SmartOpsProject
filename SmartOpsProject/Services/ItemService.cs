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

        // Λήψη όλων των αντικειμένων
        public async Task<List<Item>> GetAllAsync()
        {
            return await _context.Items.ToListAsync();
        }

        // Λήψη αντικειμένου βάσει ID
        public async Task<Item?> GetByIdAsync(int id)
        {
            return await _context.Items.FindAsync(id);
        }

        // Προσθήκη νέου αντικειμένου
        public async Task AddAsync(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
        }

        // Ενημέρωση αντικειμένου
        public async Task UpdateAsync(Item item)
        {
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
        }

        // Διαγραφή αντικειμένου βάσει ID
        public async Task DeleteAsync(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        // Έλεγχος αν υπάρχει αντικείμενο με συγκεκριμένο ID
        public bool Exists(int id)
        {
            return _context.Items.Any(i => i.Id == id);
        }
    }
}
