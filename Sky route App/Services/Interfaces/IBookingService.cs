using Microsoft.AspNetCore.Mvc;
using sky_route_app.DTOs;
using sky_route_app.Models;

namespace sky_route_app.Services.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> BookFlight(BookingRequest request);

        Task<Object> GetBookingsById(int id);
    }
}
