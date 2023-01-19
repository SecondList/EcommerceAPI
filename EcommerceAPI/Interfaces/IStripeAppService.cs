using EcommerceAPI.Dto;
using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface IStripeAppService
    {
        Task<StripePayment> AddStripePaymentAsync(StripePaymentDto payment, CancellationToken ct);
    }
}
