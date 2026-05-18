namespace ReStore.API.DTOs
{
    public class TechnicianProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public string Specialty { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public bool IsApproved { get; set; }
    }

    public class CreateTechnicianProfileDto
    {
        public string Specialty { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
    }

    public class UpdateTechnicianProfileDto
    {
        public string Specialty { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
    }
}
