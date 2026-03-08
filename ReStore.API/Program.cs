using Microsoft.EntityFrameworkCore;
using ReStore.Infrastructure.Data; 


var builder = WebApplication.CreateBuilder(args);
// السطر ده بيربط الـ API بالداتا بيز باستخدام العنوان اللي كتبناه
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


 
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
app.MapControllers();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
