using System;
using System.Collections.Generic;
using ReStore.Core.Entities;

namespace ReStore.API.Entities
{
    public class TechnicianProfile
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }  
        public decimal Rating { get; set; }
        public string Specialty { get; set; }
        public int ExperienceYears { get; set; }
        public bool IsApproved { get; set; } = false; // Default to false - requires admin approval
        public DateTime? ApprovedDate { get; set; }
    }
}
