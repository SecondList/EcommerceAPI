using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Models;
using EcommerceAPI.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using EcommerceAPI.Helper;
using EcommerceAPI.Interfaces;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(IProductRepository productRepository, IProductCategoryRepository productCategoryRepository, IMapper mapper, IWebHostEnvironment environment)
        {
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _mapper = mapper;
            _environment = environment;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery(Name = "PageSize")] int pageSize = 10, [FromQuery(Name = "Page")] int page = 1)
        {
            var pageCount = Math.Ceiling(_productRepository.CountProducts() / (float)pageSize);

            var products = await _productRepository.GetProducts(page, pageSize);

            var productsDto = _mapper.Map<List<ProductDetailDto>>(products);

            return Ok(new BaseResponse { ResultCount = productsDto.Count, Result = productsDto, PageSize = pageSize, Page = page, PageCount = (int)pageCount });
        }

        // GET: api/Products/5
        [HttpGet("{productId}")]
        public async Task<ActionResult<Product>> GetProduct(int productId)
        {
            var product = await _productRepository.GetProduct(productId);

            if (product == null)
            {
                return NotFound(new BaseResponse { Message = "Product not found.", Errors = new[] { $"No product with id {productId}" } });
            }

            var productDto = _mapper.Map<ProductDetailDto>(product);

            return Ok(new { resultCount = 1, products = productDto });
        }

        // PUT: api/Products/5
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromForm] ProductDetailDto productDto)
        {
            if (productId != productDto.ProductId)
            {
                return BadRequest(new { message = "Fail to verify product id.", errors = new[] { "Product Id not match." } });
            }

            if (!_productRepository.IsProductExists(productId))
            {
                return NotFound(new { message = "Product not found." });
            }

            if (!_productCategoryRepository.IsProductCategoryActive(productDto.CategoryId))
            {
                return NotFound(new BaseResponse { Message = "Unable to update product with the product category.", Errors = new[] { "The product category is either not found or inactive." } });
            }

            var filePath = await UploadImage(productDto.Image);

            productDto.ImagePath = filePath;

            await DeleteOldImage(productId);

            var product = _mapper.Map<Product>(productDto);

            _productRepository.UpdateProduct(product);
            await _productRepository.Save();

            return Ok(new BaseResponse { Message = "Product updated." });

        }

        // POST: api/Products/Register
        [HttpPost("Register")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] ProductCreateDto productDto)
        {
            if (!_productCategoryRepository.IsProductCategoryActive(productDto.CategoryId))
            {
                return NotFound(new BaseResponse { Message = "Unable to register a new product with the product category.", Errors = new[] { "The product category is either not found or inactive." } });
            }

            var filePath = await UploadImage(productDto.Image);

            productDto.ImagePath = filePath;

            var product = _mapper.Map<Product>(productDto);

            _productRepository.CreateProduct(product);
            await _productRepository.Save();

            return CreatedAtAction("GetProduct", new { productId = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var product = await _productRepository.GetProduct(productId);

            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            await DeleteOldImage(productId);

            _productRepository.RemoveProduct(product);
            await _productRepository.Save();

            return Ok(new { message = "Product removed." });
        }

        private async Task<bool> DeleteOldImage(int productId)
        {
            var existingProduct = await _productRepository.GetProduct(productId);

            if (existingProduct.ImagePath != null)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.IO.File.Delete(existingProduct.ImagePath);
            }

            return true;
        }

        private async Task<string> UploadImage(IFormFile image)
        {
            var filePath = Path.Combine(_environment.WebRootPath, "products", "images", FileHelper.GetUniqueFileName(image.FileName));

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await image.CopyToAsync(new FileStream(filePath, FileMode.Create));

            return filePath;
        }
    }
}
