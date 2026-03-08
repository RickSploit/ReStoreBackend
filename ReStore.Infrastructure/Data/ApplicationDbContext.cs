using Microsoft.EntityFrameworkCore;
using ReStore.Core.Entities;

namespace ReStore.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // 1. ترجمة المستطيلات لجداول فعلية في الداتا بيز (DbSets)
        public DbSet<User> Users { get; set; }
        public DbSet<Appliance> Appliances { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<RepairRequest> RepairRequests { get; set; }
        public DbSet<Certificate> Certificates { get; set; }

        // 2. ترجمة المعينات والخطوط (العلاقات 1 لـ N) باستخدام Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // ترجمة علاقة (Lists): بائع واحد يعرض كذا جهاز
            // ==========================================
            modelBuilder.Entity<Appliance>()
                .HasOne(a => a.Seller)             // (1) الجهاز ليه بائع واحد
                .WithMany(u => u.ListedAppliances) // (N) البائع يقدر يعرض أجهزة كتير
                .HasForeignKey(a => a.SellerId)    // ده الـ Foreign Key
                .OnDelete(DeleteBehavior.Restrict); // حماية: عشان لو مسحنا بائع بالغلط، أجهزته متمسحش وتطير فلوسه

            // ==========================================
            // ترجمة علاقة (Belongs To): تصنيف واحد جواه كذا جهاز
            // ==========================================
            modelBuilder.Entity<Appliance>()
                .HasOne(a => a.Category)           // (1) الجهاز ليه تصنيف واحد (غسالة مثلاً)
                .WithMany(c => c.Appliances)       // (N) التصنيف جواه أجهزة كتير
                .HasForeignKey(a => a.CategoryId);

            // ==========================================
            // ترجمة علاقة (Places): مشتري واحد بيعمل كذا طلب
            // ==========================================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)              // (1) الطلب ليه مشتري واحد
                .WithMany(u => u.Orders)           // (N) المشتري بيعمل طلبات كتير
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================
            // Seeding Initial Data (تطعيم البيانات)
            // ==========================================
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "ثلاجات وديب فريزر" },
                new Category { Id = 2, Name = "غسالات ومجففات" },
                new Category { Id = 3, Name = "بوتاجازات وأفران" },
                new Category { Id = 4, Name = "شاشات وتلفزيونات" },
                new Category { Id = 5, Name = "أجهزة منزلية صغيرة" }
);    
        }
    }
}