using System.ComponentModel.DataAnnotations;

namespace Shortify.Client.Data.ViewModels
{
    public class Confirm2FALoginVm
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Verification code is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be exactly 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must contain only digits.")]
        public string UserConfirmationCode { get; set; } = string.Empty;
    }
}
