
using Microsoft.EntityFrameworkCore;
using Shortify.Data.Models;


namespace Shortify.Data.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppUser>> GetUsersAsync()
        {
            var users = await _context.Users.Include(u => u.Urls).ToListAsync();
            return users;
        }

    }
}
