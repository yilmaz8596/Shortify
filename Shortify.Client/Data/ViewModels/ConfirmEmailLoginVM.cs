using System.ComponentModel.DataAnnotations;

namespace Shortify.Client.Data.ViewModels
{
    public class ConfirmEmailLoginVM
    {
        [Required(ErrorMessage = "Email address is required!")]
        [EmailAddress(ErrorMessage = "Invalid email address format!")]
        public string Email { get; set; }
    }
}
