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

        public string UserId { get; set; } // اللي قدم الشكوى
        public int OrderId { get; set; } // الشكوى بخصوص طلب إيه
        public Order Order { get; set; }
    }
}