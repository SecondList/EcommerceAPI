using EcommerceAPI.Data;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;

namespace EcommerceAPI.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly EcommerceContext _context;

        public PaymentRepository(EcommerceContext context)
        {
            _context = context;
        }

        public Payment CreatePayment(Payment payment)
        {
            _context.Payments.Add(payment);
            return payment;
        }

        public async Task<bool> Save()
        {
            var saved = await _context.SaveChangesAsync();

            return saved > 0 ? true : false;
        }
    }
}
