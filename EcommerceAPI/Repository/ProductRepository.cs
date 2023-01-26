using EcommerceAPI.Data;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcommerceContext _context;

        public ProductRepository(EcommerceContext context)
        {
            _context = context;
        }

        public int CountProducts()
        {
            int countProduct = _context.Products.Count();
            return countProduct;
        }

        public Product CreateProduct(Product product)
        {
            _context.Products.Add(product);
            return product;
        }

        public async Task<Product> GetProduct(int productId)
        {
            return await _context.Products.FindAsync(productId);
        }

        public async Task<ICollection<Product>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<ICollection<Product>> GetProducts(int page, int pageSize)
        {
            return await _context.Products.OrderBy(p => p.ProductId).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        public bool IsProductExists(int categoryId)
        {

            return _context.Products.Any(e => e.ProductId == categoryId);
        }

        public Product RemoveProduct(Product product)
        {
            _context.Products.Remove(product);
            return product;
        }

        public async Task<bool> Save()
        {
            var saved = await _context.SaveChangesAsync();

            return saved > 0 ? true : false;
        }

        public Product UpdateProduct(Product product)
        {
            _context.Update(product);
            return product;
        }
    }
}
