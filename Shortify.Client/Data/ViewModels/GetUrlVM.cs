namespace Shortify.Client.Data.ViewModels
{
    public class GetUrlVM
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortenedUrl { get; set; } = string.Empty;
        public int ClickCount { get; set; }
        
        // Fix: Change from int? to string? to match AppUser.Id type
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public GetUserVM? User { get; set; } = new GetUserVM();
    }
}
