using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // για να μην γίνεται validate το navigation User

namespace SmartOpsProject.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        // 🔸 Υποχρεωτικό
        [Required(ErrorMessage = "Ο Κωδικός είναι υποχρεωτικός.")]
        [StringLength(20, ErrorMessage = "Ο Κωδικός μπορεί να έχει έως 20 χαρακτήρες.")]
        [Display(Name = "Κωδικός *")]
        public string SupplierCode { get; set; } = string.Empty;

        // 🔸 Υποχρεωτικό
        [Required(ErrorMessage = "Η Περιγραφή είναι υποχρεωτική.")]
        [StringLength(200, ErrorMessage = "Η Περιγραφή μπορεί να έχει έως 200 χαρακτήρες.")]
        [Display(Name = "Περιγραφή *")]
        public string Name { get; set; } = string.Empty;

        // ▫️ Προαιρετικά πεδία
        [RegularExpression(@"^$|^\d{9}$", ErrorMessage = "Το ΑΦΜ πρέπει να είναι είτε κενό είτε 9 ψηφία.")]
        [Display(Name = "ΑΦΜ")]
        [StringLength(20)]
        public string? TaxIdentificationNumber { get; set; }

        [Display(Name = "Χώρα")]
        [StringLength(100)]
        public string? Country { get; set; }

        [Display(Name = "Διεύθυνση")]
        [StringLength(200)]
        public string? Address { get; set; }

        [Display(Name = "Πόλη")]
        [StringLength(100)]
        public string? City { get; set; }

        [Display(Name = "Τ.Κ.")]
        [StringLength(20)]
        public string? PostalCode { get; set; }

        [Display(Name = "Κατηγορία")]
        [StringLength(50)]
        public string? SupplierCategory { get; set; } // Εσωτερικού, ΕΕ, Τρίτων Χωρών

        [Display(Name = "Καθεστώς ΦΠΑ")]
        [StringLength(50)]
        public string? VatStatus { get; set; } // Κανονικά, Μειωμένο, Απαλλάσσεται

        // 🔹 Per-user scoping (ορίζεται server-side στον controller)
        [Display(Name = "Χρήστης")]
        public int? UserId { get; set; }

        [ValidateNever]
        public User? User { get; set; }
    }
}
