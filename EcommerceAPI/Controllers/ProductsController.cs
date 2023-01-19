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
using AutoMapper;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IMapper _mapper;

        public ProductsController(EcommerceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            var productsDto =  _mapper.Map<List<ProductDetailDto>>(products);

            return Ok(new { resultCount = productsDto.Count, products = productsDto });
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found.", errors = new[] { $"No product with id {id}" } });
            }

            var productDto = _mapper.Map<ProductDetailDto>(product);

            return Ok(new { resultCount = 1, products = productDto });
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{productId}")]
        public async Task<IActionResult> PutProduct(int productId, ProductDetailDto productDto)
        {
            if (productId != productDto.ProductId)
            {
                return BadRequest(new { message = "Fail to verify product id.", errors = new[] { "Product Id not match." } });
            }

            if (!ProductExists(productId))
            {
                return NotFound(new { message = "Product not found." });
            }

            if (!ProductCategoryValid(productDto.CategoryId))
            {
                return NotFound(new { message = "Unable to update product with the product category.", errors = new[] { "The product category is either not found or inactive." } });
            }

            var product = _mapper.Map<Product>(productDto);

            _context.Update(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated." });

        }

        // POST: api/Products/Register
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Register")]
        public async Task<ActionResult<ProductDto>> PostProduct(ProductCreateDto productDto)
        {
            if (!ProductCategoryValid(productDto.CategoryId))
            {
                return NotFound(new { message = "Unable to register a new product with the product category.", errors = new[] { "The product category is either not found or inactive." } });
            }

            var product = _mapper.Map<Product>(productDto);

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
                return NotFound(new { message = "Product not found." });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product removed." });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        private bool ProductCategoryValid(int id)
        {
            return _context.ProductCategories.Any(e => e.CategoryId == id && e.ActiveStatus == true);
        }
    }
}
