namespace ReStore.Core.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public decimal Price { get; set; } // سعر الجهاز وقت الشراء (عشان لو السعر اتغير بعدين)

        // مربوط بـ أي طلب؟
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        // إيه هو الجهاز اللي اتباع؟
        public int ApplianceId { get; set; }
        public Appliance Appliance { get; set; } = null!;
    }
}