using System.ComponentModel.DataAnnotations;

namespace sky_route_app.Models
{ 
    public class Booking
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Flight ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Flight ID must be a valid positive number")]
        public required int FlightId { get; set; }
   
        [StringLength(20)]
        public required string ReferenceCode { get; set; }
        
        
        [Required(ErrorMessage = "Booking date is required")]
        public required DateOnly BookingDate { get; set; }
        
        [Required(ErrorMessage = "Airline is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Airline name must be between 2 and 100 characters")]
        public required string AirLine { get; set; }

        [Required]
        public required List<CustomerBookingData> Customers { get; set; } = new List<CustomerBookingData>();
    }

    public class CustomerBookingData
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public required string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Document number is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Document number must be between 5 and 50 characters")]
        public required string DocumentNumber { get; set; }
    }
}
