using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface ICartRepository
    {
        Task<ICollection<Cart>> GetCarts();
        Task<ICollection<Cart>> GetCarts(int page, int pageSize);
        Task<Cart> GetCart(int cartId);
        Cart CreateProduct(Cart cart);
        Cart UpdateProduct(Cart cart);
        Cart RemoveProduct(Cart cart);
        Task<bool> Save();
        int CountCart(int userId);
    }
}
