using AutoMapper;
using EcommerceAPI.Data;
using EcommerceAPI.Dto;
using EcommerceAPI.Helper;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<CheckOutDto>> PostCheckout(int userId, [FromBody] CheckOutDto checkOutDto, CancellationToken ct)
        {
            decimal amount = 0;

            if (userId != checkOutDto.UserId)
            {
                return BadRequest(new { message = "Fail to verify user id.", errors = new[] { "User Id not match." } });
            }

            // Verify user
            if (!UserExists(checkOutDto.UserId))
            {
                return NotFound(new { message = "User not found.", errors = new[] { $"User ({checkOutDto.UserId}) doesn't exists." } });
            }

            var carts = await _context.Carts.Include(c => c.Product).Where(c => c.UserId == userId && checkOutDto.ProductIds.Contains(c.ProductId)).ToListAsync();

            if (carts.Count != checkOutDto.ProductIds.Length)
            {
                return NotFound(new { message = "Cart not found.", errors = new[] { $"Some of the item not found in user's ({checkOutDto.UserId}) cart." } });
            }

            foreach (var cart in carts)
            {
                amount += (cart.OrderQty * cart.Product.Price);
            }

            var orderDetails = _mapper.Map<List<OrderDetail>>(carts);

            var order = _mapper.Map<CheckOutDto, Order>(checkOutDto, opt =>
            {
                opt.AfterMap((checkOutDto, order) =>
                {
                    order.TotalPrice = amount;
                    order.OrderDetails = orderDetails;
                    order.OrderStatus = 2;
                });
            });

            var stripePayment = _mapper.Map<StripePaymentDto>(checkOutDto, opt =>
            {
                opt.AfterMap((checkOutDto, stripePayment) =>
                {
                    stripePayment.Amount = (Int64)(amount * 100);
                });
            });

            var createdPayment = await _stripeAppService.AddStripePaymentAsync(stripePayment, ct);

            _context.Orders.Add(order);
            _context.Carts.RemoveRange(carts);
            await _context.SaveChangesAsync();

            var payment = _mapper.Map<Order, Payment>(order, opt =>
            {
                opt.AfterMap((order, payment) =>
                {
                    payment.OrderId = order.OrderId;
                    payment.PaymentMethod = "CARD";
                    payment.Status = 2;
                    payment.PaymentRefId = createdPayment.PaymentId;
                    payment.PaymentRefResponse = createdPayment.Response;
                });
            });

            var shipment = _mapper.Map<CheckOutDto, Shipment>(checkOutDto, opt =>
            {
                opt.AfterMap((checkOutDto, shipment) =>
                {
                    shipment.OrderId = order.OrderId;
                    // Generate tracking number.
                    shipment.TrackingNumber = $"{shipment.Carrier}-{order.OrderId}";
                });
            });

            _context.Payments.Add(payment);
            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            var orderDto = _mapper.Map<OrderDto>(order);

            return Ok(new { message = "Successfully check out.", order = orderDto }) ;
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
