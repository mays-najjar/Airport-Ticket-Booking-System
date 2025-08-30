using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.Repositories
{
    public interface IPassengerRepository : IRepository<Passenger>
    {
        Task<Passenger?> GetByEmailAsync(string email);
        Task<Passenger?> GetByPhoneAsync(string phoneNumber);
    }
}
