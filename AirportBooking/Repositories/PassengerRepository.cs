using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.Repositories
{
    public class PassengerRepository : FileRepository<Passenger>, IPassengerRepository
    {
        public PassengerRepository(string filePath) : base(filePath) { }

        public Task<Passenger?> GetByEmailAsync(string email)
        {
            var passengers = GetAll();
            var trimmedEmail = email.Trim().ToLowerInvariant();

            var result = passengers.FirstOrDefault(p =>
                p.Email?.Trim().ToLowerInvariant() == trimmedEmail);
            
            return Task.FromResult(result);
        }

        public Task<Passenger?> GetByPhoneAsync(string phoneNumber)
        {
            var passengers = GetAll();
            var trimmedPhone = phoneNumber.Trim();

            var result = passengers.FirstOrDefault(p =>
                p.PhoneNumber?.Trim() == trimmedPhone);
            
            return Task.FromResult(result);
        }
    }
}
