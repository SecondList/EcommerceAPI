using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface IProductRepository
    {
        Task<ICollection<Product>> GetProducts();
        Task<ICollection<Product>> GetProducts(int page, int pageSize);
        Task<Product> GetProduct(int productId);
        Product CreateProduct(Product product);
        Product UpdateProduct(Product product);
        Product RemoveProduct(Product product);
        Task<bool> Save();
        bool IsProductExists(int categoryId);
        int CountProducts();
    }
}
