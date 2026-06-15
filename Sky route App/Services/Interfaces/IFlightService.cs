using sky_route_app.DTOs;

namespace sky_route_app.Services.Interfaces
{
    public interface IFlightService
    {
        Task<IEnumerable<object>> SearchFlights(FlightSearchRequest request);
        Task<IEnumerable<object>> GetAirports();
    }
}
