namespace ReStore.Core.Entities
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ApplianceId { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Appliance Appliance { get; set; } = null!;
    }
}