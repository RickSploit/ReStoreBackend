using System;
using System.Collections.Generic;

namespace ReStore.Core.Entities
{
    public class Order
    {
        public int Id { get; set; } // رقم الطلب
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // وقت الطلب
        public decimal TotalAmount { get; set; } // الإجمالي
        public OrderStatus Status { get; set; } // حالة الطلب

        // ربط الطلب بالمشتري
        public int BuyerId { get; set; }
        public User Buyer { get; set; } = null!;

        // الطلب الواحد جواه مجموعة من الأجهزة (تفاصيل الطلب)
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending,   // قيد الانتظار
        Shipped,   // تم الشحن
        Delivered, // تم التوصيل
        Cancelled  // ملغي
    }
}