using System;

namespace ReStore.Core.Entities
{
    public class Certificate
    {
        public int Id { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.UtcNow; // تاريخ الإصدار
        public int WarrantyMonths { get; set; } // عدد شهور الضمان (مثلا 6 شهور)
        public string EvaluationDetails { get; set; } = string.Empty; // تقرير الفني عن حالة الجهاز

        // الشهادة دي طالعة لأي جهاز؟
        public int ApplianceId { get; set; }
        public Appliance Appliance { get; set; } = null!;

        // مين الفني اللي فحص الجهاز وطلع الشهادة؟
        public int TechnicianId { get; set; }
        public User Technician { get; set; } = null!;
    }
}