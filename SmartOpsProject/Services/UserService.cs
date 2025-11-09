using SmartOpsProject.Models;
using SmartOps.Data;
using Microsoft.EntityFrameworkCore;
using SmartOps.Helpers;               // για το PasswordHelper (Identity hasher)
using System.Security.Cryptography;
using System.Text;

namespace SmartOps.Services
{
    public class UserService
    {
        private readonly SmartOpsDbContext _context;

        public UserService(SmartOpsDbContext context)
        {
            _context = context;
        }

        // === Helpers ===
        private static string Normalize(string? email)
            => (email ?? string.Empty).Trim().ToLowerInvariant();

        private static string Sha256(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // === Queries ===
        public Task<User?> GetByEmailAsync(string email)
        {
            var norm = Normalize(email);
            // Στο δικό σου μοντέλο δεν βλέπω Email field, άρα χρησιμοποιείς Username ως email.
            return _context.Users.FirstOrDefaultAsync(u => u.Username == norm);
        }

        // ΕΠΙΣΤΡΕΦΕΙ bool για να ξέρει ο caller αν δημιουργήθηκε
        public async Task<bool> RegisterAsync(string email, string password)
        {
            var norm = Normalize(email);

            var exists = await _context.Users.AnyAsync(u => u.Username == norm);
            if (exists) return false;

            var user = new User
            {
                Username = norm,                                   // κρατάμε normalized email ως username
                PasswordHash = PasswordHelper.HashPassword(password) // Identity hasher
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Login check με Identity hasher + συμβατότητα για ΠΑΛΙΑ SHA256
        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return false;

            // 1) Προσπάθησε με Identity hasher
            var (ok, newHash) = PasswordHelper.VerifyAndMaybeRehash(user.PasswordHash, password);
            if (ok)
            {
                if (newHash is not null)
                {
                    user.PasswordHash = newHash;
                    await UpdateAsync(user); // rehash σε νεότερη μορφή
                }
                return true;
            }

            // 2) Fallback για παλιούς χρήστες που είχαν SHA256 χωρίς salt
            //    (ΑΝ δεν έχεις παλιούς, μπορείς να διαγράψεις αυτό το block)
            if (user.PasswordHash == Sha256(password))
            {
                // Κάνε migration στον νέο hasher για τον ίδιο χρήστη
                user.PasswordHash = PasswordHelper.HashPassword(password);
                await UpdateAsync(user);
                return true;
            }

            return false;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
