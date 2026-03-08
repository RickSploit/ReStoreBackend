using System.Collections.Generic;

namespace ReStore.Core.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // التصنيف الواحد جواه أكتر من جهاز
        public ICollection<Appliance> Appliances { get; set; } = new List<Appliance>();
    }
}