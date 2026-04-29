namespace ReStore.API.DTOs 
{
    public class ApplianceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Condition { get; set; } = string.Empty; 
        public string CategoryName { get; set; } = string.Empty;
        
    
        public string? ImageUrl { get; set; } 
        
        public List<string> ImageUrls { get; set; } = new List<string>(); 
    }
}