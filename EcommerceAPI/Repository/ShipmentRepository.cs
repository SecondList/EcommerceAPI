using EcommerceAPI.Data;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;

namespace EcommerceAPI.Repository
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly EcommerceContext _context;

        public ShipmentRepository(EcommerceContext context)
        {
            _context = context;
        }

        public Shipment CreateShipment(Shipment shipment)
        {
            _context.Shipments.Add(shipment);
            return shipment;
        }

        public async Task<bool> Save()
        {
            var saved = await _context.SaveChangesAsync();

            return saved > 0 ? true : false;
        }
    }
}
