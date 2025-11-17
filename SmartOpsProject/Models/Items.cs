using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using SmartOpsProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartOps.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Display(Name = "Κωδικός Προϊόντος")]
        [StringLength(32)]
        public string? ItemCode { get; set; }

        [Required(ErrorMessage = "Η Περιγραφή είναι υποχρεωτική.")]
        [Display(Name = "Περιγραφή")]
        [StringLength(450)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Η Μονάδα Μέτρησης είναι υποχρεωτική.")]
        [Display(Name = "Μονάδα Μέτρησης")]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Το ΦΠΑ είναι υποχρεωτικό.")]
        [Display(Name = "ΦΠΑ")]
        [Range(0, 100, ErrorMessage = "Το ΦΠΑ πρέπει να είναι 0–100.")]
        [Precision(5, 2)]
        public decimal VAT { get; set; }

        [Display(Name = "Τιμή Λιανικής")]
        [Range(0, 1_000_000)]
        [Precision(18, 2)]
        public decimal? RetailPrice { get; set; }

        [Display(Name = "Τιμή Χονδρικής")]
        [Range(0, 1_000_000)]
        [Precision(18, 2)]
        public decimal? WholesalePrice { get; set; }

        [Required]
        public int UserId { get; set; }

        [ValidateNever]
        public User? User { get; set; }
    }
}
