using EcommerceAPI.Data;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repository
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly EcommerceContext _context;

        public ProductCategoryRepository(EcommerceContext context)
        {
            _context = context;
        }

        public int CountProductCategories()
        {
            int countProductCategory = _context.ProductCategories.Count();
            return countProductCategory;
        }

        public ProductCategory CreateProductCategory(ProductCategory productCategory)
        {
            _context.ProductCategories.Add(productCategory);
            return productCategory;
        }

        public async Task<ProductCategory> GetProductByCategory(int categoryId)
        {
            var productCategory = await _context.ProductCategories.Include(pc => pc.Products.OrderBy(p => p.ProductId)).FirstOrDefaultAsync(pc => pc.CategoryId == categoryId);

            return productCategory;
        }

        public async Task<ProductCategory> GetProductByCategory(int categoryId, int page, int pageSize)
        {
            var productCategory = await _context.ProductCategories.Include(pc => pc.Products.OrderBy(p => p.ProductId).Skip((page - 1) * pageSize).Take(pageSize)).FirstOrDefaultAsync(pc => pc.CategoryId == categoryId);

            return productCategory;
        }

        public async Task<ICollection<ProductCategory>> GetProductCategories()
        {
            return await _context.ProductCategories.ToListAsync();
        }

        public async Task<ProductCategory> GetProductCategory(int categoryId)
        {
            return await _context.ProductCategories.Where(c => c.CategoryId == categoryId).FirstOrDefaultAsync();
        }

        public bool IsProductCategoryExists(int categoryId)
        {
            return _context.ProductCategories.Any(e => e.CategoryId == categoryId);
        }

        public ProductCategory RemoveProductCategory(ProductCategory productCategory)
        {
            _context.ProductCategories.Remove(productCategory);
            return productCategory;
        }

        public async Task<bool> Save()
        {
            var saved = await _context.SaveChangesAsync();

            return saved > 0 ? true : false;
        }

        public ProductCategory UpdateProductCategory(ProductCategory productCategory)
        {
            _context.ProductCategories.Update(productCategory);
            return productCategory;
        }
        public bool IsProductCategoryActive(int id)
        {
            return _context.ProductCategories.Any(e => e.CategoryId == id && e.ActiveStatus == true);
        }
    }
}
