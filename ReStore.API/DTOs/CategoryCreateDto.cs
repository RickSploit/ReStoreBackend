using System.ComponentModel.DataAnnotations;

namespace ReStore.API.DTOs
{
    public class CategoryCreateDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}