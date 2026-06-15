using sky_route_app.Models;

namespace sky_route_app.Data
{
    public static class DbSeeder
    {
        public static void Seed(SkyRouteDbContext db)
        {
            if (!db.Airports.Any())
            {
                db.Airports.AddRange(
                    new Airport { Id = 1, Code = "JFK", City = "New York", Country = "USA" },
                    new Airport { Id = 2, Code = "LAX", City = "Los Angeles", Country = "USA" },
                    new Airport { Id = 3, Code = "LHR", City = "London", Country = "UK" },
                    new Airport { Id = 4, Code = "CDG", City = "Paris", Country = "France" },
                    new Airport { Id = 5, Code = "FRA", City = "Frankfurt", Country = "Germany" },
                    new Airport { Id = 6, Code = "DXB", City = "Dubai", Country = "UAE" }
                );
            }

            if (!db.Flights.Any())
            {
                db.Flights.AddRange(
                    new Flight
                    {
                        Id = 1,
                        Provider = "GlobalAir",
                        FlightNumber = "GA100",
                        OriginAirportId = 1,
                        DestinationAirportId = 3,
                        DepartureTime = DateTime.Today.AddDays(1).AddHours(8),
                        ArrivalTime = DateTime.Today.AddDays(1).AddHours(16),
                        CabinClass = "Economy",
                        BaseFare = 300
                    },
                    new Flight
                    {
                        Id = 2,
                        Provider = "BudgetWings",
                        FlightNumber = "BW200",
                        OriginAirportId = 2,
                        DestinationAirportId = 4,
                        DepartureTime = DateTime.Today.AddDays(2).AddHours(9),
                        ArrivalTime = DateTime.Today.AddDays(2).AddHours(17),
                        CabinClass = "Business",
                        BaseFare = 400
                    }
                );
            }

            db.SaveChanges();
        }
    }
}
