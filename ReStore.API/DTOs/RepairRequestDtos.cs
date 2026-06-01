namespace ReStore.API.DTOs
{
    public class RepairRequestDto
    {
        public int Id { get; set; }
        public int ApplianceId { get; set; }
        public string? ApplianceTitle { get; set; }
        public string IssuesDescription { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int SellerId { get; set; }
        public string? SellerName { get; set; }
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
    }

    public class CreateRepairRequestDto
    {
        public int ApplianceId { get; set; }
        public string IssuesDescription { get; set; } = string.Empty;
    }

    public class UpdateRepairRequestDto
    {
        public string Status { get; set; } = string.Empty;
        public int? TechnicianId { get; set; }
    }
}
