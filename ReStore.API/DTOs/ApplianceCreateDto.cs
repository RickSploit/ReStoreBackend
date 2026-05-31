using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ReStore.API.DTOs
{
    public class ApplianceCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int Condition { get; set; } // 0 = New, 1 = Used, 2 = Damaged

        // هنا التعديل: رجعناها IFormFile عشان تستقبل الصورة كملف من الـ FormData
        public IFormFile? Image { get; set; } 
    }
}