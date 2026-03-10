using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using ReStore.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// الداتا بيز
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// 1. إضافة خدمات الـ Identity
builder.Services.AddIdentityCore<User>(options =>
{
    // تقدر هنا تظبط شروط الباسورد (مثلاً مش لازم حروف معقدة دلوقتي عشان التست)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole<int>>() // تفعيل الأدوار (بائع/مشتري)
.AddEntityFrameworkStores<ApplicationDbContext>(); // ربطهم بالداتا بيز بتاعتنا

// ==========================================
// 1. السطور الجديدة بتاعة إعداد الـ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ==========================================

builder.Services.AddControllers();
builder.Services.AddScoped<ImageService>(); // خدمة الصور بتاعتنا

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // ==========================================
    // 2. السطور الجديدة لتشغيل شاشة الـ Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
    // ==========================================
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // عشان السيرفر يرضى يبعت الصور

app.MapControllers();

app.Run();