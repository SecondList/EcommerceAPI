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

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CartsController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly IMapper _mapper;

        public CartsController(ICartRepository cartRepository, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _mapper = mapper;
        }

        // GET: api/Carts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartDetailDto>>> GetCarts()
        {
            var carts = await _context.Carts.ToListAsync();
            var cartsDto = _mapper.Map<List<CartDetailDto>>(carts);

            return Ok(new { resultCount = cartsDto.Count, carts = cartsDto });
        }

        // PUT: api/Carts/UpdateOrderQtylinkid=2123754
        [HttpPut("UpdateOrderQty")]
        public async Task<IActionResult> UpdateOrderQty([FromBody] CartDetailDto cartDto)
        {
            if (userId != cartDto.UserId)
            {
                return BadRequest(new { message = "Fail to verify user id.", errors = new[] { "User Id not match." } });
            }

            // Verify user
            if (!UserExists(cartDto.UserId))
            {
                return NotFound(new { message = "User not found.", errors = new[] { $"User ({cartDto.UserId}) doesn't exists." } });
            }

            // Verify product
            if (!ProductExists(cartDto.ProductId))
            {
                return NotFound(new { message = "Product not found.", errors = new[] { $"Product ({cartDto.ProductId}) doesn't exists." } });
            }

            // Verify cart
            if (!ProductExistsInCart(cartDto.UserId, cartDto.ProductId))
            {
                return NotFound(new { message = "Unable to update this item in cart.", errors = new[] { $"Product ({cartDto.ProductId}) is not found in user's ({cartDto.UserId}) cart." } });
            }

            var cart = _mapper.Map<Cart>(cartDto);

            _context.Update(cart);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order quantity updated." });
        }

        // POST: api/Carts/1/AddItem
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}/AddItem")]
        public async Task<ActionResult<CartDto>> AddItem(int userId, CartDetailDto cartDto)
        {
            if (userId != cartDto.UserId)
            {
                return BadRequest(new { message = "Fail to verify user id.", errors = new[] { "User Id not match." } });
            }

            // Verify user
            if (!UserExists(cartDto.UserId))
            {
                return NotFound(new { message = "User not found.", errors = new[] { $"User ({cartDto.UserId}) doesn't exists." } });
            }

            // Verify product
            if (!ProductExists(cartDto.ProductId))
            {
                return NotFound(new { message = "Product not found.", errors = new[] { $"Product ({cartDto.ProductId}) doesn't exists." } });
            }

            // Verify cart
            if (ProductExistsInCart(cartDto.UserId, cartDto.ProductId))
            {
                return BadRequest(new { message = "Unable to add this item into cart.", errors = new[] { $"Product ({cartDto.ProductId}) already found in user's ({cartDto.UserId}) cart." } });
            }

            var cart = _mapper.Map<Cart>(cartDto);

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Product ({cartDto.ProductId}) added into user's ({cartDto.UserId}) cart.", cart = cartDto });
        }

        // DELETE: api/Carts/5/DeleteItem
        [HttpDelete("{userId}/DeleteItem")]
        public async Task<IActionResult> DeleteProductInCart(int userId, CartDeleteDto cartDeleteDto)
        {
            if (userId != cartDeleteDto.UserId)
            {
                return BadRequest(new { message = "Fail to verify user id.", errors = new[] { "User Id not match." } });
            }

            // Verify user
            if (!UserExists(cartDeleteDto.UserId))
            {
                return NotFound(new { message = "User not found.", errors = new[] { $"User ({cartDeleteDto.UserId}) doesn't exists." } });
            }

            // Verify product
            if (!ProductExists(cartDeleteDto.ProductId))
            {
                return NotFound(new { message = "Product not found.", errors = new[] { $"Product ({cartDeleteDto.ProductId}) doesn't exists." } });
            }

            // Verify cart
            if (!ProductExistsInCart(cartDeleteDto.UserId, cartDeleteDto.ProductId))
            {
                return NotFound(new { message = "Unable to remove this item from cart.", errors = new[] { $"Product ({cartDeleteDto.ProductId}) not found in user's ({cartDeleteDto.UserId}) cart." } });
            }

            var cart = await _context.Carts.FindAsync(cartDeleteDto.UserId, cartDeleteDto.ProductId);

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product removed from the cart." });
        }

        // DELETE: api/Carts/5/ClearCart
        [HttpDelete("{userId}/ClearCart")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            // Verify user
            if (!UserExists(userId))
            {
                return NotFound(new { message = "User not found.", errors = new[] { $"User ({userId}) doesn't exists." } });
            }

            var carts = _context.Carts.Where(c => c.UserId == userId);

            carts.ToList().ForEach(e => _context.Carts.Remove(e));
            //_context.Carts.Remove(carts);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Cleared all item in user ({userId}) cart." });
        }

        private bool UserExists(int userId)
        {
            return _context.Users.Any(e => e.UserId == userId);
        }

        private bool ProductExists(int productId)
        {
            return _context.Products.Any(e => e.ProductId == productId);
        }

        private bool ProductExistsInCart(int userId, int productId)
        {
            return _context.Carts.Any(e => e.UserId == userId && e.ProductId == productId);
        }

    }
}
