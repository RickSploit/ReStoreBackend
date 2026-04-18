using System;
using System.Collections.Generic;
using ReStore.Core.Entities;

namespace ReStore.API.Entities
{
    // --- الشكاوى ---
    public class TechnicianProfile
    {
        public int Id { get; set; } 
        public User User { get; set; }  
        public decimal Rating { get; set; }
        public string Specialty { get; set; }
        public int ExperienceYears { get; set; }

        
    }
}