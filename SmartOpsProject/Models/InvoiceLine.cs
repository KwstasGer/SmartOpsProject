using SmartOpsProject.Models; // για Item, Service
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartOps.Models
{
    public class InvoiceLine
    {
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        // ------------------- ΚΑΤΑΛΟΓΟΣ -------------------
        // Γραμμή μπορεί να είναι Είδος (Item) ή Υπηρεσία (Service)

        public int? ItemId { get; set; }          // ⭐ nullable
        public Item? Item { get; set; }

        public int? ServiceId { get; set; }       // ⭐ ΝΕΟ + nullable
        public Service? Service { get; set; }

        // ---------------- ΠΟΣΟΤΗΤΑ / ΤΙΜΕΣ ----------------

        [Range(0.0001, 999999)]
        public decimal Quantity { get; set; } = 1m;

        [Range(0, 999999)]
        public decimal UnitPrice { get; set; } = 0m;

        [Range(0, 1)]
        public decimal VatRate { get; set; } = 0.24m;   // 0.24 = 24%

        // ---------------- ΥΠΟΛΟΓΙΣΜΟΙ ----------------

        [NotMapped]
        public decimal Net => Math.Round(Quantity * UnitPrice, 2);

        [NotMapped]
        public decimal VatAmount => Math.Round(Net * VatRate, 2);

        [NotMapped]
        public decimal Gross => Math.Round(Net + VatAmount, 2);
    }
}
