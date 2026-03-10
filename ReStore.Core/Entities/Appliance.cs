namespace ReStore.Core.Entities
{
    public class Appliance
    {
        public int Id { get; set; } // Primary Key
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        
        // حالات الجهاز
        public ApplianceCondition Condition { get; set; }
        public bool IsSparePart { get; set; } // هل بيتباع كقطع غيار؟
        public ApplianceStatus Status { get; set; }
        
        // وزن الجهاز (ده التريك اللي هنحسب بيه تقليل النفايات الإلكترونية بعدين)
        public decimal Weight_Kg { get; set; }

        // ====== العلاقات (Foreign Keys) ======
        
        // ربط الجهاز بالبائع
        public int SellerId { get; set; }
        public User Seller { get; set; } = null!; // Navigation Property

        // ربط الجهاز بالتصنيف (زي ثلاجة، غسالة)
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public string? ImageUrl { get; set; }
    }

    public enum ApplianceCondition
    {
        New,
        Used,
        Damaged
    }

    public enum ApplianceStatus
    {
        Available,
        Reserved,
        Sold
    }
}