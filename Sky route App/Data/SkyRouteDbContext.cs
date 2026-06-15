using sky_route_app.Models;
using Microsoft.EntityFrameworkCore;

namespace sky_route_app.Data
{
    public class SkyRouteDbContext : DbContext
    {
        public SkyRouteDbContext(DbContextOptions<SkyRouteDbContext> options) : base(options) { }

        public DbSet<Airport> Airports => Set<Airport>();
        public DbSet<Flight> Flights => Set<Flight>();
        public DbSet<Booking> Bookings => Set<Booking>();
    }
}
