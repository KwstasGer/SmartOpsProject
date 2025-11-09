using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // για να μην γίνεται validate τα navigation properties
using SmartOps.Models; // Item ζει εδώ

namespace SmartOpsProject.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [ValidateNever]
        public Invoice Invoice { get; set; } = null!;

        [Required]
        public int ItemId { get; set; }

        [ValidateNever]
        public Item Item { get; set; } = null!;

        [Required, StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal VatRate { get; set; }

        public decimal LineNet { get; set; }
        public decimal LineVat { get; set; }
        public decimal LineGross { get; set; }
    }
}
