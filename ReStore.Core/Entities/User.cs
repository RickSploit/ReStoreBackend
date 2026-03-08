using System.Collections.Generic;

namespace ReStore.Core.Entities
{
    public class User
    {
        public int Id { get; set; } // Primary Key
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        
        // استخدام Enum عشان نحدد دور المستخدم (مشتري، بائع، إلخ)
        public UserRole Role { get; set; } 

        // العلاقات (Navigation Properties) - المستخدم ممكن يكون عنده أكتر من جهاز أو طلب
        public ICollection<Appliance> Appliances { get; set; } = new List<Appliance>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        // ضيف السطور دي جوه كلاس الـ User
        public ICollection<Appliance> ListedAppliances { get; set; } = new List<Appliance>();
    }

    // الـ Enum ده بيخلينا نكتب الأدوار بشكل آمن بدل ما نكتبها كـ String وممكن نغلط في حرف
    public enum UserRole
    {
        Buyer,
        Seller,
        Technician,
        Admin
    }
}