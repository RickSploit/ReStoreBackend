using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using ReStore.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Services Configuration 
// ==========================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole<int>>() 
.AddSignInManager<SignInManager<User>>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddScoped<ImageService>(); 


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



app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


app.UseStaticFiles();


app.UseRouting();


app.UseCors("AllowAll"); 

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();