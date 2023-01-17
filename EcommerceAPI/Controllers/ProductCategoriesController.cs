using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.Dto;
using Microsoft.CodeAnalysis;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly EcommerceContext _context;

        public ProductCategoriesController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/ProductCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> GetProductCategories()
        {
            var productCategories = await _context.ProductCategories.ToListAsync();

            return Ok(new { resultCount = productCategories.Count, productCategories = productCategories });
        }

        // GET: api/ProductCategories/5
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<ProductCategory>> GetProductCategory(int categoryId)
        {
            var productCategory = await _context.ProductCategories.FindAsync(categoryId);

            if (productCategory == null)
            {
                return NotFound(new { message = "Product category not found." });
            }

            return Ok(new { resultCount = 1, productCategories = productCategory });
        }

        // PUT: api/ProductCategories/5
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> PutProductCategory(int categoryId, ProductCategoryDto productCategoryModifyRequest)
        {
            if (categoryId != productCategoryModifyRequest.CategoryId)
            {
                return BadRequest(new { message = "Category Id unmatch." });
            }

            var productCategory = await _context.ProductCategories.FirstOrDefaultAsync(cat => cat.CategoryId == categoryId);

            if (productCategory == null)
            {
                return NotFound(new { message = "Product category not found." });
            }

            productCategory.CategoryName = productCategoryModifyRequest.CategoryName;
            productCategory.ActiveStatus = productCategoryModifyRequest.ActiveStatus;
            productCategory.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product category updated." });
        }

        // POST: api/ProductCategories
        [HttpPost("register")]
        public async Task<ActionResult<ProductCategory>> PostProductCategory(ProductCategoryDto productCategoryModifyRequest)
        {
            var productCategory = new ProductCategory
            {
                CategoryName = productCategoryModifyRequest.CategoryName,
                ActiveStatus = productCategoryModifyRequest.ActiveStatus,
                CreatedAt = DateTime.Now
            };

            _context.ProductCategories.Add(productCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductCategory", new { categoryId = productCategory.CategoryId }, productCategory);
        }

        // DELETE: api/ProductCategories/5
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteProductCategory(int categoryId)
        {
            var productCategory = await _context.ProductCategories.FindAsync(categoryId);
            if (productCategory == null)
            {
                return NotFound();
            }

            _context.ProductCategories.Remove(productCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductCategoryExists(int categoryId)
        {
            return _context.ProductCategories.Any(e => e.CategoryId == categoryId);
        }
    }
}
