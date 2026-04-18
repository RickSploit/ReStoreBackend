using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using ReStore.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);


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

// ==========================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ==========================================

builder.Services.AddControllers();
builder.Services.AddScoped<ImageService>(); 



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    
              .AllowAnyMethod()    
              .AllowAnyHeader();   
    });
});
var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("AllowAll"); 

app.UseAuthentication(); 
app.UseAuthorization();


if (app.Environment.IsDevelopment())
{
    // ==========================================
    app.UseSwagger();
    app.UseSwaggerUI();
    // ==========================================
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapControllers();

app.Run();