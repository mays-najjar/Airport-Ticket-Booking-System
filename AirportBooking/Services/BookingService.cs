using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirportBooking.Models;
using AirportBooking.Repositories;

namespace AirportBooking.Services
{
    public class BookingService
    {
        private readonly IRepository<Booking> _bookingRepository;
        private readonly FlightService _flightService;
        private readonly PassengerService _passengerService;

        public BookingService(
            IRepository<Booking> bookingRepository,
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
            var bookings = await _bookingRepository.GetAll();
            var flights = await _flightService.GetAllFlightsAsync();

            return bookings.Where(b =>
                !b.IsCancelled &&
                (string.IsNullOrEmpty(flightId) || b.FlightId == flightId) &&
                (string.IsNullOrEmpty(passengerId) || b.PassengerId == passengerId) &&
                (!maxPrice.HasValue || b.TotalPrice <= maxPrice.Value)
            ).Where(b =>
            {
                var flight = flights.FirstOrDefault(f => f.Id == b.FlightId);
                if (flight == null) return false;

                return (string.IsNullOrEmpty(departureCountry) || flight.DepartureCountry.Contains(departureCountry, StringComparison.OrdinalIgnoreCase)) &&
                       (string.IsNullOrEmpty(destinationCountry) || flight.DestinationCountry.Contains(destinationCountry, StringComparison.OrdinalIgnoreCase)) &&
                       (!departureDate.HasValue || flight.DepartureDate.Date == departureDate.Value.Date) &&
                       (string.IsNullOrEmpty(departureAirport) || flight.DepartureAirport.Contains(departureAirport, StringComparison.OrdinalIgnoreCase)) &&
                       (string.IsNullOrEmpty(arrivalAirport) || flight.ArrivalAirport.Contains(arrivalAirport, StringComparison.OrdinalIgnoreCase)) &&
                       (!flightClass.HasValue || b.SelectedClass == flightClass.Value);
            });
        }

           public async Task<BookingResult> CreateBookingAsync(string flightId, string passengerId, FlightClass flightClass, int numberOfSeats)
        {
            var flight = await _flightService.GetFlightByIdAsync(flightId);
            var passenger = await _passengerService.GetPassengerByIdAsync(passengerId);

            if (flight == null)
                return new BookingResult { Success = false, Message = "Flight not found" };

            if (passenger == null)
                return new BookingResult { Success = false, Message = "Passenger not found" };

            if (!await _flightService.IsFlightAvailableAsync(flightId, numberOfSeats))
                return new BookingResult { Success = false, Message = "Not enough seats available" };

            var totalPrice = flight.GetPriceForClass(flightClass) * numberOfSeats;

            var booking = new Booking
            {
                FlightId = flightId,
                PassengerId = passengerId,
                SelectedClass = flightClass,
                NumberOfSeats = numberOfSeats,
                TotalPrice = totalPrice
            };

            if (!await _flightService.ReserveSeatsAsync(flightId, numberOfSeats))
                return new BookingResult { Success = false, Message = "Failed to reserve seats" };

            await _bookingRepository.AddAsync(booking);
            return new BookingResult { Success = true, Message = "Booking created successfully", Booking = booking };
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

         public async Task<BookingResult> ModifyBookingAsync(string bookingId, FlightClass newClass, int newNumberOfSeats)
        {
            var booking = await GetBookingByIdAsync(bookingId);
            if (booking == null || booking.IsCancelled)
                return new BookingResult { Success = false, Message = "Booking not found or cancelled" };

            var flight = await _flightService.GetFlightByIdAsync(booking.FlightId);
            if (flight == null)
                return new BookingResult { Success = false, Message = "Flight not found" };

            await _flightService.ReleaseSeatsAsync(booking.FlightId, booking.NumberOfSeats);

            if (!await _flightService.IsFlightAvailableAsync(booking.FlightId, newNumberOfSeats))
            {
                await _flightService.ReserveSeatsAsync(booking.FlightId, booking.NumberOfSeats);
                return new BookingResult { Success = false, Message = "Not enough seats available for modification" };
            }
            if (!await _flightService.ReserveSeatsAsync(booking.FlightId, newNumberOfSeats))
            {
                await _flightService.ReserveSeatsAsync(booking.FlightId, booking.NumberOfSeats);
                return new BookingResult { Success = false, Message = "Failed to reserve new seats" };
            }
            booking.SelectedClass = newClass;
            booking.NumberOfSeats = newNumberOfSeats;
            booking.TotalPrice = flight.GetPriceForClass(newClass) * newNumberOfSeats;
            await _bookingRepository.UpdateAsync(booking);
            return new BookingResult { Success = true, Message = "Booking modified successfully", Booking = booking };
        }

        public class BookingResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public Booking? Booking { get; set; }
        }
    }
}