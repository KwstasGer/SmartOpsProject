using System.ComponentModel.DataAnnotations;

namespace SmartOps.ViewModels
{
    public class LoginViewModel
    {

        [DataType(DataType.Date)]
        [Display(Name = "Ημερομηνία")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


    }
}