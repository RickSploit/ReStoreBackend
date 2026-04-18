using ReStore.Core.Entities;

namespace ReStore.API.Entities
{
    public class DeliveryInfo
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string TrackingNumber { get; set; }

        // Relations
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}