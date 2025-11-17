using Microsoft.EntityFrameworkCore;
using SmartOps.Data;
using SmartOps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartOps.Services
{
    public class InvoiceService
    {
        private readonly SmartOpsDbContext _db;

        public InvoiceService(SmartOpsDbContext db)
        {
            _db = db;
        }

        // --------------------- BASIC QUERIES ---------------------

        public async Task<Invoice?> GetByIdAsync(int id, int userId)
        {
            return await _db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Lines).ThenInclude(l => l.Item)
                .Where(i => i.UserId == userId && i.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Invoice>> GetAllByUserAsync(int userId)
        {
            return await _db.Invoices
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.IssueDate)
                .ThenByDescending(i => i.Id)
                .ToListAsync();
        }

        public async Task<int> GetNextNumberAsync(string series, int year, int userId)
        {
            // απλό: μέγιστος αριθμός ανά χρήστη/σειρά/έτος +1
            var max = await _db.Invoices
                .Where(i => i.UserId == userId && i.Series == series && i.Year == year)
                .MaxAsync(i => (int?)i.Number) ?? 0;

            return max + 1;
        }

        // --------------------- CREATE ---------------------

        public async Task<Invoice> CreateAsync(
            int userId,
            string series,
            DateTime issueDate,
            int customerId,
            PaymentMethod paymentMethod,
            IEnumerable<(int itemId, decimal qty, decimal unitPrice, decimal vatRate, string? note)> lines)
        {
            var inv = new Invoice
            {
                UserId = userId,
                Series = series,
                Number = await GetNextNumberAsync(series, issueDate.Year, userId),
                IssueDate = issueDate,
                Year = issueDate.Year,
                CustomerId = customerId,
                PaymentMethod = paymentMethod,
                Lines = lines.Select(l => new InvoiceLine
                {
                    ItemId = l.itemId,
                    Quantity = l.qty,
                    UnitPrice = l.unitPrice, // καθαρή τιμή
                    VatRate = l.vatRate,     // 0.24 για 24%
                }).ToList()
            };

            inv.RecalculateTotals();

            _db.Invoices.Add(inv);
            await _db.SaveChangesAsync();
            return inv;
        }

        // --------------------- UPDATE LINES ---------------------

        public async Task<bool> AddLineAsync(int invoiceId, int userId, int itemId, decimal qty, decimal unitPrice, decimal vatRate, string? note)
        {
            var inv = await _db.Invoices
                .Include(i => i.Lines)
                .Where(i => i.UserId == userId && i.Id == invoiceId)
                .FirstOrDefaultAsync();

            if (inv == null) return false;

            inv.Lines.Add(new InvoiceLine
            {
                ItemId = itemId,
                Quantity = qty,
                UnitPrice = unitPrice,
                VatRate = vatRate,
            });

            inv.RecalculateTotals();
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLineAsync(int lineId, int userId, decimal qty, decimal unitPrice, decimal vatRate, string? note)
        {
            var line = await _db.InvoiceLines
                .Include(l => l.Invoice)
                .Where(l => l.Id == lineId && l.Invoice.UserId == userId)
                .FirstOrDefaultAsync();

            if (line == null) return false;

            line.Quantity = qty;
            line.UnitPrice = unitPrice;
            line.VatRate = vatRate;

            line.Invoice.RecalculateTotals();
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveLineAsync(int lineId, int userId)
        {
            var line = await _db.InvoiceLines
                .Include(l => l.Invoice)
                .Where(l => l.Id == lineId && l.Invoice.UserId == userId)
                .FirstOrDefaultAsync();

            if (line == null) return false;

            var inv = line.Invoice;
            _db.InvoiceLines.Remove(line);
            inv.RecalculateTotals();

            await _db.SaveChangesAsync();
            return true;
        }

        // --------------------- UPDATE HEADER ---------------------

        public async Task<bool> UpdateHeaderAsync(int invoiceId, int userId, string series, DateTime issueDate, int customerId, PaymentMethod paymentMethod, string? comments)
        {
            var inv = await _db.Invoices
                .Where(i => i.Id == invoiceId && i.UserId == userId)
                .FirstOrDefaultAsync();

            if (inv == null) return false;

            // Αν αλλάξει έτος/σειρά, ανανέωσε Number (προαιρετικό)
            var changedSeriesOrYear = inv.Series != series || inv.Year != issueDate.Year;

            inv.Series = series;
            inv.IssueDate = issueDate;
            inv.Year = issueDate.Year;
            inv.CustomerId = customerId;
            inv.PaymentMethod = paymentMethod;

            if (changedSeriesOrYear)
            {
                inv.Number = await GetNextNumberAsync(inv.Series, inv.Year, userId);
            }

            await _db.SaveChangesAsync();
            return true;
        }

        // --------------------- PAYMENTS ---------------------

        public async Task<bool> SetPaidAmountAsync(int invoiceId, int userId, decimal paidAmount)
        {
            var inv = await _db.Invoices
                .Where(i => i.Id == invoiceId && i.UserId == userId)
                .FirstOrDefaultAsync();

            if (inv == null) return false;

            inv.PaidAmount = paidAmount;
            await _db.SaveChangesAsync();
            return true;
        }

        // --------------------- DELETE ---------------------

        public async Task<bool> DeleteAsync(int invoiceId, int userId)
        {
            var inv = await _db.Invoices
                .Where(i => i.Id == invoiceId && i.UserId == userId)
                .FirstOrDefaultAsync();

            if (inv == null) return false;

            _db.Invoices.Remove(inv);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
