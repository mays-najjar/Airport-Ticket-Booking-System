using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.Repositories
{
    public class PassengerRepository : IPassengerRepository
    {
        private readonly string _filePath = "data/passenger.json";



        public async Task<IEnumerable<Passenger>> GetAll()
        {
            return await ReadPassengersAsync();
        }

        public async Task<Passenger?> GetById(string id)
        {
            var passengers = await ReadPassengersAsync();
            return passengers.FirstOrDefault(p => p.PassengerId == id);
        }

        public async Task AddAsync(Passenger passenger)
        {
            var passengers = await ReadPassengersAsync();
            passengers.Add(passenger);
            await WritePassengersAsync(passengers);
        }

        public async Task UpdateAsync(Passenger passenger)
        {
            var passengers = await ReadPassengersAsync();
            var index = passengers.FindIndex(p => p.PassengerId == passenger.PassengerId);
            if (index >= 0)
            {
                passengers[index] = passenger;
                await WritePassengersAsync(passengers);
            }
        }

        public async Task DeleteAsync(string id)
        {
            var passengers = await ReadPassengersAsync();
            var passenger = passengers.FirstOrDefault(p => p.PassengerId == id);
            if (passenger != null)
            {
                passengers.Remove(passenger);
                await WritePassengersAsync(passengers);
            }
        }

        public async Task<Passenger?> GetByEmailAsync(string email)
        {
            var passengers = await GetAll();
            var trimmedEmail = email.Trim().ToLowerInvariant();

            var result = passengers.FirstOrDefault(p =>
                p.Email?.Trim().ToLowerInvariant() == trimmedEmail);

            return result;
        }

        public async Task<Passenger?> GetByPhoneAsync(string phoneNumber)
        {
            var passengers = await GetAll();
            var trimmedPhone = phoneNumber.Trim();

            var result = passengers.FirstOrDefault(p =>
                p.PhoneNumber?.Trim() == trimmedPhone);

            return result;
        }
        
        private async Task<List<Passenger>> ReadPassengersAsync()
        {
            if (!File.Exists(_filePath))
                return new List<Passenger>();

            using var stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<List<Passenger>>(stream) ?? new List<Passenger>();
        }

        private async Task WritePassengersAsync(List<Passenger> passengers)
        {
            using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, passengers, new JsonSerializerOptions { WriteIndented = true });
        }

    }
}
