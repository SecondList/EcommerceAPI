using AutoMapper;
using EcommerceAPI.Data;
using EcommerceAPI.Dto;
using EcommerceAPI.Helper;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Services.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CheckOutsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IMapper _mapper;
        private readonly IStripeAppService _stripeAppService;

        public CheckOutsController(IUserService userService, ICartRepository cartRepository, IOrderRepository orderRepository, IPaymentRepository paymentRepository, IShipmentRepository shipmentRepository, IMapper mapper, IStripeAppService stripeAppService)
        {
            _userService = userService;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _shipmentRepository = shipmentRepository;
            _mapper = mapper;
            _stripeAppService = stripeAppService;

        }

        // POST api/Checkouts
        [HttpPost]
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult<CheckOutDto>> PostCheckout([FromBody] CheckOutDto checkOutDto, CancellationToken ct)
        {
            decimal amount = 0;

            var carts = await _cartRepository.GetCartsByProductIds(_userService.GetUserId(), checkOutDto.ProductIds);

            if (carts.Count != checkOutDto.ProductIds.Length)
            {
                return NotFound(new BaseResponse { Message = "Cart not found.", Errors = new[] { $"Some of the item not found in user's ({_userService.GetUserId()}) cart." } });
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
                    order.UserId = _userService.GetUserId();
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

            _orderRepository.CreateOrder(order);
            _cartRepository.RemoveRange(carts);
            await _cartRepository.Save();

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

            _paymentRepository.CreatePayment(payment);
            _shipmentRepository.CreateShipment(shipment);
            await _paymentRepository.Save();

            var orderDto = _mapper.Map<OrderDto>(order);

            return Ok(new BaseResponse { Message = "Successfully check out.", Result = orderDto });
        }

        // POST : api/CheckOuts/Stripe/Payment/Add
        [HttpPost("stripe/payment/add")]
        public async Task<ActionResult<StripePayment>> AddStripePayment(
            [FromBody] StripePaymentDto payment,
            CancellationToken ct)
        {
            StripePayment createdPayment = await _stripeAppService.AddStripePaymentAsync(payment, ct);

            return Ok(new BaseResponse { Result = createdPayment });
        }
    }
}
