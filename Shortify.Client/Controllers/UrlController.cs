using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shortify.Client.Data.ViewModels;
using Shortify.Data.Models;
using Shortify.Data.Services;

namespace Shortify.Client.Controllers
{
    [Authorize] // Require authentication for all actions
    public class UrlController : Controller
    {
        private readonly IUrlService _urlService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public UrlController(IUrlService urlService, IMapper mapper, UserManager<AppUser> userManager)
        { 
            _urlService = urlService;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Get current logged-in user
            var currentUser = await _userManager.GetUserAsync(User);
            
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            // Get all URLs
            var allUrls = await _urlService.GetUrlsAsync();
            
            // Filter by current user (Admins see all, Users see only their own)
            List<Url> filteredUrls;
            if (User.IsInRole("Admin"))
            {
                filteredUrls = allUrls; // Admins see all URLs
            }
            else
            {
                filteredUrls = allUrls.Where(u => u?.UserId?.ToString() == currentUser.Id).ToList(); // Users see only their URLs
            }

            var mappedUrls = _mapper.Map<List<Url>, List<GetUrlVM>>(filteredUrls);

            return View(mappedUrls);
        }

        public IActionResult Create()
        {
            // Redirect to Home page where the URL creation form is
            return RedirectToAction("Index", "Home");
        }

        // POST action for modal form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            var url = await _urlService.GetByIdAsync(id);

            if (url == null)
            {
                return NotFound();
            }

            // Check if user owns this URL or is Admin
            if (url.UserId != currentUser.Id && !User.IsInRole("Admin"))
            {
                return Forbid(); // 403 Forbidden
            }

            await _urlService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
