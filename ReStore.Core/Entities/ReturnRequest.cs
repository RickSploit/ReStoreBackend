using System;

namespace ReStore.Core.Entities
{
    public class ReturnRequest
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        
public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
        
        public decimal? RefundAmount { get; set; }
        
        public string? AdminNote { get; set; }
        
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? ProcessedDate { get; set; }
    }

    public enum ReturnStatus
    {
        Pending,
        Approved,
        Rejected,
        Processed
    }
}
