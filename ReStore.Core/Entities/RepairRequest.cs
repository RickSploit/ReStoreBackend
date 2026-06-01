namespace ReStore.Core.Entities
{
    public class RepairRequest
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public int? TechnicianId { get; set; }
        public int ApplianceId { get; set; }
        public string IssuesDescription { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public RepairRequestStatus Status { get; set; } = RepairRequestStatus.Pending;

        public User Seller { get; set; } = null!;
        public User Technician { get; set; } = null!;
        public Appliance Appliance { get; set; } = null!;
    }
}