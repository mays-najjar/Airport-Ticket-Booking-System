using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirportBooking.Models;
using AirportBooking.Repositories;

namespace AirportBooking.Services
{
    public class FlightService
    {
        private readonly IRepository<Flight> _flightRepository;

        public FlightService(IRepository<Flight> flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<IEnumerable<Flight>> GetAllFlightsAsync()
        {
            return await _flightRepository.GetAll();
        }

        public async Task<Flight?> GetFlightByIdAsync(string id)
        {
            return await _flightRepository.GetById(id);
        }

        public async Task AddFlightAsync(Flight flight)
        {
            await _flightRepository.AddAsync(flight);
        }

        public async Task UpdateFlightAsync(Flight flight)
        {
            await _flightRepository.UpdateAsync(flight);
        }

        public async Task DeleteFlightAsync(string id)
        {
            await _flightRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(
        string? departureCountry = null,
        string? destinationCountry = null,
        DateTime? departureDate = null,
        string? departureAirport = null,
        string? arrivalAirport = null,
        decimal? maxPrice = null,
        FlightClass? flightClass = null)
    {
        var flights = await _flightRepository.GetAll();
        
        return flights.Where(f =>
            (string.IsNullOrEmpty(departureCountry) || f.DepartureCountry.Contains(departureCountry, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(destinationCountry) || f.DestinationCountry.Contains(destinationCountry, StringComparison.OrdinalIgnoreCase)) &&
            (!departureDate.HasValue || f.DepartureDate.Date == departureDate.Value.Date) &&
            (string.IsNullOrEmpty(departureAirport) || f.DepartureAirport.Contains(departureAirport, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(arrivalAirport) || f.ArrivalAirport.Contains(arrivalAirport, StringComparison.OrdinalIgnoreCase)) &&
            (!maxPrice.HasValue || f.Price <= maxPrice.Value) &&
            f.AvailableSeats > 0
        );
    }



        public async Task<bool> IsFlightAvailableAsync(string flightId, int requestedSeats)
        {
            var flight = await GetFlightByIdAsync(flightId);
            return flight != null && flight.AvailableSeats >= requestedSeats;
        }

        public async Task<bool> IsSeatAvailableAsync(string flightId, string seatNumber)
        {
            var flight = await _flightRepository.GetById(flightId);
            if (flight == null) return false;

            return flight.AvailableSeats > 0;
        }

        public async Task<bool> ReserveSeatsAsync(string flightId, int seats)
        {
            var flight = await GetFlightByIdAsync(flightId);
            if (flight == null)
                return false;

            flight.AvailableSeats -= seats;
            await _flightRepository.UpdateAsync(flight);
            return true;
        }

        public async Task ReleaseSeatsAsync(string flightId, int seats)
        {
            var flight = await GetFlightByIdAsync(flightId);
            if (flight == null)
                return;

            flight.AvailableSeats += seats;
            await _flightRepository.UpdateAsync(flight);
        }
    }
}
