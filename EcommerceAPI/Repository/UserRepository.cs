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

        public async Task<User> GetUser(int userId)
        {
            return await _context.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<User> GetUserCart(int userId)
        {
            return await _context.Users
                                 .Include(u => u.Carts.OrderBy(c => c.CreatedAt))
                                     .ThenInclude(c => c.Product)
                                 .FirstOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task<User> GetUserOrder(int userId)
        {
            return await _context.Users
                                 .Include(u => u.Orders.OrderBy(o => o.OrderId))
                                     .ThenInclude(o => o.OrderDetails)
                                        .ThenInclude(od => od.Product)
                                 .Include(u => u.Orders)
                                     .ThenInclude(o => o.Payment)
                                 .Include(u => u.Orders)
                                     .ThenInclude(o => o.Shipment)
                                 .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<ICollection<User>> UserLogin()
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<User>> UserRegistration()
        {
            throw new NotImplementedException();
        }

        public User CreateUser(User user)
        {
            _context.Users.Add(user);
            return user;
            
        }

        public UserToken CreateToken(UserToken userToken)
        {
            _context.UserTokens.Add(userToken);
            return userToken;
        }

        public UserToken UpdateToken(UserToken userToken)
        {
            _context.UserTokens.Update(userToken);
            return userToken;
        }

        public async Task<bool> Save()
        {
            var saved = await _context.SaveChangesAsync();

            return saved > 0 ? true : false;
        }

        public async Task<UserToken> GetUserToken(string refreshToken)
        {
            return await _context.UserTokens.FirstOrDefaultAsync(ut => ut.Token == refreshToken);
        }

        public bool EmailUsed(string email)
        {
            return _context.Users.Any(user => user.Email == email);
        }
    }
}
