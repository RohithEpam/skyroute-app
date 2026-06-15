using Microsoft.EntityFrameworkCore;
using sky_route_app.Data;
using sky_route_app.Models;
using sky_route_app.Services;
using sky_route_app.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SkyRouteDbContext>(options =>
    options.UseInMemoryDatabase("SkyRouteDb"));
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IBookingService, BookingService>();
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SkyRouteDbContext>();
    DbSeeder.Seed(db); // Call your seeder here
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();                     
    app.UseSwaggerUI();                    
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
