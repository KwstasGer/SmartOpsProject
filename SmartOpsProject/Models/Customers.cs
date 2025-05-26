using System.ComponentModel.DataAnnotations;

namespace SmartOps.Models
{
    public class Customers
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Κωδικός Πελάτη")]
        public string CustomerCode { get; set; }

        [Required]
        [Display(Name = "Ονοματεπώνυμο / Επωνυμία")]
        public string Name { get; set; }

        [Display(Name = "ΑΦΜ")]
        public string TaxIdentificationNumber { get; set; }

        [Required]
        [Display(Name = "Χώρα")]
        public string Country { get; set; }

        [Display(Name = "Διεύθυνση")]
        public string Address { get; set; }

        [Display(Name = "Πόλη")]
        public string City { get; set; }

        [Display(Name = "Τ.Κ.")]
        public string PostalCode { get; set; }

        [Required]
        [Display(Name = "Καθεστώς ΦΠΑ")]
        public string VatStatus { get; set; }

        [Required]
        [Display(Name = "Κατηγορία Πελάτη")]
        public string CustomerCategory { get; set; }
    }
}
