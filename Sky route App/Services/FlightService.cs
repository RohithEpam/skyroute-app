using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sky_route_app.Controllers;
using sky_route_app.Data;
using sky_route_app.DTOs;
using sky_route_app.Services.Interfaces;

namespace sky_route_app.Services
{
    public class FlightService : IFlightService
    {
        private readonly SkyRouteDbContext _skyRoutedb;
        private readonly ILogger<FlightService> _logger;

        public FlightService(SkyRouteDbContext skyRoutedb, ILogger<FlightService> logger)
        {
            _skyRoutedb = skyRoutedb ?? throw new ArgumentNullException(nameof(skyRoutedb)); 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<object>> GetAirports()
        {
            var airports = await _skyRoutedb.Airports.ToListAsync();
            return airports;
        }

        public async Task<IEnumerable<object>> SearchFlights([FromBody] FlightSearchRequest request)
        {
            _logger.LogInformation(
                "Flight search initiated. Origin: {OriginAirportId}, Destination: {DestinationAirportId}, " +
                "Date: {DepartureDate}, CabinClass: {CabinClass}, Passengers: {Passengers}",
                request.OriginAirportId, request.DestinationAirportId, request.DepartureDate,
                request.CabinClass, request.Passengers);

            var flights = await _skyRoutedb.Flights
                .Where(f =>
                    f.OriginAirportId == request.OriginAirportId &&
                    f.DestinationAirportId == request.DestinationAirportId &&
                    f.DepartureTime.Date == request.DepartureDate.Date &&
                    f.CabinClass == request.CabinClass)
                .ToListAsync();

            _logger.LogInformation("Found {FlightCount} flights matching search criteria", flights.Count);

            var results = flights.Select(f =>
            {
                decimal pricePerPassenger = f.Provider switch
                {
                    "GlobalAir" => Math.Round(f.BaseFare * 1.15m, 2),
                    "BudgetWings" => Math.Max(Math.Round(f.BaseFare * 0.9m, 2), 29.99m),
                    _ => f.BaseFare
                };
                pricePerPassenger += request.CabinClass switch
                {
                    "Economy" => 100,
                    "Business" => 200,
                    "First" => 300,
                    _ => 0
                };

                _logger.LogDebug("Calculated price for flight {FlightNumber} ({Provider}): {PricePerPassenger}",
                    f.FlightNumber, f.Provider, pricePerPassenger);

                return new
                {
                    f.Id,
                    f.Provider,
                    f.FlightNumber,
                    f.DepartureTime,
                    f.ArrivalTime,
                    Duration = f.ArrivalTime - f.DepartureTime,
                    f.CabinClass,
                    PricePerPassenger = pricePerPassenger,
                    TotalPrice = pricePerPassenger * request.Passengers
                };
            });

            _logger.LogInformation("Flight search completed successfully. Results: {ResultCount}", results.Count());
            return results;
        }
    }
}
