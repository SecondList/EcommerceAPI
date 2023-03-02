using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.Dto;
using Microsoft.CodeAnalysis;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Services.User;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CartsController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public CartsController(ICartRepository cartRepository, IProductRepository productRepository, IMapper mapper, IUserService userService)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _userService = userService;
        }

        // GET: api/Carts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartDetailDto>>> GetCarts()
        {
            var carts = await _cartRepository.GetCarts();
            var cartsDto = _mapper.Map<List<CartDetailDto>>(carts);

            return Ok(new BaseResponse { ResultCount = cartsDto.Count, Result = cartsDto });
        }

        // PUT: api/Carts/UpdateOrderQty
        [HttpPut("UpdateOrderQty")]
        public async Task<IActionResult> UpdateOrderQty([FromBody] CartDetailDto cartDto)
        {
            // Verify product
            if (!_productRepository.IsProductExists(cartDto.ProductId))
            {
                return NotFound(new BaseResponse { Message = "Product not found.", Errors = new[] { $"Product ({cartDto.ProductId}) doesn't exists." } });
            }

            // Verify cart
            if (!_cartRepository.IsProductFoundInCart(_userService.GetUserId(), cartDto.ProductId))
            {
                return NotFound(new BaseResponse { Message = "Product not found in cart.", Errors = new[] { $"Product ({cartDto.ProductId}) is not found in user's ({_userService.GetUserId()}) cart." } });
            }

            var cart = _mapper.Map<CartDetailDto, Cart>(cartDto, opt =>
            {
                opt.AfterMap((cartDto, cart) =>
                {
                    cart.UserId = _userService.GetUserId();
                });
            });

            _cartRepository.UpdateCart(cart);
            await _cartRepository.Save();

            return Ok(new BaseResponse { Message = "Order quantity updated.", Result = cartDto });
        }

        // POST: api/Carts/AddItem
        [HttpPost("AddItem")]
        public async Task<ActionResult<CartDto>> AddItem(int userId, CartDetailDto cartDto)
        {
            // Verify product
            if (!_productRepository.IsProductExists(cartDto.ProductId))
            {
                return NotFound(new BaseResponse { Message = "Product not found.", Errors = new[] { $"Product ({cartDto.ProductId}) doesn't exists." } });
            }

            // Verify cart
            if (_cartRepository.IsProductFoundInCart(_userService.GetUserId(), cartDto.ProductId))
            {
                return BadRequest(new BaseResponse { Message = "Product found in cart.", Errors = new[] { $"Product ({cartDto.ProductId}) already found in user's ({_userService.GetUserId()}) cart." } });
            }

            var cart = _mapper.Map<CartDetailDto, Cart>(cartDto, opt =>
            {
                opt.AfterMap((cartDto, cart) =>
                {
                    cart.UserId = _userService.GetUserId();
                    cart.CreatedAt = DateTime.Now;
                });
            });

            _cartRepository.CreateCart(cart);
            await _cartRepository.Save();

            return Ok(new BaseResponse { Message = $"Product added into user's cart.", Result = cartDto });
        }

        // DELETE: api/Carts/DeleteItem
        [HttpDelete("DeleteItem")]
        public async Task<IActionResult> DeleteProductInCart(CartDeleteDto cartDeleteDto)
        {
            // Verify product
            if (!_productRepository.IsProductExists(cartDeleteDto.ProductId))
            {
                return NotFound(new BaseResponse { Message = "Product not found.", Errors = new[] { $"Product ({cartDeleteDto.ProductId}) doesn't exists." } });
            }

            // Verify cart
            if (!_cartRepository.IsProductFoundInCart(_userService.GetUserId(), cartDeleteDto.ProductId))
            {
                return NotFound(new BaseResponse { Message = "Product not found in cart.", Errors = new[] { $"Product ({cartDeleteDto.ProductId}) not found in user's ({_userService.GetUserId()}) cart." } });
            }

            await _cartRepository.RemoveCart(_userService.GetUserId(), cartDeleteDto.ProductId);
            await _cartRepository.Save();

            return Ok(new BaseResponse { Message = "Product removed from the cart." });
        }

        // DELETE: api/Carts/ClearCart
        [HttpDelete("ClearCart")]
        public async Task<IActionResult> ClearCart()
        {
            var carts = await _cartRepository.ClearCart(_userService.GetUserId());
            await _cartRepository.Save();

            return Ok(new BaseResponse { Message = $"Cleared all item in cart.", Result = carts, ResultCount = carts.Count() });
        }
    }
}
