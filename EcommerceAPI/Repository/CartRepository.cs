using EcommerceAPI.Data;
using EcommerceAPI.Dto;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Issuing;

namespace EcommerceAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly EcommerceContext _context;

        public CartRepository(EcommerceContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Cart>> ClearCart(int userId)
        {
            var carts = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();

            if (carts != null) carts.ForEach(c => RemoveCart(c));
            return carts;
        }

        public int CountCart(int userId)
        {
            throw new NotImplementedException();
        }

        public int CountCarts(int userId)
        {
            int countCart = _context.Carts.Where(c => c.UserId == userId).Count();
            return countCart;
        }

        public Cart CreateCart(Cart cart)
        {
            _context.Carts.Add(cart);
            return cart;
        }

        public async Task<Cart> GetCart(int userId)
        {
            return await _context.Carts.Where(c => c.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<ICollection<Cart>> GetCarts()
        {
            var carts = await _context.Carts.ToListAsync();
            return carts;
        }

        public Task<ICollection<Cart>> GetCarts(int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Cart>> GetCartsByProductIds(int userId, int[] productIds)
        {
            var carts = await _context.Carts.Include(c => c.Product).Where(c => c.UserId == userId && productIds.Contains(c.ProductId)).ToListAsync();

            return carts;
        }

        public bool IsProductFoundInCart(int userId, int productId)
        {
            return _context.Carts.Any(e => e.UserId == userId && e.ProductId == productId);
        }

        public Cart RemoveCart(Cart cart)
        {
            _context.Carts.Remove(cart);
            return cart;
        }

        public async Task<Cart> RemoveCart(int userId, int productId)
        {
            var cart = await _context.Carts.Where(c => c.UserId == userId && c.ProductId == productId).FirstOrDefaultAsync();

            if (cart != null) RemoveCart(cart);

            return cart;
        }

        public ICollection<Cart> RemoveRange(ICollection<Cart> carts)
        {
            _context.Carts.RemoveRange(carts);
            return carts;
        }

        public async Task<bool> Save()
        {
            var saved = await _context.SaveChangesAsync();

            return saved > 0 ? true : false;
        }

        public Cart UpdateCart(Cart cart)
        {
            _context.Carts.Update(cart);
            return cart;
        }
    }
}
