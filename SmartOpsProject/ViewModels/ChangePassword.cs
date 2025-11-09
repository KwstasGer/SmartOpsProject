using System.ComponentModel.DataAnnotations;

namespace SmartOps.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Το email είναι υποχρεωτικό.")]

        public string Email { get; set; }

        [Required(ErrorMessage = "Ο νέος κωδικός είναι υποχρεωτικός.")]
        [MinLength(6, ErrorMessage = "Ο κωδικός πρέπει να έχει τουλάχιστον 6 χαρακτήρες.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Η επιβεβαίωση κωδικού είναι υποχρεωτική.")]
        [Compare("NewPassword", ErrorMessage = "Οι κωδικοί δεν ταιριάζουν.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
