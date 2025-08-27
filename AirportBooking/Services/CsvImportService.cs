using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.Services
{
    public class CsvImportService
    {

        private readonly FlightService _flightService;

        public CsvImportService(FlightService flightService)
        {
            _flightService = flightService;
        }

        public async Task<CsvImportResult> ImportFlightsFromCsvAsync(string csvContent)
        {
            var result = new CsvImportResult();
            var flights = new List<Flight>();
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
            {
                result.Errors.Add("CSV file must contain at least a header row and one data row");
                return result;
            }

            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            var expectedHeaders = new[]
            {
            "FlightNumber", "DepartureCountry", "DestinationCountry", "DepartureDate",
            "DepartureAirport", "ArrivalAirport", "Price", "AvailableSeats"
        };

            // Validate headers
            if (!headers.SequenceEqual(expectedHeaders, StringComparer.OrdinalIgnoreCase))
            {
                result.Errors.Add($"Invalid headers. Expected: {string.Join(", ", expectedHeaders)}");
                return result;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = ParseCsvLine(line);
                if (values.Length != headers.Length)
                {
                    result.Errors.Add($"Line {i + 1}: Invalid number of columns");
                    continue;
                }

                try
                {
                    var flight = new Flight();
                    var validationErrors = new List<string>();

                    // Flight Number
                    flight.FlightNumber = values[0].Trim();
                    if (string.IsNullOrWhiteSpace(flight.FlightNumber))
                        validationErrors.Add("Flight number is required");

                    // Departure Country
                    flight.DepartureCountry = values[1].Trim();
                    if (string.IsNullOrWhiteSpace(flight.DepartureCountry))
                        validationErrors.Add("Departure country is required");

                    // Destination Country
                    flight.DestinationCountry = values[2].Trim();
                    if (string.IsNullOrWhiteSpace(flight.DestinationCountry))
                        validationErrors.Add("Destination country is required");

                    // Departure Date
                    if (!DateTime.TryParseExact(values[3].Trim(), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var departureDate))
                        validationErrors.Add("Invalid departure date format. Use yyyy-MM-dd HH:mm");
                    else
                        flight.DepartureDate = departureDate;

                    // Departure Airport
                    flight.DepartureAirport = values[4].Trim();
                    if (string.IsNullOrWhiteSpace(flight.DepartureAirport))
                        validationErrors.Add("Departure airport is required");

                    // Arrival Airport
                    flight.ArrivalAirport = values[5].Trim();
                    if (string.IsNullOrWhiteSpace(flight.ArrivalAirport))
                        validationErrors.Add("Arrival airport is required");

                    // Price
                    if (!decimal.TryParse(values[6].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price) || price <= 0)
                        validationErrors.Add("Price must be a positive number");
                    else
                        flight.Price = price;

                    // Available Seats
                    if (!int.TryParse(values[7].Trim(), out var availableSeats) || availableSeats < 0)
                        validationErrors.Add("Available seats must be a non-negative integer");
                    else
                        flight.AvailableSeats = availableSeats;

                    if (validationErrors.Any())
                    {
                        result.Errors.Add($"Line {i + 1}: {string.Join("; ", validationErrors)}");
                        continue;
                    }

                    flights.Add(flight);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Line {i + 1}: {ex.Message}");
                }
            }

            // Save valid flights
            foreach (var flight in flights)
            {
                await _flightService.AddFlightAsync(flight);
            }

            result.ImportedFlights = flights;
            return result;
        }

        private static string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            values.Add(current.ToString());
            return values.ToArray();
        }

        public static string GetValidationDetails()
        {
            var details = new StringBuilder();
            details.AppendLine("Flight Data Model Validation Details:");
            details.AppendLine();
            details.AppendLine("Departure Country:");
            details.AppendLine("  Type: Free Text");
            details.AppendLine("  Constraint: Required, Max 50 characters");
            details.AppendLine();
            details.AppendLine("Destination Country:");
            details.AppendLine("  Type: Free Text");
            details.AppendLine("  Constraint: Required, Max 50 characters");
            details.AppendLine();
            details.AppendLine("Departure Date:");
            details.AppendLine("  Type: Date Time");
            details.AppendLine("  Constraint: Required, Format: yyyy-MM-dd HH:mm, Must be today or future");
            details.AppendLine();
            details.AppendLine("Price:");
            details.AppendLine("  Type: Decimal");
            details.AppendLine("  Constraint: Required, Must be greater than 0");
            details.AppendLine();
            details.AppendLine("Available Seats:");
            details.AppendLine("  Type: Integer");
            details.AppendLine("  Constraint: Required, Must be non-negative");

            return details.ToString();
        }
    }

    public class CsvImportResult
    {
        public int SuccessCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<Flight> ImportedFlights { get; set; } = new();
        public bool IsSuccess => Errors.Count == 0;
    }
}