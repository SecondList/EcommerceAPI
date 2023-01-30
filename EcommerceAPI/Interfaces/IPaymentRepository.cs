using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface IPaymentRepository
    {
        Payment CreatePayment(Payment payment);
        Task<bool> Save();
    }
}
