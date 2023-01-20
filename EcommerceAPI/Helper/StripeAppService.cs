using AutoMapper;
using Castle.Core.Resource;
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
            // Set Stripe Token options based on cart detail
            TokenCreateOptions tokenOptions = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Name = payment.Card.CardName,
                    Number = payment.Card.CardNumber,
                    ExpYear = payment.Card.ExpYear,
                    ExpMonth = payment.Card.ExpMonth.ToString(),
                    Cvc = payment.Card.Cvc
                }
            };
            // Create new Stripe Token
            Token stripeToken = await _tokenService.CreateAsync(tokenOptions, null, ct);

            AddressOptions addressOptions = new AddressOptions
            {
                City = payment.City,
                Country = payment.Country,
                Line1 = payment.Address1,
                Line2 = payment.Address2,
                PostalCode = payment.PostalCode,
                State = payment.State
            };

            ChargeShippingOptions shippingOptions = new ChargeShippingOptions
            {
                Address = addressOptions,
                Carrier = payment.Carrier,
                Name = payment.RecipientName,
                Phone = payment.Phone,
                TrackingNumber = payment.TrackingNumber
            };

            ChargeCreateOptions paymentOptions = new ChargeCreateOptions
            {
                ReceiptEmail = payment.ReceiptEmail,
                Description = payment.Description,
                Currency = payment.Currency,
                Amount = payment.Amount,
                Shipping = shippingOptions,
                Source = stripeToken.Id
            };
            var createdPayment = await _chargeService.CreateAsync(paymentOptions, null, ct);

            // Return the payment to requesting method
            var stripePayment = new StripePayment
            {
                Description = createdPayment.Description,
                Currency = createdPayment.Currency,
                Amount = createdPayment.Amount,
                PaymentId = createdPayment.Id,
                Response = createdPayment.ToString()
            };

            return stripePayment;
        }
    }
}
