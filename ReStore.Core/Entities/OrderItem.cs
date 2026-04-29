namespace ReStore.Core.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public decimal Price { get; set;}
       
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

       
        public int ApplianceId { get; set; }
        public Appliance Appliance { get; set; } = null!;
    }
}