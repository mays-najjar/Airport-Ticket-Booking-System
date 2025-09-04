using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportBooking.Models
{
    public class BookingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Booking? Booking { get; set; }
    }
}