using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;


namespace Shortify.Client.Data.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Email address is required!")]
        [EmailAddress(ErrorMessage = "Invalid email address format!")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long!")]
        public required string Password { get; set; }

        // Added so the "Remember me" checkbox from the form binds to the model
        public bool RememberMe { get; set; }

        // Third-party login provider (e.g., Google, Facebook) 
        public IEnumerable<AuthenticationScheme> Schemes { get; set; }

    }
}
