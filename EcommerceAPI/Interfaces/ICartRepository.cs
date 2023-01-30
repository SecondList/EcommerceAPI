using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface ICartRepository
    {
        Task<ICollection<Cart>> GetCarts();
        Task<ICollection<Cart>> GetCarts(int page, int pageSize);
        Task<ICollection<Cart>> GetCartsByProductIds(int userId, int[] productIds);
        Task<Cart> GetCart(int userId);
        Cart CreateCart(Cart cart);
        Cart UpdateCart(Cart cart);
        Cart RemoveCart(Cart cart);
        Task<Cart> RemoveCart(int userId, int productId);
        Task<ICollection<Cart>> ClearCart(int userId);
        ICollection<Cart> RemoveRange(ICollection<Cart> carts);
        Task<bool> Save();
        bool IsProductFoundInCart(int userId, int productId);
        int CountCarts(int userId);
    }
}
