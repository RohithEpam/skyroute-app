using System.ComponentModel.DataAnnotations;

namespace sky_route_app.Models
{
    public class Flight
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Provider is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Provider must be between 2 and 100 characters")]
        public required string Provider { get; set; }

        [Required(ErrorMessage = "Flight number is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Flight number must be between 2 and 20 characters")]
        [RegularExpression(@"^[A-Z0-9]{2,20}$", ErrorMessage = "Flight number must contain only uppercase letters and numbers")]
        public required string FlightNumber { get; set; }

        [Required(ErrorMessage = "Origin airport is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Origin airport ID must be a positive number")]
        public required int OriginAirportId { get; set; }

        [Required(ErrorMessage = "Destination airport is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Destination airport ID must be a positive number")]
        public required int DestinationAirportId { get; set; }

        [Required(ErrorMessage = "Departure time is required")]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Arrival time is required")]
        public DateTime ArrivalTime { get; set; }

        [Required(ErrorMessage = "Cabin class is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Cabin class must be between 2 and 50 characters")]
        public required string CabinClass { get; set; }

        [Required(ErrorMessage = "Base fare is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Base fare must be greater than 0")]
        public decimal BaseFare { get; set; }
    }
}
