using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.Repositories
{
    public class FlightRepository : IRepository<Flight>
    {
        private readonly string _filePath = "data/flights.json";

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

        public async Task<IEnumerable<Flight>> GetAll()
        {
            return await ReadFlightsAsync();
        }

        public async Task<Flight?> GetById(string id)
        {
            var flights = await ReadFlightsAsync();
            return flights.FirstOrDefault(f => f.Id == id);
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
            var index = flights.FindIndex(f => f.Id == flight.Id);
            if (index >= 0)
            {
                flights[index] = flight;
                await WriteFlightsAsync(flights);
            }
        }

        public async Task DeleteAsync(string id)
        {
            var flights = await ReadFlightsAsync();
            var flightToRemove = flights.FirstOrDefault(f => f.Id == id);
            if (flightToRemove != null)
            {
                flights.Remove(flightToRemove);
                await WriteFlightsAsync(flights);
            }
        }
    }
}