using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Models;
using EcommerceAPI.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Repository;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductCategoriesController(IProductCategoryRepository productCategoryRepository, IProductRepository productRepository, IMapper mapper)
        {
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        // GET: api/ProductCategories
        [HttpGet]
        [Authorize(Roles = "Buyer,Admin")]
        public async Task<ActionResult<IEnumerable<ProductCategoryDetailDto>>> GetProductCategories()
        {
            var productCategories = await _productCategoryRepository.GetProductCategories();

            var productCategoriesDto = _mapper.Map<List<ProductCategoryDetailDto>>(productCategories);

            return Ok(new BaseResponse { ResultCount = productCategories.Count, Result = productCategoriesDto });
        }

        // GET: api/ProductCategories/5
        [HttpGet("{categoryId}")]
        [Authorize(Roles = "Buyer,Admin")]
        public async Task<ActionResult<ProductCategoryDto>> GetProductCategory(int categoryId)
        {
            var productCategory = await _productCategoryRepository.GetProductCategory(categoryId);

            if (productCategory == null)
            {
                return NotFound(new BaseResponse { Message = "Product category not found." });
            }

            var productCategoryDto = _mapper.Map<ProductCategoryDetailDto>(productCategory);

            return Ok(new BaseResponse { ResultCount = 1, Result = productCategoryDto });
        }

        // PUT: api/ProductCategories/5
        [HttpPut("{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProductCategory(int categoryId, ProductCategoryDetailDto productCategoryModifyRequest)
        {
            if (categoryId != productCategoryModifyRequest.CategoryId)
            {
                return BadRequest(new BaseResponse { Message = "Category Id unmatch." });
            }

            if (!_productCategoryRepository.IsProductCategoryExists(categoryId))
            {
                return NotFound(new BaseResponse { Message = "Product category not found." });
            }

            var productCategory = _mapper.Map<ProductCategory>(productCategoryModifyRequest);

            _productCategoryRepository.UpdateProductCategory(productCategory);
            await _productCategoryRepository.Save();

            return Ok(new BaseResponse { Message = "Product category updated." });
        }

        // POST: api/ProductCategories/Register
        [HttpPost("Register")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Buyer,Admin")]
        public async Task<IActionResult> GetProductInCategory([FromRoute] int categoryId, [FromQuery(Name = "PageSize")] int pageSize = 10, [FromQuery(Name = "Page")] int page = 1)
        {
            if (!_productCategoryRepository.IsProductCategoryExists(categoryId))
            {
                return NotFound(new BaseResponse { Message = "Product category not found." });
            }

            var pageCount = Math.Ceiling(_productRepository.CountProductsByCategory(categoryId) / (float)pageSize);

            var productCat = await _productCategoryRepository.GetProductByCategory(categoryId, page, pageSize);

            var productCategoryDto = _mapper.Map<ProductCategoryDto>(productCat);

            return Ok(new BaseResponse { ResultCount = productCategoryDto.Products.Count, Result = productCategoryDto, PageSize = pageSize, Page = page, PageCount = (int)pageCount });
        }
    }
}
