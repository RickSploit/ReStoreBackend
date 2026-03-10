using Microsoft.EntityFrameworkCore;
using ReStore.Infrastructure.Data;
using ReStore.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// الداتا بيز
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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