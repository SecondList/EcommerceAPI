using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface IProductCategoryRepository
    {
        Task<ICollection<ProductCategory>> GetProductCategories();
        Task<ProductCategory> GetProductCategory(int categoryId);
        Task<ProductCategory> GetProductByCategory(int categoryId);
        ProductCategory CreateProductCategory(ProductCategory productCategory);
        ProductCategory UpdateProductCategory(ProductCategory productCategory);
        ProductCategory RemoveProductCategory(ProductCategory productCategory);
        Task<bool> Save();
        bool IsProductCategoryExists(int categoryId);
        bool IsProductCategoryActive(int categoryId);
    }
}
