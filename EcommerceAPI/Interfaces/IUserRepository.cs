using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<ICollection<User>> GetUsers();
        Task<User> GetUser(int userId);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserCart(int userId, int page, int pageSize);
        Task<User> GetUserOrder(int userId, int page, int pageSize);
        User CreateUser(User user);
        Task<User> UpdateUserRole(int userId, int roleId);
        UserToken CreateToken(UserToken userToken);
        UserToken UpdateToken(UserToken userToken);
        Task<bool> Save();
        Task<UserToken> GetUserToken(string refreshToken);
        bool EmailUsed(string email);
        bool IsUserExists(int userId);
    }
}
