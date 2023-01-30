using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface IOrderRepository
    {
        Order CreateOrder(Order order);
        Task<bool> Save();
        int CountOrders(int userId);
    }
}
