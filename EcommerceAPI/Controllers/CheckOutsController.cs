using AutoMapper;
using EcommerceAPI.Data;
using EcommerceAPI.Dto;
using EcommerceAPI.Helper;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckOutsController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IMapper _mapper;
        private readonly IStripeAppService _stripeAppService;

        public CheckOutsController(EcommerceContext context, IMapper mapper, IStripeAppService stripeAppService)
        {
            _context = context;
            _mapper = mapper;
            _stripeAppService = stripeAppService;

        }

        // POST api/Checkouts/1
        [HttpPost("{userId}")]
        public async Task<ActionResult<CheckOutDto>> PostCheckout(int userId, [FromBody] CheckOutDto checkOutDto)
        {
            if (userId != checkOutDto.UserId)
            {
                return BadRequest(new { message = "Fail to verify user id.", errors = new[] { "User Id not match." } });
            }

            // Verify user
            if (!UserExists(checkOutDto.UserId))
            {
                return NotFound(new { message = "User not found.", errors = new[] { $"User ({checkOutDto.UserId}) doesn't exists." } });
            }

            var user = await _context.Users.FindAsync(userId);



            
            return Ok(checkOutDto);
        }

        // POST : api/CheckOuts/Stripe/Payment/Add
        [HttpPost("stripe/payment/add")]
        public async Task<ActionResult<StripePayment>> AddStripePayment(
            [FromBody] StripePaymentDto payment,
            CancellationToken ct)
        {
            StripePayment createdPayment = await _stripeAppService.AddStripePaymentAsync(payment, ct);

            return Ok(createdPayment);
        }

        private bool UserExists(int userId)
        {
            return _context.Users.Any(e => e.UserId == userId);
        }

    }
}
