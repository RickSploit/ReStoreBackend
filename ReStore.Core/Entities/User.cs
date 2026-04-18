using Microsoft.AspNetCore.Identity;

namespace ReStore.Core.Entities
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;
        
        public UserRole Role { get; set; } 

        public ICollection<Appliance> Appliances { get; set; } = new List<Appliance>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Appliance> ListedAppliances { get; set; } = new List<Appliance>();
    }

    public enum UserRole
    {
        Buyer,
        Seller,
        Technician,
        Admin
    }
}