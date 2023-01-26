using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<ICollection<User>> GetUsers();
        Task<User> GetUser(int userId);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserCart(int userId);
        Task<User> GetUserOrder(int userId);
        User CreateUser(User user);
        UserToken CreateToken(UserToken userToken);
        UserToken UpdateToken(UserToken userToken);
        Task<bool> Save();
        Task<UserToken> GetUserToken(string refreshToken);
        bool EmailUsed(string email);
    }
}
