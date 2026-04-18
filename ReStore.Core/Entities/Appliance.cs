using ReStore.API.Entities;

namespace ReStore.Core.Entities
{
    public class Appliance
    {
        public int Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        
        public ApplianceCondition Condition { get; set; }
        public bool IsSparePart { get; set; }
        public ApplianceStatus Status { get; set; }
        
        public decimal Weight_Kg { get; set; }

        public int SellerId { get; set; }
        public User Seller { get; set; } = null!; 

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        
        // --- التصحيح هنا ---
        // مسحنا ImageUrl وحطينا علاقة الـ One-to-Many مع جدول الصور
        public ICollection<ApplianceImage> Images { get; set; } = new List<ApplianceImage>();
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