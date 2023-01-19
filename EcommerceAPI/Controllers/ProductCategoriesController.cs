using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.Dto;
using Microsoft.CodeAnalysis;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IMapper _mapper;

        public ProductCategoriesController(EcommerceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/ProductCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCategoryDetailDto>>> GetProductCategories()
        {
            var productCategories = await _context.ProductCategories.ToListAsync();

            var productCategoriesDto = _mapper.Map<List<ProductCategoryDetailDto>>(productCategories);

            return Ok(new { resultCount = productCategories.Count, productCategories = productCategoriesDto });
        }

        // GET: api/ProductCategories/5
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<ProductCategoryDto>> GetProductCategory(int categoryId)
        {
            var productCategory = await _context.ProductCategories.FindAsync(categoryId);

            if (productCategory == null)
            {
                return NotFound(new { message = "Product category not found." });
            }

            var productCategoryDto = _mapper.Map<ProductCategoryDetailDto>(productCategory);

            return Ok(new { resultCount = 1, productCategories = productCategoryDto });
        }

        // PUT: api/ProductCategories/5
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> PutProductCategory(int categoryId, ProductCategoryDetailDto productCategoryModifyRequest)
        {
            if (categoryId != productCategoryModifyRequest.CategoryId)
            {
                return BadRequest(new { message = "Category Id unmatch." });
            }

            if (!ProductCategoryExists(categoryId))
            {
                return NotFound(new { message = "Product category not found." });
            }

            var productCategory = _mapper.Map<ProductCategory>(productCategoryModifyRequest);

            _context.Update(productCategory);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product category updated."});
        }

        // POST: api/ProductCategories/Register
        [HttpPost("Register")]
        public async Task<ActionResult<ProductCategoryDetailDto>> PostProductCategory(ProductCategoryCreateDto productCategoryModifyRequest)
        {
            var productCategory = _mapper.Map<ProductCategory>(productCategoryModifyRequest);

            _context.ProductCategories.Add(productCategory);
            await _context.SaveChangesAsync();

            var productCategoriesDto = _mapper.Map<ProductCategoryDetailDto>(productCategory);

            return CreatedAtAction("GetProductCategory", new { categoryId = productCategory.CategoryId }, productCategoriesDto);
        }

        // DELETE: api/ProductCategories/5
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteProductCategory(int categoryId)
        {
            var productCategory = await _context.ProductCategories.FindAsync(categoryId);

            if (productCategory == null)
            {
                return NotFound(new { message = "Product category not found." });
            }

            _context.ProductCategories.Remove(productCategory);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product category removed." });
        }

        // GET : api/ProductCategories/1/Products
        [HttpGet("{categoryId}/Products")]
        public async Task<IActionResult> GetProductInCategory(int categoryId)
        {
            if (!ProductCategoryExists(categoryId))
            {
                return NotFound(new { message = "Product category not found." });
            }

            var productCat = await _context.ProductCategories.Include(pc => pc.Products).FirstOrDefaultAsync(pc => pc.CategoryId == categoryId);

            var productCategoryDto = _mapper.Map<ProductCategoryDto>(productCat);

            return Ok(new { resultCount = productCategoryDto.Products.Count, productCategories = productCategoryDto });
        }

        private bool ProductCategoryExists(int categoryId)
        {
            return _context.ProductCategories.Any(e => e.CategoryId == categoryId);
        }
    }
}
