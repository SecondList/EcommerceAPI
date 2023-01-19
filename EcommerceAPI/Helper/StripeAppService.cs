using EcommerceAPI.Dto;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using Stripe;

namespace EcommerceAPI.Helper
{
    public class StripeAppService : IStripeAppService
    {
        private readonly ChargeService _chargeService;
        private readonly TokenService _tokenService;

        public StripeAppService(ChargeService chargeService, TokenService tokenService)
        {
            _chargeService = chargeService;
            _tokenService = tokenService;
        }

        /// Add a new payment at Stripe.
        /// Customer not exist at Stripe.
        public async Task<StripePayment> AddStripePaymentAsync(StripePaymentDto payment, CancellationToken ct)
        {
            ChargeCreateOptions paymentOptions = new ChargeCreateOptions
            {
                Customer = payment.CustomerId,
                ReceiptEmail = payment.ReceiptEmail,
                Description = payment.Description,
                Currency = payment.Currency,
                Amount = payment.Amount
            };

            var createdPayment = await _chargeService.CreateAsync(paymentOptions, null, ct);

            // Return the payment to requesting method
            var stripePayment = new StripePayment
            {
                CustomerId = createdPayment.CustomerId,
                ReceiptEmail = createdPayment.ReceiptEmail,
                Description = createdPayment.Description,
                Currency = createdPayment.Currency,
                Amount = createdPayment.Amount,
                PaymentId = createdPayment.Id
            };

            return stripePayment;
        }
    }
}
