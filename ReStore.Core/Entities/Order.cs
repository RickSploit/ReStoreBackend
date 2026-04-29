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
        public OrderStatus Status { get; set; } 

        
        public int BuyerId { get; set; }
        public User Buyer { get; set; } = null!;

        
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending,  
        Shipped,  
        Delivered, 
        Cancelled  
    }
}