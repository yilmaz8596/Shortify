
using System.ComponentModel.DataAnnotations;

namespace Shortify.Client.Data.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Full Name is required!")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required!")]
        [EmailAddress(ErrorMessage = "Invalid email address format!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required!")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long!")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required!")]
        [Compare("Password", ErrorMessage = "Passwords do not match!")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
