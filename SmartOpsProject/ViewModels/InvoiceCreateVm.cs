using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;   // <-- ΝΕΟ
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartOpsProject.Models;

namespace SmartOps.ViewModels
{
    public class InvoiceCreateVm
    {
        [Display(Name = "Σειρά")]
        public string Series { get; set; } = "ΤΙΜ";

        [Display(Name = "Ημ/νία")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [Display(Name = "Πελάτης")]
        public int CustomerId { get; set; }

        [Display(Name = "Τρόπος Πληρωμής")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public List<InvoiceLineVm> Lines { get; set; } = new() { new InvoiceLineVm() };

        public IEnumerable<SelectListItem> Customers { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Items { get; set; } = Array.Empty<SelectListItem>();
    }

    public class InvoiceLineVm
    {
        [Display(Name = "Είδος")]
        public int ItemId { get; set; }

        [Display(Name = "Σχόλια")]
        public string? Description { get; set; }

        [Display(Name = "Ποσότητα")]
        public decimal Quantity { get; set; } = 1;

        [Display(Name = "Τιμή")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "ΦΠΑ %")]
        public decimal VatRate { get; set; } = 24;
    }
}

