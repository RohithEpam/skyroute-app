using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using sky_route_app.Controllers;
using sky_route_app.DTOs;
using sky_route_app.Services.Interfaces;

namespace SkyrouteTests.Controllers
{
    public class BookingControllerTests
    {
        private readonly Mock<IBookingService> _bookingService = new();
        private readonly Mock<ILogger<BookingsController>> _logger = new();

        public BookingControllerTests()
        {
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenBookigServiceIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BookingsController(null!, _logger.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BookingsController(_bookingService.Object!, null!));
        }

        [Fact]
        public async Task Book_InvalidAirLine_ReturnsBadRequest()
        {
            // Arrange
            var request = new BookingRequest
            {
                FlightId = 123,
                AirLine = "InvalidAir",
                customerDetails = new List<CustomerDetails>
            {
                new CustomerDetails
                {
                    FullName = "Jane Doe",
                    Email = "jane.doe@example.com",
                    DocumentNumber = "DOC54321",
                    PhoneNumber = "0987654321"
                }
            }
            };

            var controller = new BookingsController(_bookingService.Object, _logger.Object);

            // Act
            var result = await controller.Book(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid AirLine", badRequestResult.Value!.ToString());
        }

        [Fact]
        public async Task Book_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var bookingServiceMock = new Mock<IBookingService>();
            var loggerMock = new Mock<ILogger<BookingsController>>();
            var controller = new BookingsController(_bookingService.Object, _logger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => controller.Book(null!));
        }

        [Fact]
        public async Task GetBookingsById_ValidId_ReturnsOk()
        {
            // Arrange
            int bookingId = 1;
            var mockBookingData = new
            {
                Id = bookingId,
                BookingDate = DateOnly.FromDateTime(DateTime.Now),
                AirLine = "GlobalAir",
                customers = new[]
                {
            new
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890",
                DocumentNumber = "DOC12345"
            }
        },
                Flight = new
                {
                    Id = 10,
                    FlightNumber = "GA123",
                    DepartureAirport = "JFK",
                    ArrivalAirport = "LAX",
                    DepartureTime = DateTime.Now.AddHours(2),
                    ArrivalTime = DateTime.Now.AddHours(5),
                    CabinClass = "Economy"
                }
            };

            _bookingService.Setup(s => s.GetBookingsById(bookingId)).ReturnsAsync(mockBookingData);

            var loggerMock = new Mock<ILogger<BookingsController>>();
            var controller = new BookingsController(_bookingService.Object, _logger.Object);

            // Act
            var result = await controller.GetBookingsById(bookingId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mockBookingData, okResult.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetBookingsById_IdLessThanOrEqualZero_ThrowsArgumentOutOfRangeException(int invalidId)
        {
            // Arrange
            var bookingServiceMock = new Mock<IBookingService>();
            var loggerMock = new Mock<ILogger<BookingsController>>();
            var controller = new BookingsController(bookingServiceMock.Object, loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => controller.GetBookingsById(invalidId));
        }
    }
}
