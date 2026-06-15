
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using sky_route_app.Data;
using sky_route_app.DTOs;
using sky_route_app.Models;
using sky_route_app.Services;

namespace Sky_View_Tests.Services
{
    public class FlightsServiceTest
    {
        private readonly Mock<ILogger<FlightService>> _logger = new();
        private readonly DbContextOptions<SkyRouteDbContext> _dbContextOptions;
        private readonly SkyRouteDbContext? _context;
        private readonly SqliteConnection _connection;
        public FlightsServiceTest()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection!.Open();
            _dbContextOptions = new DbContextOptionsBuilder<SkyRouteDbContext>().UseSqlite(_connection).Options;
            _context = new(_dbContextOptions);
            if (_context != null)
            {
                _ = _context.Database.EnsureDeleted();
                _ = _context.Database.EnsureCreated();
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
            _connection?.Dispose();
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenDbContextIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FlightService(null!, _logger.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FlightService(_context!, null!));
        }

        [Fact]
        public async Task GetAirports_ReturnsListOfAirports()
        {
            // Arrange
            var flightService = new FlightService(_context!, _logger.Object);
            _context!.Airports.Add(new Airport { Id = 1, Code = "JFK", City = "New York", Country = "USA" });
            await _context.SaveChangesAsync();

            // Act
            var result = await flightService.GetAirports();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task SearchFlights_ReturnsMatchingFlights()
        {
            // Arrange
            var flightService = new FlightService(_context!, _logger.Object);

            // Seed a matching flight
            _context!.Flights.Add(new Flight
            {
                Id = 1,
                Provider = "GlobalAir",
                FlightNumber = "GA123",
                OriginAirportId = 1,
                DestinationAirportId = 2,
                DepartureTime = new DateTime(2026, 6, 15, 8, 0, 0),
                ArrivalTime = new DateTime(2026, 6, 15, 12, 0, 0),
                CabinClass = "Economy",
                BaseFare = 200
            });
            await _context.SaveChangesAsync();

            var request = new FlightSearchRequest
            {
                OriginAirportId = 1,
                DestinationAirportId = 2,
                DepartureDate = new DateTime(2026, 6, 15),
                CabinClass = "Economy",
                Passengers = 2
            };

            // Act
            var result = await flightService.SearchFlights(request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task SearchFlights_ReturnsEmpty_WhenNoFlightsMatch()
        {
            // Arrange
            var flightService = new FlightService(_context!, _logger.Object);

            // Seed a flight that does NOT match the search criteria
            _context!.Flights.Add(new Flight
            {
                Id = 2,
                Provider = "BudgetWings",
                FlightNumber = "BW456",
                OriginAirportId = 3,
                DestinationAirportId = 4,
                DepartureTime = new DateTime(2026, 6, 16, 10, 0, 0),
                ArrivalTime = new DateTime(2026, 6, 16, 14, 0, 0),
                CabinClass = "Business",
                BaseFare = 150
            });
            await _context.SaveChangesAsync();

            var request = new FlightSearchRequest
            {
                OriginAirportId = 1,
                DestinationAirportId = 2,
                DepartureDate = new DateTime(2026, 6, 15),
                CabinClass = "Economy",
                Passengers = 1
            };

            // Act
            var result = await flightService.SearchFlights(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
