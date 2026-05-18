using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReStore.API.Entities;
using ReStore.Core.Entities; 
using Microsoft.AspNetCore.Identity;

namespace ReStore.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Appliance> Appliances { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<ApplianceImage> ApplianceImages { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DeliveryInfo> DeliveryInfo { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<RepairRequest> RepairRequests { get; set; }
        public DbSet<TechnicianProfile> TechnicianProfiles { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReturnRequest> ReturnRequests { get; set; }


                protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); 

            builder.Entity<Appliance>()
                .HasOne(a => a.Seller) 
                .WithMany(u => u.ListedAppliances) 
                .HasForeignKey(a => a.SellerId) 
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.PlatformCommission)
                .HasColumnType("decimal(18,2)");

            builder.Entity<TechnicianProfile>()
                .Property(t => t.Rating)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Appliance>()
                .Property(a => a.Price)
                .HasColumnType("decimal(18,2)");
                builder.Entity<Review>()
    .HasOne(r => r.Reviewer)
    .WithMany()
    .HasForeignKey(r => r.ReviewerId)
    .OnDelete(DeleteBehavior.Restrict); 

builder.Entity<Review>()
    .HasOne(r => r.ReviewedUser)
    .WithMany()
    .HasForeignKey(r => r.ReviewedUserId)
    .OnDelete(DeleteBehavior.Restrict);

            // ReturnRequest configurations - prevent cascade delete cycles
            builder.Entity<ReturnRequest>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

builder.Entity<ReturnRequest>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Complaint configurations - prevent cascade delete cycles
            builder.Entity<Complaint>()
                .HasOne(c => c.Order)
                .WithMany()
                .HasForeignKey(c => c.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

builder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TechnicianProfile configurations - prevent cascade delete cycles
            builder.Entity<TechnicianProfile>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
