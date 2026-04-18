using System;

namespace ReStore.Core.Entities
{
    public class RepairRequest
    {
        public int Id { get; set; }
        public string DeviceType { get; set; } = string.Empty; 
        public string ProblemDescription { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public RepairRequestStatus Status { get; set; }

        
        public int BuyerId { get; set; }
        public User Buyer { get; set; } = null!;

       
        public int? TechnicianId { get; set; }
        public User? Technician { get; set; }
    }

    public enum RepairRequestStatus
    {
        Pending,   
        Accepted,  
        Completed, 
        Rejected   
    }
}