using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SmartOps.Models; // Customer ζει εδώ

namespace SmartOpsProject.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string Series { get; set; } = "ΤΙΜ";

        [Required]
        public int Number { get; set; }

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        public int Year { get; set; } = DateTime.Today.Year;

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public decimal TotalNet { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalGross { get; set; }
        public decimal PaidAmount { get; set; } = 0m;
        public bool IsPaid { get; set; } = false;

        [Required]
        public int UserId   { get; set; }

        [ValidateNever]
        public User? User { get; set; }

        public List<InvoiceItem> Items { get; set; } = new();
    }

    public enum PaymentMethod { Cash = 0, Card = 1, Bank = 2 }
}
