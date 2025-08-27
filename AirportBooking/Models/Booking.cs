using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AirportBooking.Models
{
    public class Booking
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Passenger ID is required")]
        public string PassengerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Flight ID is required")]
        public string FlightId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Flight class is required")]
        public FlightClass SelectedClass { get; set; }

        [Required(ErrorMessage = "Booking date is required")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Number of seats is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Must book at least one seat")]
        public int NumberOfSeats { get; set; }

        [Required(ErrorMessage = "Total price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0")]
        public decimal TotalPrice { get; set; }

        public bool IsCancelled { get; set; }

        public override string ToString()
        {
            string status = IsCancelled ? "Cancelled" : "Active";
            return $"Booking ID: {Id} | Passenger ID: {PassengerId} | Flight ID: {FlightId} | Class: {SelectedClass} | Seats: {NumberOfSeats} | Total: ${TotalPrice} | Status: {status}";
        }
    }
}