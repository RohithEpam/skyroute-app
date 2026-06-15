using Microsoft.AspNetCore.Mvc;
using sky_route_app.Data;
using sky_route_app.DTOs;
using sky_route_app.Models;
using sky_route_app.Services.Interfaces;


namespace sky_route_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> Book([FromBody] BookingRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.AirLine != "GlobalAir" && request.AirLine != "BudgetWings")
            {
                _logger.LogWarning("Invalid airline specified: {AirLine}", request.AirLine);
                return BadRequest(new { error = "Invalid AirLine. Must be 'GlobalAir' or 'BudgetWings'." });
            }

            _logger.LogInformation("Received booking request for flight ID: {FlightId}", request.FlightId);
            Booking bookingData = await _bookingService.BookFlight(request);
            _logger.LogInformation("Booking successful with ID: {Id} and Reference Code: {ReferenceCode}", bookingData.Id, bookingData.ReferenceCode);

            return CreatedAtAction(nameof(GetBookingsById), new { id = bookingData.Id },
                new { bookingData.Id, bookingData.ReferenceCode });
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingsById(int id)

        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0);

            _logger.LogInformation("Received request to get booking details for ID: {Id}", id);
            var BookingDataWithFlightDetails = await _bookingService.GetBookingsById(id);
            _logger.LogInformation("Returning booking details for ID: {Id}", id);
            return Ok(BookingDataWithFlightDetails);
        }
    }
}
