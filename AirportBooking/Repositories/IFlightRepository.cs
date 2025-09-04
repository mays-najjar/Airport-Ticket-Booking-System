using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.Repositories
{
    public interface IFlightRepository : IRepository<Flight>
    {
        Task<IEnumerable<Flight>> SearchFlightsAsync(
            string? departureCountry = null,
            string? destinationCountry = null,
            DateTime? departureDate = null,
            string? departureAirport = null,
            string? arrivalAirport = null,
            decimal? maxPrice = null,
            FlightClass? flightClass = null);
            
        Task<bool> IsFlightAvailableAsync(string flightId, int requestedSeats);
        Task<bool> ReserveSeatsAsync(string flightId, int seats);
        Task ReleaseSeatsAsync(string flightId, int seats);
    }
}
