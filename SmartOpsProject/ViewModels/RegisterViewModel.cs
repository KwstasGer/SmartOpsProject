using System.ComponentModel.DataAnnotations;

namespace SmartOps.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Ο κωδικός πρέπει να έχει τουλάχιστον 6 χαρακτήρες.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Οι κωδικοί δεν ταιριάζουν.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
