using Microsoft.EntityFrameworkCore;
using SmartOps.Data;
using SmartOps.Models;
using SmartOpsProject.Models;
using System.Threading;

namespace SmartOpsProject.Services
{
    public class ServiceService
    {
        private readonly SmartOpsDbContext _context;

        public ServiceService(SmartOpsDbContext context)
        {
            _context = context;
        }

        // -------- Generic (μη scoped) --------
        public async Task<List<Service>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Services
                .AsNoTracking()
                .OrderBy(s => s.Description)
                .ToListAsync(ct);
        }

        public async Task<Service?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            // Για Details αρκεί AsNoTracking (γρηγορότερο)
            return await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task AddAsync(Service service, CancellationToken ct = default)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Service service, CancellationToken ct = default)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync(ct);
        }

        // Ενιαίο Delete (αν θέλεις και by-id helper, το κρατάμε ξεχωριστά πιο κάτω)
        public async Task DeleteAsync(Service service, CancellationToken ct = default)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync(ct);
        }

        // Προαιρετικό helper: διαγραφή by id
        public async Task<bool> DeleteByIdAsync(int id, CancellationToken ct = default)
        {
            var service = await _context.Services.FindAsync(new object?[] { id }, ct);
            if (service == null) return false;
            _context.Services.Remove(service);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // -------- User-scoped --------
        public async Task<List<Service>> GetAllByUserAsync(int userId, CancellationToken ct = default)
        {
            return await _context.Services
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Description)
                .ToListAsync(ct);
        }

        public async Task<Service?> GetByIdForUserAsync(int id, int userId, CancellationToken ct = default)
        {
            return await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, ct);
        }

        // Για Edit χρειαζόμαστε tracked entity
        public async Task<Service?> GetTrackedByIdForUserAsync(int id, int userId, CancellationToken ct = default)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, ct);
        }

        public async Task<bool> ExistsForUserAsync(int id, int userId, CancellationToken ct = default)
        {
            return await _context.Services
                .AsNoTracking()
                .AnyAsync(s => s.Id == id && s.UserId == userId, ct);
        }

        // -------- Helpers --------
        // Έλεγχος μοναδικότητας κωδικού ανά χρήστη (εξαιρεί το τρέχον id στο Edit)
        public async Task<bool> IsCodeTakenAsync(int userId, string code, int? excludeId = null, CancellationToken ct = default)
        {
            var q = _context.Services.AsNoTracking().Where(s => s.UserId == userId && s.ServiceCode == code);
            if (excludeId.HasValue)
                q = q.Where(s => s.Id != excludeId.Value);
            return await q.AnyAsync(ct);
        }
    }
}
