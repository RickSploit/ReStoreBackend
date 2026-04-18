using ReStore.Core.Entities;

namespace ReStore.API.Entities 
{
    public class ApplianceImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; } 
        // Relations
        public int ApplianceId { get; set; }
        public Appliance Appliance { get; set; }
    }
}