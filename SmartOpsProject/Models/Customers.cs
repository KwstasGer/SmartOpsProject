using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;            // BindNever
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;  // ValidateNever
using SmartOpsProject.Models;

namespace SmartOps.Models
{
    public class Customer
    {
        public int Id { get; set; }

        // Υπολογιζόμενος κωδικός από τη DB (computed).
        // Είναι null πριν το SaveChanges, γι’ αυτό ΔΕΝ είναι [Required] και είναι nullable.
        // Προαιρετικό στη φόρμα: ο χρήστης μπορεί να δώσει τιμή ή να αφήσει "*"
        // (αν είναι "*" ή κενό, στον controller το κάνουμε null για auto από DB)
        [Required(ErrorMessage = "Ο Κωδικός Πελάτη είναι υποχρεωτικός.")]
        [Display(Name = "Κωδικός Πελάτη")]
        public string? CustomerCode { get; set; }

        [Required(ErrorMessage = "Το Ονοματεπώνυμο / Επωνυμία είναι υποχρεωτικό.")]

        [Display(Name = "Ονοματεπώνυμο / Επωνυμία")]
        public string Name { get; set; } = string.Empty;

        [RegularExpression(@"^$|^\d{9}$", ErrorMessage = "Το ΑΦΜ πρέπει να είναι είτε κενό είτε 9 ψηφία.")]
        [Display(Name = "ΑΦΜ")]
        public string? TaxIdentificationNumber { get; set; }

        [Display(Name = "Χώρα")]
        public string? Country { get; set; } 

        [Display(Name = "Διεύθυνση")]
        public string? Address { get; set; }

        [Display(Name = "Πόλη")]
        public string? City { get; set; }

        [Display(Name = "Τ.Κ.")]
        public string? PostalCode { get; set; }

        [Required]
        [Display(Name = "Καθεστώς ΦΠΑ")]
        public string VatStatus { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Κατηγορία Πελάτη")]
        public string CustomerCategory { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [ValidateNever]
        public User? User { get; set; }
    }
}
