using System;
using System.Collections.Generic;
using ReStore.Core.Entities;

namespace ReStore.API.Entities
{
    // --- الشكاوى ---
    public class Complaint
    {
        public int Id { get; set; }
        public string IssueType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // Open, Resolved

public int UserId { get; set; }
        public User User { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}