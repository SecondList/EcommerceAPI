using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Models;
using EcommerceAPI.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using EcommerceAPI.Interfaces;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IMapper _mapper;

        public ProductCategoriesController(IProductCategoryRepository productCategoryRepository, IMapper mapper)
        {
            _productCategoryRepository = productCategoryRepository;
            _mapper = mapper;
        }

        // GET: api/ProductCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCategoryDetailDto>>> GetProductCategories()
        {
            var productCategories = await _productCategoryRepository.GetProductCategories();

            var productCategoriesDto = _mapper.Map<List<ProductCategoryDetailDto>>(productCategories);

            return Ok(new BaseResponse { ResultCount = productCategories.Count, Result = productCategoriesDto });
        }

        // GET: api/ProductCategories/5
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<ProductCategoryDto>> GetProductCategory(int categoryId)
        {
            var productCategory = await _productCategoryRepository.GetProductCategory(categoryId);

            if (productCategory == null)
            {
                return NotFound(new { message = "Product category not found." });
            }

            var productCategoryDto = _mapper.Map<ProductCategoryDetailDto>(productCategory);

            return Ok(new BaseResponse { ResultCount = 1, Result = productCategoryDto });
        }

        // PUT: api/ProductCategories/5
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> PutProductCategory(int categoryId, ProductCategoryDetailDto productCategoryModifyRequest)
        {
            if (categoryId != productCategoryModifyRequest.CategoryId)
            {
                return BadRequest(new { message = "Category Id unmatch." });
            }

            if (!_productCategoryRepository.IsProductCategoryExists(categoryId))
            {
                return NotFound(new { message = "Product category not found." });
            }

            var productCategory = _mapper.Map<ProductCategory>(productCategoryModifyRequest);

            _productCategoryRepository.UpdateProductCategory(productCategory);
            await _productCategoryRepository.Save();

            return Ok(new { message = "Product category updated."});
        }

        // POST: api/ProductCategories/Register
        [HttpPost("Register")]
        public async Task<ActionResult<ProductCategoryDetailDto>> PostProductCategory(ProductCategoryCreateDto productCategoryModifyRequest)
        {
            var productCategory = _mapper.Map<ProductCategory>(productCategoryModifyRequest);

            _productCategoryRepository.CreateProductCategory(productCategory);
            await _productCategoryRepository.Save();

            var productCategoriesDto = _mapper.Map<ProductCategoryDetailDto>(productCategory);

            return CreatedAtAction("GetProductCategory", new { categoryId = productCategory.CategoryId }, productCategoriesDto);
        }

        // DELETE: api/ProductCategories/5
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteProductCategory(int categoryId)
        {
            var productCategory = await _productCategoryRepository.GetProductCategory(categoryId);

            if (productCategory == null)
            {
                return NotFound(new BaseResponse { Message = "Product category not found." });
            }

            _productCategoryRepository.RemoveProductCategory(productCategory);
            await _productCategoryRepository.Save();

            return Ok(new BaseResponse { Message = "Product category removed." });
        }

        // GET : api/ProductCategories/1/Products
        [HttpGet("{categoryId}/Products")]
        public async Task<IActionResult> GetProductInCategory(int categoryId)
        {
            if (!_productCategoryRepository.IsProductCategoryExists(categoryId))
            {
                return NotFound(new { message = "Product category not found." });
            }

            var productCat = await _productCategoryRepository.GetProductByCategory(categoryId);

            var productCategoryDto = _mapper.Map<ProductCategoryDto>(productCat);

            return Ok(new BaseResponse { ResultCount = productCategoryDto.Products.Count, Result = productCategoryDto });
        }
    }
}
