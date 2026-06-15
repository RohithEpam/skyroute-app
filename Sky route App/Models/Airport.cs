using System.ComponentModel.DataAnnotations;

namespace sky_route_app.Models
{
    public class Airport
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Airport code is required")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Code must be between 1 and 10 characters")]
        public required string Code { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "City must be between 1 and 100 characters")]
        public required string City { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Country must be between 1 and 100 characters")]
        public required string Country { get; set; }
    }
}
