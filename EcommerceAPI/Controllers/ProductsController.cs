using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Models;
using EcommerceAPI.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using EcommerceAPI.Helper;
using EcommerceAPI.Interfaces;
using System.Net.Mime;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            var productCount = _productRepository.CountProducts();
            var pageCount = Math.Ceiling(productCount / (float)pageSize);

            var products = await _productRepository.GetProducts(page, pageSize);

            var productsDto = _mapper.Map<List<ProductDetailDto>>(products);

            return Ok(new BaseResponse { ResultCount = productsDto.Count, TotalCount = productCount, Result = productsDto, PageSize = pageSize, Page = page, PageCount = (int)pageCount });
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

            return Ok(new BaseResponse { ResultCount = 1, TotalCount = 1, Result = productDto });
        }

        // PUT: api/Products/5
        [HttpPut("{productId}")]

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromForm] ProductUpdateDto productDto)
        {
            if (productId != productDto.ProductId)
            {
                return BadRequest(new BaseResponse { Message = "Fail to verify product id.", Errors = new[] { "Product Id not match." } });
            }

            if (!_productRepository.IsProductExists(productId))
            {
                return NotFound(new BaseResponse { Message = "Product not found." });
            }

            if (!_productCategoryRepository.IsProductCategoryActive(productDto.CategoryId))
            {
                return NotFound(new BaseResponse { Message = "Unable to update product with the product category.", Errors = new[] { "The product category is either not found or inactive." } });
            }

            var product = _mapper.Map<Product>(productDto);

            _productRepository.UpdateProduct(product);
            await _productRepository.Save();

            return Ok(new BaseResponse { Message = "Product updated." });
        }

        // PUT: api/Products/5/Image
        [HttpPut("{productId}/Image")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UpdateProductImage(int productId, [FromForm] ProductImageDto productImageDto)
        {
            if (productId != productImageDto.ProductId)
            {
                return BadRequest(new BaseResponse { Message = "Fail to verify product id.", Errors = new[] { "Product Id not match." } });
            }

            if (!_productRepository.IsProductExists(productId))
            {
                return NotFound(new BaseResponse { Message = "Product not found." });
            }

            var filePath = await UploadImage(productImageDto.Image);

            productImageDto.ImagePath = filePath;

            await DeleteOldImage(productId);

            await _productRepository.UpdateProductImage(productId, filePath);
            await _productRepository.Save();

            return Ok(new BaseResponse { Message = "Product updated." });
        }

        // POST: api/Products/Register
        [HttpPost("Register")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var product = await _productRepository.GetProduct(productId);

            if (product == null)
            {
                return NotFound(new BaseResponse { Message = "Product not found." });
            }

            await DeleteOldImage(productId);

            _productRepository.RemoveProduct(product);
            await _productRepository.Save();

            return Ok(new BaseResponse { Message = "Product removed." });
        }

        // GET: api/Products/Image/
        [HttpGet("Image/{imageFile}")]
        public async Task<IActionResult> GetProductImage(string imageFile)
        {
            string imagePath = Path.Combine(_environment.WebRootPath, "products", "images", imageFile);
            string contentType = "image/jpeg";

            try
            {
                var fileByte = await System.IO.File.ReadAllBytesAsync(imagePath);
                var fileContentResult = new FileContentResult(fileByte, contentType);

                return fileContentResult;
            }
            catch (Exception ex)
            {
                return NotFound("");
            }
        }

        private async Task<bool> DeleteOldImage(int productId)
        {
            var existingProduct = await _productRepository.GetProduct(productId);

            if (existingProduct.ImagePath != "")
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
