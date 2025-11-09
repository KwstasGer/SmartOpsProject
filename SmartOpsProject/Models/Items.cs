using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // για να μην γίνεται validate το navigation User
using SmartOpsProject.Models;                            // τύπος User

namespace SmartOps.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Κωδικός Είδους")]
        public string ItemCode { get; set; }

        [Required]
        [Display(Name = "Περιγραφή")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Μονάδα Μέτρησης")]
        public string Unit { get; set; }

        [Required]
        [Display(Name = "ΦΠΑ")]
        public decimal VAT { get; set; }

        [Display(Name = "Τιμή Λιανικής")]
        public decimal? RetailPrice { get; set; }

        [Display(Name = "Τιμή Χονδρικής")]
        public decimal? WholesalePrice { get; set; }

        [Display(Name = "Εικόνα Προϊόντος")]
        public string? ImagePath { get; set; }

        // 🔹 Προσθήκες για per-user scoping
        [Required]
        public int UserId { get; set; }

        [ValidateNever]
        public User? User { get; set; }
    }
}
