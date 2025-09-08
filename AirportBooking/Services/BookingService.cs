using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirportBooking.Models;
using AirportBooking.Repositories;
using AirportBooking.UI;

namespace AirportBooking.Services
{
    public class BookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly FlightService _flightService;
        private readonly PassengerService _passengerService;


        public BookingService(
            IBookingRepository bookingRepository,
            FlightService flightService,
            PassengerService passengerService)
        {
            _bookingRepository = bookingRepository;
            _flightService = flightService;
            _passengerService = passengerService;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _bookingRepository.GetAll();
        }

        public async Task<Booking?> GetBookingByIdAsync(string id)
        {
            return await _bookingRepository.GetById(id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByPassengerIdAsync(string passengerId)
        {
            var bookings = await _bookingRepository.GetAll();
            return bookings.Where(b => b.PassengerId == passengerId);
        }

        public async Task<IEnumerable<Booking>> SearchBookingsAsync(
            string? flightId = null,
            string? passengerId = null,
            string? departureCountry = null,
            string? destinationCountry = null,
            DateTime? departureDate = null,
            string? departureAirport = null,
            string? arrivalAirport = null,
            FlightClass? flightClass = null,
            decimal? maxPrice = null)
        {
            return await _bookingRepository.SearchBookingsAsync(
            flightId,
            passengerId,
            departureCountry,
            destinationCountry,
            departureDate,
            departureAirport,
            arrivalAirport,
            flightClass,
            maxPrice
        );
        }


        public async Task<Booking> CreateBookingAsync(string passengerEmail, string flightId, string classInputStr, int numberOfSeats)
        {
            var passenger = await GetValidatedPassengerAsync(passengerEmail);
            var flightClass = ParseFlightClass(classInputStr);
            ValidateNumberOfSeats(numberOfSeats);
            var flight = await GetValidatedFlightAsync(flightId);
            await ValidateAvailabilityAsync(flightId, numberOfSeats);
            var totalPrice = CalculateTotalPrice(flight, flightClass, numberOfSeats);
            var booking = new Booking
            {
                FlightId = flightId,
                PassengerId = passenger.PassengerId,
                SelectedClass = flightClass,
                NumberOfSeats = numberOfSeats,
                TotalPrice = totalPrice
            };
            await ReserveSeatsAsync(flightId, numberOfSeats);
            try
            {
                await _bookingRepository.AddAsync(booking);
                return booking;
            }
            catch (Exception)
            {
                await _flightService.ReleaseSeatsAsync(flightId, numberOfSeats);
                throw new InvalidOperationException("Failed to create booking due to an unexpected error. Seats have been released.");
            }
        }

        public async Task<bool> CancelBookingAsync(string bookingId)
        {
            var booking = await GetBookingByIdAsync(bookingId);
            if (booking == null || booking.IsCancelled)
                return false;

            booking.IsCancelled = true;
            await _bookingRepository.UpdateAsync(booking);
            await _flightService.ReleaseSeatsAsync(booking.FlightId, booking.NumberOfSeats);
            return true;
        }

        public async Task<IEnumerable<Booking>> ManageBookingsAsync(string passengerEmail)
        {
            var passenger = await _passengerService.GetPassengerByEmailAsync(passengerEmail);
            if (passenger == null)
            {
                throw new InvalidOperationException("Passenger not found. Please register before booking a flight.");
            }

            var bookings = await GetBookingsByPassengerIdAsync(passenger.PassengerId);
            return bookings;
        }

        public async Task<BookingResult> ModifyBookingAsync(string bookingId, FlightClass newClass, int newNumberOfSeats)
        {
            var booking = await GetBookingByIdAsync(bookingId);
            if (booking == null || booking.IsCancelled)
                return new BookingResult { Success = false, Message = "Booking not found or cancelled" };

            var flight = await _flightService.GetFlightByIdAsync(booking.FlightId);
            if (flight == null)
                return new BookingResult { Success = false, Message = "Flight not found" };

            if (!await _flightService.IsFlightAvailableAsync(booking.FlightId, newNumberOfSeats - booking.NumberOfSeats))
            {
                return new BookingResult { Success = false, Message = "Not enough seats available for modification" };
            }

            if (newNumberOfSeats > booking.NumberOfSeats)
            {
                var additionalSeats = newNumberOfSeats - booking.NumberOfSeats;
                if (!await _flightService.ReserveSeatsAsync(booking.FlightId, additionalSeats))
                {
                    return new BookingResult { Success = false, Message = "Failed to reserve additional seats" };
                }
            }

            booking.SelectedClass = newClass;
            var oldSeats = booking.NumberOfSeats;
            booking.NumberOfSeats = newNumberOfSeats;
            booking.TotalPrice = flight.GetPriceForClass(newClass) * newNumberOfSeats;

            await _bookingRepository.UpdateAsync(booking);

            if (newNumberOfSeats < oldSeats)
            {
                var seatsToRelease = oldSeats - newNumberOfSeats;
                await _flightService.ReleaseSeatsAsync(booking.FlightId, seatsToRelease);
            }

            return new BookingResult { Success = true, Message = "Booking modified successfully", Booking = booking };
        }

        private async Task<Passenger> GetValidatedPassengerAsync(string passengerEmail)
        {
            var passenger = await _passengerService.GetPassengerByEmailAsync(passengerEmail);
            if (passenger == null)
            {
                throw new InvalidOperationException("Passenger not found. Please enter your details to register.");
            }
            return passenger;
        }

        private FlightClass ParseFlightClass(string classInputStr)
        {
            if (!Enum.TryParse<FlightClass>(classInputStr, true, out var flightClass))
            {
                throw new ArgumentException("Invalid class selected.");
            }
            return flightClass;
        }

        private void ValidateNumberOfSeats(int numberOfSeats)
        {
            if (numberOfSeats <= 0)
            {
                throw new ArgumentException("Number of seats must be greater than zero.");
            }
        }

        private async Task<Flight> GetValidatedFlightAsync(string flightId)
        {
            var flight = await _flightService.GetFlightByIdAsync(flightId);
            if (flight == null)
            {
                throw new ArgumentException($"Flight with ID '{flightId}' not found.");
            }
            return flight;
        }

        private async Task ValidateAvailabilityAsync(string flightId, int numberOfSeats)
        {
            if (!await _flightService.IsFlightAvailableAsync(flightId, numberOfSeats))
            {
                throw new InvalidOperationException("Not enough seats available.");
            }
        }

        private decimal CalculateTotalPrice(Flight flight, FlightClass flightClass, int numberOfSeats)
        {
            return flight.GetPriceForClass(flightClass) * numberOfSeats;
        }

        private async Task ReserveSeatsAsync(string flightId, int numberOfSeats)
        {
            if (!await _flightService.ReserveSeatsAsync(flightId, numberOfSeats))
            {
                throw new InvalidOperationException("Failed to reserve seats.");
            }
        }
    }
}