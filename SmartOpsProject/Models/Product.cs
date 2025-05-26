using System.ComponentModel.DataAnnotations;

namespace SmartOps.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Κωδικός Προϊόντος")]
        public string ProductCode { get; set; }

        [Required]
        [Display(Name = "Περιγραφή")]
        public string Description { get; set; }

        [Display(Name = "Τιμή Λιανικής")]
        public decimal? RetailPrice { get; set; } 

        [Display(Name = "Τιμή Χονδρικής")]
        public decimal? WholesalePrice { get; set; } 

        [Required]
        [Display(Name = "Μονάδα Μέτρησης")]
        public string Unit { get; set; }

        [Required]
        [Display(Name = "ΦΠΑ")]
        public decimal VAT { get; set; }

        [Display(Name = "Εικόνα Προϊόντος")]
        public string? ImagePath { get; set; }
    }
}
