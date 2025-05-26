using System.ComponentModel.DataAnnotations;

namespace SmartOpsProject.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        public string SupplierCode { get; set; }

        [Required]
        public string Name { get; set; }

        public string TaxIdentificationNumber { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        [Required]
        public string SupplierCategory { get; set; } // Εσωτερικού, ΕΕ, Τρίτων Χωρών

        [Required]
        public string VatStatus { get; set; } // Κανονικά, Μειωμένο, Απαλλάσσεται
    }
}

