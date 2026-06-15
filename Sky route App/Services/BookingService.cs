using Microsoft.EntityFrameworkCore;
using sky_route_app.Data;
using sky_route_app.DTOs;
using sky_route_app.Models;
using sky_route_app.Services.Interfaces;

namespace sky_route_app.Services
{
    public class BookingService : IBookingService
    {
        private readonly SkyRouteDbContext _skyRoutedb;
        private readonly ILogger<BookingService> _logger;

        public BookingService(SkyRouteDbContext skyRoutedb, ILogger<BookingService> logger)
        {
            _skyRoutedb = skyRoutedb ?? throw new ArgumentNullException(nameof(skyRoutedb));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Booking> BookFlight(BookingRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var flight = await _skyRoutedb.Flights.FindAsync(request.FlightId);


            _logger.LogInformation("Attempting to book flight with ID {FlightId}", request.FlightId);

            if (flight == null)
                throw new InvalidOperationException($"Flight with ID {request.FlightId} not found.");

            var booking = new Booking
            {
                FlightId = request.FlightId,
                ReferenceCode = Guid.NewGuid().ToString()[..8].ToUpper(),
                BookingDate = DateOnly.FromDateTime(DateTime.Now),
                AirLine = request.AirLine,
                Customers = request.customerDetails.Select(cd => new CustomerBookingData
                {
                    FullName = cd.FullName,
                    Email = cd.Email,
                    PhoneNumber = cd.PhoneNumber,
                    DocumentNumber = cd.DocumentNumber,
                }).ToList()
            };

            _logger.LogInformation("Creating booking for flight ID {FlightId} with reference code {ReferenceCode}", request.FlightId, booking.ReferenceCode);

            _skyRoutedb.Bookings.Add(booking);
            await _skyRoutedb.SaveChangesAsync();

            return booking;
        }

        public async Task<object> GetBookingsById(int id)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0);

            var booking = await _skyRoutedb.Bookings
                         .Include(b => b.Customers).FirstOrDefaultAsync(b => b.Id == id);
            _logger.LogInformation("We got the booking details for the ID {id} as {Booking}", id, booking);
            if (booking == null)
            {
                throw new InvalidOperationException("We didn't have any request with the particular id = " + id);
            }

            _logger.LogInformation("Booking found for ID {BookingId}: {BookingDetails}", id, booking);

            var flightdetails = await _skyRoutedb.Flights.FindAsync(booking.FlightId);

            var DepartureAirport = await _skyRoutedb.Airports.FindAsync(flightdetails!.OriginAirportId);

            var ArrivalAirport = await _skyRoutedb.Airports.FindAsync(flightdetails.DestinationAirportId);

            if (flightdetails == null)
            {
                _logger.LogWarning("Flight details not found for booking ID {BookingId}", id);
                return booking;
            }

            var BookingDataWithFlightDetails = new
            {
                booking.Id,
                booking.BookingDate,
                booking.AirLine,
                customers = booking.Customers.Select(c => new
                {
                    c.FullName,
                    c.Email,
                    c.PhoneNumber,
                    c.DocumentNumber
                }),
                Flight = new
                {
                    flightdetails.Id,
                    flightdetails.FlightNumber,
                    DepartureAirport,
                    ArrivalAirport,
                    flightdetails.DepartureTime,
                    flightdetails.ArrivalTime,
                    flightdetails.CabinClass
                }
            };

            _logger.LogInformation("Merged booking and flight data: {MergedData}", BookingDataWithFlightDetails);

            return BookingDataWithFlightDetails;
        }
    }
}
