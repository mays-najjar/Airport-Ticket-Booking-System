using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AirportBooking.Models
{
    public class Flight
    {
        public string FlightId { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Flight number is required")]
        [StringLength(10, ErrorMessage = "Flight number cannot be longer than 10 characters")]
        public string FlightNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Departure country is required")]
        [StringLength(50, ErrorMessage = "Departure country cannot exceed 50 characters")]
        public string DepartureCountry { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arrival country is required")]
        [StringLength(50, ErrorMessage = "Arrival country cannot exceed 50 characters")]
        public string DestinationCountry { get; set; } = string.Empty;

        [Required(ErrorMessage = "Departure date is required")]
        [DataType(DataType.DateTime)]
        public DateTime DepartureDate { get; set; }

        [Required(ErrorMessage = "Departure airport is required")]
        [StringLength(50, ErrorMessage = "Departure airport cannot exceed 50 characters")]
        public string DepartureAirport { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arrival airport is required")]
        [StringLength(50, ErrorMessage = "Arrival airport cannot exceed 50 characters")]
         public string ArrivalAirport { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
         public decimal Price { get; set; }

        [Required(ErrorMessage = "Available seats is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Available seats must be non-negative")]
        public int AvailableSeats { get; set; }

        public static readonly Dictionary<FlightClass, decimal> ClassPrices = new()
         {
          { FlightClass.Economy, 1.0m },
          { FlightClass.Business, 2.5m },
          { FlightClass.FirstClass, 4.0m }
         };
        public decimal GetPriceForClass(FlightClass flightClass)
        {
            return Price * ClassPrices[flightClass];
        }

        public override string ToString()
        {
            return $"{FlightNumber} - {DepartureCountry} to {DestinationCountry} - {DepartureDate:yyyy-MM-dd HH:mm}";

        }
    }
}