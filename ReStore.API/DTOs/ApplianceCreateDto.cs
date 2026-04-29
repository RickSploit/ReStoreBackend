using Microsoft.AspNetCore.Http; // ضروري عشان الـ IFormFile

namespace ReStore.API.DTOs
{
    public class ApplianceCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int Condition { get; set; } 
        
        // شيلنا الـ SellerId من هنا
        // وضفنا الصورة عشان تيجي مع الـ Form
        public IFormFile Image { get; set; } 
    }
}