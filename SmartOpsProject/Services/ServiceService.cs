using Microsoft.EntityFrameworkCore;
using SmartOps.Data;
using SmartOps.Models;
using SmartOpsProject.Models;

namespace SmartOpsProject.Services
{
    public class ServiceService
    {
        private readonly SmartOpsDbContext _context;

        public ServiceService(SmartOpsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Service>> GetAllAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            return await _context.Services.FindAsync(id);
        }

        public async Task AddAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Service service)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }

        // ——— User-scoped βελτιώσεις ———

        public async Task<List<Service>> GetAllByUserAsync(int userId)
        {
            return await _context.Services
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Description)
                .ToListAsync();
        }

        public async Task<Service?> GetByIdForUserAsync(int id, int userId)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        }

        public async Task<bool> ExistsForUserAsync(int id, int userId)
        {
            return await _context.Services
                .AnyAsync(s => s.Id == id && s.UserId == userId);
        }

        public async Task DeleteAsync(Service service)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }
    }
}
