using System;

namespace ReStore.Core.Entities
{
    public class Certificate
    {
        public int Id { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public int WarrantyMonths { get; set; }         
        public string EvaluationDetails { get; set; } = string.Empty; 

        
        public int ApplianceId { get; set; }
        public Appliance Appliance { get; set; } = null!;

        public int TechnicianId { get; set; }
        public User Technician { get; set; } = null!;
    }
}