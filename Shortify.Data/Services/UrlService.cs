

using Microsoft.EntityFrameworkCore;
using Shortify.Data.Models;

namespace Shortify.Data.Services
{
    public class UrlService : IUrlService
    {
        private readonly AppDbContext _context;

        public UrlService(AppDbContext context)
        {
            _context = context;
        }

        public async Task DeleteAsync(int id)
        {
            var url = await _context.Urls.FirstOrDefaultAsync(u => u.Id == id);

            if(url != null)
            {
                _context.Urls.Remove(url);
               await _context.SaveChangesAsync();
            }
        }

        public async Task<Url> GetByIdAsync(int id)
        {
            var url = await _context.Urls.Include(n => n.User).FirstOrDefaultAsync(n => n.Id == id); 

            if(url != null)
            {
                return url;
            }

            return null!;
        }

        public async Task<List<Url>> GetUrlsAsync()
        {
            var allUrls = await _context.Urls.Include(u => u.User).ToListAsync();
            return allUrls;
        }

        public async Task<Url> UpdateAsync(int id, Url url)
        {
            var urlToUpdate = await _context.Urls.FirstOrDefaultAsync(u => u.Id == id);

            if (urlToUpdate != null)
            {
                urlToUpdate.OriginalUrl = url.OriginalUrl;
                urlToUpdate.ShortenedUrl = url.ShortenedUrl;
                urlToUpdate.UpdatedAt = DateTime.UtcNow;
              await  _context.SaveChangesAsync();
            }

            return urlToUpdate;
        }
        public async Task<Url> CreateAsync(Url url)
        {
            await _context.Urls.AddAsync(url);
            await _context.SaveChangesAsync();
            return url;
        }
    }
}
