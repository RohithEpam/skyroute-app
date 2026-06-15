

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
    public class BookingServiceTest
    {
        private readonly Mock<ILogger<BookingService>> _logger = new();
        private readonly DbContextOptions<SkyRouteDbContext> _dbContextOptions;
        private readonly SkyRouteDbContext? _context;
        private readonly SqliteConnection _connection;
        BookingService bookingService;

        public BookingServiceTest()
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

            bookingService = new BookingService(_context!, _logger.Object);
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
            Assert.Throws<ArgumentNullException>(() => new BookingService(null!, _logger.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BookingService(_context!, null!));
        }

        [Fact]
        public async Task BookFlight_SuccessfullyBooksAFlight()
        {
            // Arrange
            var flight = new Flight
            {
                Id = 1,
                Provider = "GlobalAir",
                FlightNumber = "GA123",
                OriginAirportId = 1,
                DestinationAirportId = 2,
                DepartureTime = DateTime.Now.AddDays(1),
                ArrivalTime = DateTime.Now.AddDays(1).AddHours(2),
                CabinClass = "Economy",
                BaseFare = 200
            };
            _context!.Flights.Add(flight);
            await _context.SaveChangesAsync();

            var bookingRequest = new BookingRequest
            {
                FlightId = 1,
                AirLine = "GlobalAir",
                customerDetails = new List<CustomerDetails>
                {
                    new CustomerDetails
                    {
                        FullName = "John Doe",
                        Email = "john@example.com",
                        PhoneNumber = "1234567890",
                        DocumentNumber = "A1234567"
                    }
                }
            };


            // Act
            var booking = await bookingService.BookFlight(bookingRequest);

            // Assert
            Assert.NotNull(booking);
            Assert.Equal(1, booking.FlightId);
            Assert.Equal("GlobalAir", booking.AirLine);
            Assert.NotNull(booking.ReferenceCode);
            Assert.Single(booking.Customers);
            Assert.Equal("John Doe", booking.Customers.First().FullName);
        }

        [Fact]
        public async Task BookFlight_ThrowsException_WhenFlightNotFound()
        {
            // Arrange
            var bookingRequest = new BookingRequest
            {
                FlightId = 999,
                AirLine = "GlobalAir",
                customerDetails = new List<CustomerDetails>
                {
                    new CustomerDetails
                    {
                        FullName = "Jane Doe",
                        Email = "jane@example.com",
                        PhoneNumber = "0987654321",
                        DocumentNumber = "B7654321"
                    }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => bookingService.BookFlight(bookingRequest));
        }

        [Fact]
        public async Task BookFlight_ThrowsException_WhenRequestIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => bookingService.BookFlight(null!));
        }

        [Fact]
        public async Task BookFlight_BooksFlightWithMultipleCustomers()
        {
            // Arrange
            var flight = new Flight
            {
                Id = 2,
                Provider = "GlobalAir",
                FlightNumber = "GA456",
                OriginAirportId = 1,
                DestinationAirportId = 2,
                DepartureTime = DateTime.Now.AddDays(2),
                ArrivalTime = DateTime.Now.AddDays(2).AddHours(2),
                CabinClass = "Business",
                BaseFare = 400
            };
            _context!.Flights.Add(flight);
            await _context.SaveChangesAsync();

            var bookingRequest = new BookingRequest
            {
                FlightId = 2,
                AirLine = "GlobalAir",
                customerDetails = new List<CustomerDetails>
                {
                    new CustomerDetails
                    {
                        FullName = "Alice Smith",
                        Email = "alice@example.com",
                        PhoneNumber = "1112223333",
                        DocumentNumber = "C1234567"
                    },
                    new CustomerDetails
                    {
                        FullName = "Bob Brown",
                        Email = "bob@example.com",
                        PhoneNumber = "4445556666",
                        DocumentNumber = "D7654321"
                    }
                }
            };


            // Act
            var booking = await bookingService.BookFlight(bookingRequest);

            // Assert
            Assert.NotNull(booking);
            Assert.Equal(2, booking.FlightId);
            Assert.Equal("GlobalAir", booking.AirLine);
            Assert.NotNull(booking.ReferenceCode);
            Assert.Equal(2, booking.Customers.Count);
            Assert.Contains(booking.Customers, c => c.FullName == "Alice Smith");
            Assert.Contains(booking.Customers, c => c.FullName == "Bob Brown");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetBookingsById_Throws_WhenIdIsZeroOrNegative(int invalidId)
        {
            // Arrange
            var bookingService = new BookingService(_context!, _logger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => bookingService.GetBookingsById(invalidId));
        }

        [Fact]
        public async Task GetBookingsById_Throws_WhenBookingNotFound()
        {
            // Arrange
            var bookingService = new BookingService(_context!, _logger.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => bookingService.GetBookingsById(999));
            Assert.Contains("We didn't have any request with the particular id", ex.Message);
        }

        [Fact]
        public async Task GetBookingsById_ReturnsBookingWithFlightAndAirportDetails()
        {
            // Arrange
            var departureAirport = new Airport { Id = 1, Code = "ORG", City = "Origin City", Country = "Origin Country" };
            var arrivalAirport = new Airport { Id = 2, Code = "DST", City = "Destination City", Country = "Destination Country" };
            var flight = new Flight
            {
                Id = 10,
                FlightNumber = "GA123",
                OriginAirportId = departureAirport.Id,
                DestinationAirportId = arrivalAirport.Id,
                DepartureTime = DateTime.Now.AddDays(1),
                ArrivalTime = DateTime.Now.AddDays(1).AddHours(2),
                CabinClass = "Economy",
                Provider = "GlobalAir",
            };
            var booking = new Booking
            {
                Id = 100,
                FlightId = flight.Id,
                ReferenceCode = "REF12345",
                BookingDate = DateOnly.FromDateTime(DateTime.Now),
                AirLine = "GlobalAir",
                Customers = new List<CustomerBookingData>
        {
            new CustomerBookingData
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                DocumentNumber = "A1234567"
            }
        }
            };

            _context!.Airports.AddRange(departureAirport, arrivalAirport);
            _context.Flights.Add(flight);
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var bookingService = new BookingService(_context, _logger.Object);

            // Act
            var result = await bookingService.GetBookingsById(booking.Id);

            // Assert
            Assert.NotNull(result);
        }
    }
}
