using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.Repositories
{
    public class PassengerRepository : IRepository<Passenger>
    {
        private readonly List<Passenger> _passengers = new();

        public async Task<IEnumerable<Passenger>> GetAll()
        {
            return await Task.FromResult(_passengers);
        }

        public async Task<Passenger?> GetById(string id)
        {
            var passenger = _passengers.FirstOrDefault(p => p.Id == id);
            return await Task.FromResult(passenger);
        }

        public async Task AddAsync(Passenger passenger)
        {
            _passengers.Add(passenger);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Passenger passenger)
        {
            var index = _passengers.FindIndex(p => p.Id == passenger.Id);
            if (index >= 0)
            {
                _passengers[index] = passenger;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string id)
        {
            var passenger = _passengers.FirstOrDefault(p => p.Id == id);
            if (passenger != null)
            {
                _passengers.Remove(passenger);
            }
            await Task.CompletedTask;
        }
    }
}