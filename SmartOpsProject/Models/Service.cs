using System.ComponentModel.DataAnnotations;

namespace SmartOpsProject.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Κωδικός Υπηρεσίας")]
        public string ServiceCode { get; set; }

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
        [Range(0, double.MaxValue)]
        public decimal? RetailPrice { get; set; }

        [Display(Name = "Τιμή Χονδρικής")]
        [Range(0, double.MaxValue)]
        public decimal? WholesalePrice { get; set; }
            }

}
