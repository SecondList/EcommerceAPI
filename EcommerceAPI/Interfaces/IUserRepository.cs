using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<ICollection<User>> GetUsers();
        Task<User> GetUserByEmail(string email);
        bool AddUser(User user);
        Task<bool> Save();

        void SaveToken(UserToken userToken);
        bool EmailUsed(string email);
    }
}
