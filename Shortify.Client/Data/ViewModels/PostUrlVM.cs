

using System.ComponentModel.DataAnnotations;

namespace Shortify.Client.Data.ViewModels
{
    public class PostUrlVM
    {
        [Required(ErrorMessage = "Url is required!")]
        [RegularExpression(@"^https?://([\w-]+\.)+[\w-]+(/[^\s]*)?$", ErrorMessage = "Invalid Url format!")]
        public string Url { get; set; } = string.Empty;
    }
}
