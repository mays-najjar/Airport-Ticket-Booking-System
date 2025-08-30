using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly string _filePath = "data/bookings.json";

        private async Task<List<Booking>> ReadBookingsAsync()
        {
            if (!File.Exists(_filePath))
                return new List<Booking>();

            using var stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<List<Booking>>(stream) ?? new List<Booking>();
        }

        private async Task WriteBookingsAsync(List<Booking> bookings)
        {
            using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, bookings);
        }

        public async Task<IEnumerable<Booking>> GetAll()
        {
            return await ReadBookingsAsync();
        }

        public async Task<Booking?> GetById(string id)
        {
            var bookings = await ReadBookingsAsync();
            return bookings.FirstOrDefault(b => b.BookingId == id);
        }

        public async Task<IEnumerable<Booking>> GetByPassengerIdAsync(string passengerId)
        {
            var bookings = await ReadBookingsAsync();
            return bookings.Where(b => b.PassengerId == passengerId);
        }

        public async Task AddAsync(Booking booking)
        {
            var bookings = await ReadBookingsAsync();
            bookings.Add(booking);
            await WriteBookingsAsync(bookings);
        }

        public async Task UpdateAsync(Booking booking)
        {
            var bookings = await ReadBookingsAsync();
            var index = bookings.FindIndex(b => b.BookingId == booking.BookingId);
            if (index >= 0)
            {
                bookings[index] = booking;
                await WriteBookingsAsync(bookings);
            }
        }

        public async Task DeleteAsync(string id)
        {
            var bookings = await ReadBookingsAsync();
            var bookingToRemove = bookings.FirstOrDefault(b => b.BookingId == id);
            if (bookingToRemove != null)
            {
                bookings.Remove(bookingToRemove);
                await WriteBookingsAsync(bookings);
            }
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
            var bookings = await ReadBookingsAsync();
            
            return bookings.Where(b =>
                (string.IsNullOrEmpty(flightId) || b.FlightId == flightId) &&
                (string.IsNullOrEmpty(passengerId) || b.PassengerId == passengerId) &&
                (!maxPrice.HasValue || b.TotalPrice <= maxPrice.Value) &&
                (!flightClass.HasValue || b.SelectedClass == flightClass.Value) &&
                !b.IsCancelled
            );
        }
    }
}
