using System.ComponentModel.DataAnnotations;

namespace sky_route_app.DTOs
{
    public class BookingRequest
    {
        [Range(1, int.MaxValue)]
        public required int FlightId { get; init; }
        [Required]
        [StringLength(20, MinimumLength = 5)]
        public required string AirLine { get; init; }
        [Required]
        public required List<CustomerDetails> customerDetails { get; init; }
    }

    public class CustomerDetails
    {
        [StringLength(100, MinimumLength = 2)]
        public required string FullName { get; init; }
        [Required]
        [EmailAddress]
        public required string Email { get; init; }
        [Required]
        [StringLength(50, MinimumLength = 5)]
        public required string DocumentNumber { get; init; }
        [Required]
        [StringLength(20, MinimumLength = 5)]
        [Phone]
        public required string PhoneNumber { get; init; }
    }
}
