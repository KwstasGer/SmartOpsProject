using System.ComponentModel.DataAnnotations;

namespace SmartOpsProject.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(160)]
        public string Username { get; set; } = string.Empty; // Email ως username

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty; // Θα αποθηκεύεται ως hash
    }
}
