using System;

namespace ReStore.Core.Entities
{
    public class RepairRequest
    {
        public int Id { get; set; }
        public string DeviceType { get; set; } = string.Empty; // نوع الجهاز (ثلاجة، غسالة)
        public string ProblemDescription { get; set; } = string.Empty; // وصف العطل
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public RepairRequestStatus Status { get; set; }

        // مين اللي طالب الصيانة؟
        public int BuyerId { get; set; }
        public User Buyer { get; set; } = null!;

        // مين الفني اللي هيستلم الطلب؟ (ممكن تكون Null في البداية لحد ما فني يقبلها)
        public int? TechnicianId { get; set; }
        public User? Technician { get; set; }
    }

    public enum RepairRequestStatus
    {
        Pending,   // في انتظار فني
        Accepted,  // فني قبل الطلب
        Completed, // تم الإصلاح
        Rejected   // مرفوض
    }
}