namespace ReStore.API.DTOs
{
    public class ApplianceDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Condition { get; set; } // هنخليها string عشان ترجع "Used" بدل رقم 1
        public string ImageUrl { get; set; }
    }
}