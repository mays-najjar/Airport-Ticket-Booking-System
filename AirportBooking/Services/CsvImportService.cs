using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AirportBooking.Models;
using CsvHelper;
using CsvHelper.Configuration;

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
            var validFlights = new List<Flight>();

            try
            {
                using var reader = new StringReader(csvContent);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null,
                    HeaderValidated = null
                });

                var records = csv.GetRecords<Flight>().ToList();

                for (int i = 0; i < records.Count; i++)
                {
                    var flight = records[i];
                    var validationResults = new List<ValidationResult>();
                    var context = new ValidationContext(flight);

                    if (!Validator.TryValidateObject(flight, context, validationResults, true))
                    {
                        foreach (var error in validationResults)
                            result.Errors.Add($"Line {i + 2}: {error.ErrorMessage}"); 
                        continue;
                    }

                    validFlights.Add(flight);
                    result.SuccessCount++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"CSV parsing error: {ex.Message}");
                return result;
            }

            foreach (var flight in validFlights)
                await _flightService.AddFlightAsync(flight);

            result.ImportedFlights = validFlights;
            return result;
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
