

using Shortify.Data.Models;

namespace Shortify.Data.Services
{
    public interface IUrlService
    {
        Task<List<Url>> GetUrlsAsync();
        Task<Url> GetByIdAsync(int id);
        Task<Url> CreateAsync(Url url);
        Task<Url> UpdateAsync(int id, Url url);
        Task DeleteAsync(int id);
    }
}
