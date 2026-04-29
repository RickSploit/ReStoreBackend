using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using ReStore.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Services Configuration 
// ==========================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// تعريف الـ Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 🔐 إعدادات المصادقة بالتوكن (JWT Authentication)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// خدمة الصور بتاعتنا
builder.Services.AddScoped<ImageService>(); 

// إعدادات الكورز (CORS) 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200") 
              .AllowAnyMethod()    
              .AllowAnyHeader()
              .AllowCredentials(); 
    });
});

var app = builder.Build();

// ==========================================
// 2. HTTP Request Pipeline (الترتيب هنا حياة أو موت)
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// عشان السيرفر يرضى يبعت الصور للفرونت إند
app.UseStaticFiles(); 

// التوجيه لازم ييجي قبل الكورز
app.UseRouting();

// تفعيل الكورز
app.UseCors("AllowAll"); 

// المصادقة والصلاحيات لازم يجوا بعد الكورز وقبل الكنترولرز
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();