namespace ReStore.API.DTOs
{
    public class CertificateDto
    {
        public int Id { get; set; }
        public DateTime IssueDate { get; set; }
        public int WarrantyMonths { get; set; }
        public string EvaluationDetails { get; set; } = string.Empty;
        public int ApplianceId { get; set; }
        public string? ApplianceTitle { get; set; }
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
    }

    public class CreateCertificateDto
    {
        public int ApplianceId { get; set; }
        public int WarrantyMonths { get; set; }
        public string EvaluationDetails { get; set; } = string.Empty;
    }

    public class UpdateCertificateDto
    {
        public int WarrantyMonths { get; set; }
        public string EvaluationDetails { get; set; } = string.Empty;
    }
}
