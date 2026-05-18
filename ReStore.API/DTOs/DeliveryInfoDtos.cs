namespace ReStore.API.DTOs
{
    public class DeliveryInfoDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
    }

    public class CreateDeliveryInfoDto
    {
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int OrderId { get; set; }
    }

    public class UpdateDeliveryInfoDto
    {
        public string Status { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
    }
}
