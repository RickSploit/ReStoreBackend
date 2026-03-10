namespace ReStore.API.DTOs
{
    public class ApplianceCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int SellerId { get; set; }
        public int Condition { get; set; } 
    }
}