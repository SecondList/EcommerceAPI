using EcommerceAPI.Data;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly EcommerceContext _context;

        public UserRepository(EcommerceContext context)
        {
            _context = context;
        }

        public async Task<ICollection<User>> GetUsers()
        {
            return await _context.Users.OrderBy(u => u.UserId).ToListAsync();
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<ICollection<User>> UserLogin()
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<User>> UserRegistration()
        {
            throw new NotImplementedException();
        }

        public bool AddUser(User user)
        {
            _context.Users.Add(user);
            boolResponse = Save();

            return boolResponse;
            
        }

        public async Task<bool> Save()
        {
            var saved = await _context.SaveChangesAsync();

            return saved > 0 ? true : false;
        }

        public bool EmailUsed(string email)
        {
            return _context.Users.Any(user => user.Email == email);
        }
    }
}
