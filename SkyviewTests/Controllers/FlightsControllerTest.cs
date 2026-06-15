
using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using sky_route_app.Controllers;
using sky_route_app.DTOs;
using sky_route_app.Models;
using sky_route_app.Services;
using sky_route_app.Services.Interfaces;

namespace SkyrouteTests.Controllers
{
    public class FlightsControllerTest
    {
        public readonly Mock<IFlightService> _flightService = new();
        public readonly Mock<ILogger<FlightsController>> _logger = new();


        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFlightServiceIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FlightsController(null!, _logger.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FlightsController(_flightService.Object!, null!));
        }

        [Fact]
        public async Task GetAirports_ReturnsOkWithAirportObjects()
        {
            // Arrange
            var mockAirports = new List<Airport>
            {
                new Airport { Id = 1, Code = "JFK", City = "New York", Country = "USA" },
                new Airport { Id = 2, Code = "LAX", City = "Los Angeles", Country = "USA" },
                new Airport { Id = 3, Code = "LHR", City = "London", Country = "UK" },
                new Airport { Id = 4, Code = "CDG", City = "Paris", Country = "France" },
                new Airport { Id = 5, Code = "FRA", City = "Frankfurt", Country = "Germany" },
                new Airport { Id = 6, Code = "DXB", City = "Dubai", Country = "UAE" }
            };

            _flightService.Setup(s => s.GetAirports()).ReturnsAsync(mockAirports);

            var controller = new FlightsController(_flightService.Object, _logger.Object);

            // Act
            var result = await controller.GetAirports();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAirports = Assert.IsAssignableFrom<IEnumerable<Airport>>(okResult.Value);
            Assert.Equal(6, returnedAirports.Count());
            Assert.Contains(returnedAirports, a => a.Code == "JFK" && a.City == "New York");
        }

        [Fact]
        public async Task GetAirports_Return_NULL()
        {
            // Arrange

            var controller = new FlightsController(_flightService.Object, _logger.Object);

            // Act
            var result = await controller.GetAirports();

            //Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task SearchFlights_ValidRequest_ReturnsOkWithCalculatedFlights()
        {
            // Arrange
            var request = new FlightSearchRequest
            {
                OriginAirportId = 1,
                DestinationAirportId = 2,
                DepartureDate = DateTime.Today,
                Passengers = 2,
                CabinClass = "Economy"
            };

            var mockFlights = new List<dynamic>
            {
                new
                {
                    Id = 1,
                    Provider = "GlobalAir",
                    FlightNumber = "GA123",
                    DepartureTime = DateTime.Today.AddHours(8),
                    ArrivalTime = DateTime.Today.AddHours(12),
                    CabinClass = "Economy",
                    PricePerPassenger = 100.00m,
                    TotalPrice = 200.00m,
                    Duration = TimeSpan.FromHours(4)
                },
                new
                {
                    Id = 2,
                    Provider = "BudgetWings",
                    FlightNumber = "BW456",
                    DepartureTime = DateTime.Today.AddHours(9),
                    ArrivalTime = DateTime.Today.AddHours(13),
                    CabinClass = "Economy",
                    PricePerPassenger = 90.00m,
                    TotalPrice = 180.00m,
                    Duration = TimeSpan.FromHours(4)
                }
            };

            var flightServiceMock = new Mock<IFlightService>();
            flightServiceMock.Setup(s => s.SearchFlights(request)).ReturnsAsync(mockFlights);

            var loggerMock = new Mock<ILogger<FlightsController>>();
            var controller = new FlightsController(flightServiceMock.Object, loggerMock.Object);

            // Act
            var result = await controller.SearchFlights(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedFlights = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.Equal(2, returnedFlights.Count());
            Assert.Contains(returnedFlights, f => f.Provider == "GlobalAir");
            Assert.Contains(returnedFlights, f => f.Provider == "BudgetWings");
        }

        [Fact]
        public async Task SearchFlights_ServiceThrowsException_ThrowsException()
        {
            // Arrange
            var request = new FlightSearchRequest
            {
                OriginAirportId = 1,
                DestinationAirportId = 2,
                DepartureDate = DateTime.Today,
                Passengers = 2,
                CabinClass = "Economy"
            };

            var flightServiceMock = new Mock<IFlightService>();
            flightServiceMock.Setup(s => s.SearchFlights(request)).ThrowsAsync(new Exception("Service error"));

            var loggerMock = new Mock<ILogger<FlightsController>>();
            var controller = new FlightsController(flightServiceMock.Object, loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => controller.SearchFlights(request));
        }
    }
}
