using EcommerceAPI.Models;

namespace EcommerceAPI.Interfaces
{
    public interface IShipmentRepository
    {
        Shipment CreateShipment(Shipment shipment);
        Task<bool> Save();
    }
}
