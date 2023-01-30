using EcommerceAPI.Data;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;

namespace EcommerceAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EcommerceContext _context;

        public OrderRepository(EcommerceContext context)
        {
            _context = context;
        }

        public int CountOrders(int userId)
        {
            int countOrder = _context.Orders.Where(o => o.UserId == userId).Count();
            return countOrder;
        }

        public Order CreateOrder(Order order)
        {
            _context.Orders.Add(order);
            return order;
        }

        public async Task<bool> Save()
        {
            var saved = await _context.SaveChangesAsync();

            return saved > 0 ? true : false;
        }
    }
}
