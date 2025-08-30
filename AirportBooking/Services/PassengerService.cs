using System.Collections.Generic;
using System.Threading.Tasks;
using AirportBooking.Models;
using AirportBooking.Repositories;


namespace AirportBooking.Services
{
    public class PassengerService
    {
        private readonly IPassengerRepository _passengerRepository;

        public PassengerService(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;
        }

        public async Task<IEnumerable<Passenger>> GetAllPassengersAsync()
        {
            return await _passengerRepository.GetAll();
        }

        public async Task<Passenger?> GetPassengerByIdAsync(string id)
        {
            return await _passengerRepository.GetById(id);
        }

        public async Task<Passenger?> GetPassengerByEmailAsync(string email)
        {
            return await _passengerRepository.GetByEmailAsync(email);
        }

        public async Task<Passenger?> GetPassengerByPhoneAsync(string phoneNumber)
        {
            return await _passengerRepository.GetByPhoneAsync(phoneNumber);
        }

        public async Task AddPassengerAsync(Passenger passenger)
        {
            await _passengerRepository.AddAsync(passenger);
        }

        public async Task UpdatePassengerAsync(Passenger passenger)
        {
            await _passengerRepository.UpdateAsync(passenger);
        }


        public async Task DeletePassengerAsync(string id)
        {
            await _passengerRepository.DeleteAsync(id);
        }
    }
}
