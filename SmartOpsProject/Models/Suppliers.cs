using System.ComponentModel.DataAnnotations;

namespace SmartOpsProject.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Κωδικός Προμηθευτή *")]
        public string SupplierCode { get; set; }

        [Required]
        [Display(Name = "Περιγραφή *")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "ΑΦΜ *")]
        public string TaxIdentificationNumber { get; set; }

        [Display(Name = "Χώρα")]
        public string Country { get; set; }

        [Display(Name = "Διεύθυνση")]
        public string Address { get; set; }

        [Display(Name = "Πόλη")]
        public string City { get; set; }

        [Display(Name = "Τ.Κ.")]
        public string PostalCode { get; set; }

        [Required]
        [Display(Name = "Κατηγορία *")]
        public string SupplierCategory { get; set; } // Εσωτερικού, ΕΕ, Τρίτων Χωρών

        [Required]
        [Display(Name = "Καθεστώς ΦΠΑ *")]
        public string VatStatus { get; set; } // Κανονικά, Μειωμένο, Απαλλάσσεται
    }
}

