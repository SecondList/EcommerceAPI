using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.Dto;
using Microsoft.CodeAnalysis;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System;
using EcommerceAPI.Helper;
using System.IO;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductsController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(EcommerceContext context, IMapper mapper, IWebHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery(Name = "PageSize")] int pageSize = 10, [FromQuery(Name = "Page")] int page = 1)
        {
            var pageCount = Math.Ceiling(_context.Products.Count() / (float)pageSize);

            var products = await _context.Products.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var productsDto = _mapper.Map<List<ProductDetailDto>>(products);

            return Ok(new { resultCount = productsDto.Count, products = productsDto, pageSize = pageSize, page = page, pageCount = pageCount });
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
        [HttpPut("{productId}")]
        public async Task<IActionResult> PutProduct(int productId, [FromForm] ProductDetailDto productDto)
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

            var filePath = Path.Combine(_environment.WebRootPath, "products", "images", FileHelper.GetUniqueFileName(productDto.Image.FileName));

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await productDto.Image.CopyToAsync(new FileStream(filePath, FileMode.Create));

            productDto.ImagePath = filePath;

            var oldPath = await _context.Products.Where(p => p.ProductId == productId).Select(p => p.ImagePath).FirstOrDefaultAsync();

            if (oldPath != null)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.IO.File.Delete(oldPath);
            }

            var product = _mapper.Map<Product>(productDto);

            _context.Update(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated." });

        }

        // POST: api/Products/Register
        [HttpPost("Register")]
        public async Task<ActionResult<ProductDto>> PostProduct([FromForm] ProductCreateDto productDto)
        {
            if (!ProductCategoryValid(productDto.CategoryId))
            {
                return NotFound(new { message = "Unable to register a new product with the product category.", errors = new[] { "The product category is either not found or inactive." } });
            }

            var filePath = Path.Combine(_environment.WebRootPath, "products", "images", FileHelper.GetUniqueFileName(productDto.Image.FileName));

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await productDto.Image.CopyToAsync(new FileStream(filePath, FileMode.Create));

            productDto.ImagePath = filePath;

            var product = _mapper.Map<Product>(productDto);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }
            
            var oldPath = await _context.Products.Where(p => p.ProductId == productId).Select(p => p.ImagePath).FirstOrDefaultAsync();

            if (oldPath != null)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.IO.File.Delete(oldPath);
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
