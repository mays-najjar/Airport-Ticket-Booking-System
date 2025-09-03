using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly string _filePath = "data/flights.json";

        public async Task<IEnumerable<Flight>> GetAll()
        {
            return await ReadFlightsAsync();
        }

        public async Task<Flight?> GetById(string id)
        {
            var flights = await ReadFlightsAsync();
            return flights.FirstOrDefault(f => f.FlightId == id);
        }

        public async Task AddAsync(Flight flight)
        {
            var flights = await ReadFlightsAsync();
            flights.Add(flight);
            await WriteFlightsAsync(flights);
        }

        public async Task UpdateAsync(Flight flight)
        {
            var flights = await ReadFlightsAsync();
            var index = flights.FindIndex(f => f.FlightId == flight.FlightId);
            if (index >= 0)
            {
                flights[index] = flight;
                await WriteFlightsAsync(flights);
            }
        }

        public async Task DeleteAsync(string id)
        {
            var flights = await ReadFlightsAsync();
            var flightToRemove = flights.FirstOrDefault(f => f.FlightId == id);
            if (flightToRemove != null)
            {
                flights.Remove(flightToRemove);
                await WriteFlightsAsync(flights);
            }
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
            var flights = await ReadFlightsAsync();

            return flights.Where(f =>
                (string.IsNullOrEmpty(departureCountry) || f.DepartureCountry.Contains(departureCountry, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(destinationCountry) || f.DestinationCountry.Contains(destinationCountry, StringComparison.OrdinalIgnoreCase)) &&
                (!departureDate.HasValue || f.DepartureDate.Date == departureDate.Value.Date) &&
                (string.IsNullOrEmpty(departureAirport) || f.DepartureAirport.Contains(departureAirport, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(arrivalAirport) || f.ArrivalAirport.Contains(arrivalAirport, StringComparison.OrdinalIgnoreCase)) &&
                (!maxPrice.HasValue || f.Price <= maxPrice.Value) &&
                (!flightClass.HasValue || f.AvailableSeats > 0)
            );
        }

        public async Task<bool> IsFlightAvailableAsync(string flightId, int requestedSeats)
        {
            var flight = await GetById(flightId);
            return flight != null && flight.AvailableSeats >= requestedSeats;
        }

        public async Task<bool> ReserveSeatsAsync(string flightId, int seats)
        {
            var flight = await GetById(flightId);
            if (flight == null || flight.AvailableSeats < seats)
                return false;

            flight.AvailableSeats -= seats;
            await UpdateAsync(flight);
            return true;
        }

        public async Task ReleaseSeatsAsync(string flightId, int seats)
        {
            var flight = await GetById(flightId);
            if (flight == null)
                return;

            flight.AvailableSeats += seats;
            await UpdateAsync(flight);
        }
        
         private async Task<List<Flight>> ReadFlightsAsync()
        {
            if (!File.Exists(_filePath))
                return new List<Flight>();

            using var stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<List<Flight>>(stream) ?? new List<Flight>();
        }

        private async Task WriteFlightsAsync(List<Flight> flights)
        {
            using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, flights);
        }
    }
}
