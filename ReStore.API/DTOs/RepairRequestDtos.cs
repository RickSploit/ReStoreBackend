namespace ReStore.API.DTOs
{
    public class RepairRequestDto
    {
        public int Id { get; set; }
        public string DeviceType { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
    }

    public class CreateRepairRequestDto
    {
        public string DeviceType { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
    }

    public class UpdateRepairRequestDto
    {
        public string Status { get; set; } = string.Empty;
        public int? TechnicianId { get; set; }
    }
}
