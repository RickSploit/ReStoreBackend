using Microsoft.EntityFrameworkCore;
using ReStore.Core.Entities;

namespace ReStore.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // 1. الجداول الأساسية
        public DbSet<User> Users { get; set; }
        public DbSet<Appliance> Appliances { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<RepairRequest> RepairRequests { get; set; }
        public DbSet<Certificate> Certificates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // العلاقات (Fluent API)
            // ==========================================

            modelBuilder.Entity<Appliance>()
                .HasOne(a => a.Seller)
                .WithMany(u => u.ListedAppliances)
                .HasForeignKey(a => a.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appliance>()
                .HasOne(a => a.Category)
                .WithMany(c => c.Appliances)
                .HasForeignKey(a => a.CategoryId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // حل مشكلة الـ Decimal عشان التحذيرات تختفي
            modelBuilder.Entity<Appliance>().Property(a => a.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Appliance>().Property(a => a.Weight_Kg).HasColumnType("decimal(18,2)");

            // ==========================================
            // Seeding Data (تطعيم البيانات)
            // ==========================================

            // 1. الأقسام
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "ثلاجات وديب فريزر" },
                new Category { Id = 2, Name = "غسالات ومجففات" },
                new Category { Id = 3, Name = "بوتاجازات وأفران" },
                new Category { Id = 4, Name = "شاشات وتلفزيونات" },
                new Category { Id = 5, Name = "أجهزة منزلية صغيرة" }
            );

            // 2. الأجهزة
            modelBuilder.Entity<Appliance>().HasData(
                new Appliance { 
                    Id = 1, 
                    Title = "ثلاجة شارب 450 لتر", 
                    Description = "حالة ممتازة، استعمال سنة واحدة، لون فضي", 
                    Price = 15000, 
                    CategoryId = 1, 
                    Condition = ApplianceCondition.Used,
                    Weight_Kg = 80
                },
                new Appliance { 
                    Id = 2, 
                    Title = "غسالة إل جي اتوماتيك", 
                    Description = "7 كيلو، بحالة جيدة جداً، تحتاج صيانة بسيطة", 
                    Price = 8500, 
                    CategoryId = 2, 
                    Condition = ApplianceCondition.Used,
                    Weight_Kg = 65
                },
                new Appliance { 
                    Id = 3, 
                    Title = "بوتاجاز فريش 5 شعلة", 
                    Description = "استعمال خفيف، ستانلس ستيل بالكامل", 
                    Price = 6000, 
                    CategoryId = 3, 
                    Condition = ApplianceCondition.Used,
                    Weight_Kg = 50
                }
            );
        }
    }
}