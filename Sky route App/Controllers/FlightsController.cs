using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sky_route_app.Data;
using sky_route_app.DTOs;
using sky_route_app.Services.Interfaces;

namespace sky_route_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : Controller
    {
        private readonly IFlightService _flightService;
        private readonly ILogger<FlightsController> _logger;

        public FlightsController(IFlightService flightService, ILogger<FlightsController> logger)
        {
            _flightService = flightService ?? throw new ArgumentNullException(nameof(flightService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("airports")]
        public async Task<IActionResult> GetAirports()
        {
            var airports = await _flightService.GetAirports();
            return Ok(airports);
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchFlights([FromBody] FlightSearchRequest request)
        {
           _logger.LogInformation("Received flight search request: {Request}", request);
           var getAvaliableFlights = await _flightService.SearchFlights(request);
           _logger.LogInformation("got the avaliable filghts information.");
           return Ok(getAvaliableFlights);
        }
    }
}
