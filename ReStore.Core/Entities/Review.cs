using System;
using ReStore.Core.Entities;

namespace ReStore.Core.Entities 
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

        public int ReviewerId { get; set; }
        public User Reviewer { get; set; }        

               public int ReviewedUserId { get; set; }
        public User ReviewedUser { get; set; }

               public int? ApplianceId { get; set; } 
        public Appliance Appliance { get; set; }
    }
}