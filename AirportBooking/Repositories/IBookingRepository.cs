using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetByPassengerIdAsync(string passengerId);
        
        Task<IEnumerable<Booking>> SearchBookingsAsync(
            string? flightId = null,
            string? passengerId = null,
            string? departureCountry = null,
            string? destinationCountry = null,
            DateTime? departureDate = null,
            string? departureAirport = null,
            string? arrivalAirport = null,
            FlightClass? flightClass = null,
            decimal? maxPrice = null);
    }
}
