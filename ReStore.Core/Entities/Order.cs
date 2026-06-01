using System;
using System.Collections.Generic;

namespace ReStore.Core.Entities
{
    public class Order
    {
        public int Id { get; set; } 
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; 
        public decimal TotalAmount { get; set; } 
        public decimal PlatformCommission { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        
        public int BuyerId { get; set; }
        public User Buyer { get; set; } = null!;

        
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Appliance> Appliances { get; set; } = new List<Appliance>();
    }

    public enum OrderStatus
    {
        Pending,
        PickedUp,
        InTransit,
        Delivered,
        Cancelled
    }
}