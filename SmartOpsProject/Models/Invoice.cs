using SmartOpsProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartOps.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string Series { get; set; } = "TIM";

        [Required]
        public int Number { get; set; }

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        public int Year { get; set; } = DateTime.Today.Year;

        
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

     
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        // ⭐ ΝΕΟ ΠΕΔΙΟ – Τύπος παραστατικού
        [Required, StringLength(20)]
        public string InvoiceType { get; set; } = "Items";   // "Items" or "Services"

        // --- Σύνολα ---
        public decimal TotalNet { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalGross { get; set; }

        public decimal PaidAmount { get; set; } = 0m;
        public bool IsPaid { get; set; } = false;

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        public List<InvoiceLine> Lines { get; set; } = new();

        public void RecalculateTotals()
        {
            decimal net = 0m, vat = 0m, gross = 0m;

            foreach (var l in Lines)
            {
                net += l.Net;
                vat += l.VatAmount;
                gross += l.Gross;
            }

            TotalNet = Math.Round(net, 2);
            TotalVat = Math.Round(vat, 2);
            TotalGross = Math.Round(gross, 2);
        }
    }

    // ⭐ Σωστό enum, μέσα στο namespace SmartOps.Models
    public enum PaymentMethod
    {
        Cash = 0,
        Card = 1,
        Bank = 2
    }
}
