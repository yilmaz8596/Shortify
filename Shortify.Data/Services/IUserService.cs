

using Shortify.Data.Models;

namespace Shortify.Data.Services
{
     public interface IUserService
    {
        Task<List<AppUser>> GetUsersAsync();

    }
}
