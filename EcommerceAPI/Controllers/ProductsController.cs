using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Identity;
using EcommerceAPI.Dto;
using Microsoft.CodeAnalysis;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly EcommerceContext _context;

        public ProductsController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(new { resultCount = products.Count, products = products });
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found.", errors = new[] { "No product with id " + id } });
            }

            return Ok(new { resultCount = 1, products = product });
        }

        // GET: api/Products/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductByCategory(int categoryId)
        {
            var products = await _context.Products.Where(p => p.CategoryId == categoryId).ToListAsync();

            // CategoryId is foreign key
            if (products == null)
            {
                return NotFound(new { message = "Fail to retrieve products by category" });
            }

            return Ok(new { resultCount = products.Count, products = products });
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{productId}")]
        public async Task<IActionResult> PutProduct(int productId, ProductDto productDto)
        {
            if (productId != productDto.ProductId)
            {
                return BadRequest(new { message = "Product Id not match." });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            if (!ProductCategoryExists(productDto.CategoryId))
            {
                return NotFound(new { message = "Product category not found." });
            }

            product.ProductId = productDto.ProductId;
            product.Title = productDto.Title;
            product.Description = productDto.Description;
            product.CategoryId = productDto.CategoryId;
            product.Price = productDto.Price;
            product.ImageUrl = productDto.ImageUrl;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product category updated." });

        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("register")]
        public async Task<ActionResult<Product>> PostProduct(ProductDto productDto)
        {

            if (!ProductCategoryExists(productDto.CategoryId))
            {
                return NotFound(new { message = "Product category not found." });
            }

            var product = new Product
            {
                Title = productDto.Title,
                Description = productDto.Description,
                CategoryId = productDto.CategoryId,
                Price = productDto.Price,
                ImageUrl = productDto.ImageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        private bool ProductCategoryExists(int id)
        {
            return _context.ProductCategories.Any(e => e.CategoryId == id);
        }
    }
}
