using System.ComponentModel.DataAnnotations;

namespace sky_route_app.DTOs
{
    public class FlightSearchRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public required int OriginAirportId { get; init; }
        [Required]
        [Range(1, int.MaxValue)]
        public required int DestinationAirportId { get; init; }
        [Required]
        [DataType(DataType.Date)]
        public required DateTime DepartureDate { get; init; }
        [Required]
        [Range (1, 10)]
        public required int Passengers { get; init; }
        [Required]
        public required string CabinClass { get; init; } = "";
    }
}
