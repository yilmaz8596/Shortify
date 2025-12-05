using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shortify.Client.Data.ViewModels;
using Shortify.Data;
using Shortify.Data.Models;


namespace Shortify.Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var newUrl = new PostUrlVM();
            return View(newUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShortenUrl(PostUrlVM postUrlVM, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                // If called from modal (returnUrl present), redirect back
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    TempData["Error"] = "Please enter a valid URL.";
                    return LocalRedirect(returnUrl);
                }
                return View("Index", postUrlVM);
            }

            // Get current logged-in user (if authenticated)
            var currentUser = await _userManager.GetUserAsync(User);

            var newUrl = new Url()
            {
                OriginalUrl = postUrlVM.Url,
                ShortenedUrl = GenerateShortUrl(),
                ClickCount = 0,
                UserId = currentUser?.Id, // Set UserId if user is logged in, null otherwise
                CreatedAt = DateTime.UtcNow,
            };

            _context.Urls.Add(newUrl);  
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Your URL was shortened successfully to {newUrl.ShortenedUrl}!";

            // If returnUrl is provided (from modal), redirect there
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            
            return RedirectToAction("Index");
        }

        private string GenerateShortUrl()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
