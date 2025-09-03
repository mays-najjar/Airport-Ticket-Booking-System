
using AirportBooking.Models;
using AirportBooking.Repositories;
using AirportBooking.Services;
using AirportBooking.UI;

namespace Airport_Ticket_Booking;

class Program
{
    private static FlightService _flightService = null!;
    private static PassengerService _passengerService = null!;
    private static BookingService _bookingService = null!;
    private static CsvImportService _csvImportService = null!;

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        InitializeServices();
        await ShowWelcomeScreen();
    }

    private static void InitializeServices()
    {
        var flightRepository = new FlightRepository();
        var passengerRepository = new PassengerRepository();
        var bookingRepository = new BookingRepository(flightRepository);

        _flightService = new FlightService(flightRepository);
        _passengerService = new PassengerService(passengerRepository);
        _bookingService = new BookingService(bookingRepository, _flightService, _passengerService);
        _csvImportService = new CsvImportService(_flightService);
    }

    private static async Task ShowWelcomeScreen()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Airport Ticket Booking System");
            Console.WriteLine("Welcome to the Airport Ticket Booking System!");
            Console.WriteLine();
            Console.WriteLine("Please select your role:");
            Console.WriteLine();
            Console.WriteLine("1. Passenger");
            Console.WriteLine("2. Manager");
            Console.WriteLine("3. Exit");
            Console.WriteLine();

            var choice = ConsoleHelper.GetMenuChoice(3);

            switch (choice)
            {
                case 1:
                    await ShowPassengerMenu();
                    break;
                case 2:
                    await ShowManagerMenu();
                    break;
                case 3:
                    ConsoleHelper.PrintInfo("Thank you for using the Airport Ticket Booking System. Goodbye!");
                    return;
            }
        }
    }

    private static async Task ShowPassengerMenu()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Passenger Menu");
            ConsoleHelper.DisplayMenu(new[]
            {
                "Search Flights",
                "Book a Flight",
                "Manage Bookings",
                "Back to Main Menu"
            });

            var choice = ConsoleHelper.GetMenuChoice(4);

            switch (choice)
            {
                case 1:
                    await SearchFlights();
                    break;
                case 2:
                    await BookFlight();
                    break;
                case 3:
                    await ManageBookings();
                    break;
                case 4:
                    return;
            }
        }
    }

    private static async Task ShowManagerMenu()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Manager Menu");
            ConsoleHelper.DisplayMenu(new[]
            {
                "Filter Bookings",
                "Batch Flight Upload (CSV)",
                "View Flight Data Validation Details",
                "Back to Main Menu"
            });

            var choice = ConsoleHelper.GetMenuChoice(4);

            switch (choice)
            {
                case 1:
                    await FilterBookings();
                    break;
                case 2:
                    await BatchFlightUpload();
                    break;
                case 3:
                    ViewValidationDetails();
                    break;
                case 4:
                    return;
            }
        }
    }

    private static async Task SearchFlights()
    {
        ConsoleHelper.PrintHeader("Search Flights");

        var departureCountry = ConsoleHelper.GetStringInput("Departure Country (leave blank to skip): ", false);
        var destinationCountry = ConsoleHelper.GetStringInput("Destination Country (leave blank to skip): ", false);
        var departureDate = ConsoleHelper.GetDateTimeInput("Departure Date (yyyy-MM-dd HH:mm): ");
        var maxPrice = ConsoleHelper.GetDecimalInput("Max Price (leave blank to skip): ", 0);

        var flights = await _flightService.SearchFlightsAsync(
            departureCountry: string.IsNullOrWhiteSpace(departureCountry) ? null : departureCountry,
            destinationCountry: string.IsNullOrWhiteSpace(destinationCountry) ? null : destinationCountry,
            departureDate: departureDate,
            maxPrice: maxPrice == 0 ? null : maxPrice
        );

        if (!flights.Any())
        {
            ConsoleHelper.PrintWarning("No flights found matching your criteria.");
            ConsoleHelper.Pause();
            return;
        }

        ConsoleHelper.PrintSuccess($"Found {flights.Count()} flights:");
        foreach (var flight in flights)
        {
            Console.WriteLine($"{flight.FlightId}: {flight}");
        }

        ConsoleHelper.Pause();
    }


    private static async Task BookFlight()
    {
        ConsoleHelper.PrintHeader("Book a Flight");

        var passengerEmail = ConsoleHelper.GetStringInput("Enter your email: ");
        var flightId = ConsoleHelper.GetStringInput("Enter Flight ID to book: ");
        var classInputStr = ConsoleHelper.GetStringInput("Select Class (Economy, Business, FirstClass): ");
         var seats = ConsoleHelper.GetIntInput("Number of seats to book: ", 1, int.MaxValue);

        try
        {
            var booking = await _bookingService.CreateBookingAsync(passengerEmail, flightId, classInputStr, seats);
            ConsoleHelper.PrintSuccess("Booking created successfully!");
        }
        catch (ArgumentException ex)
        {
            ConsoleHelper.PrintError($"Input Error: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            ConsoleHelper.PrintError($"Booking Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            ConsoleHelper.Pause();
        }
    }

   public static async Task ManageBookings()
    {
        ConsoleHelper.PrintHeader("Manage Bookings");

        var passengerEmail = ConsoleHelper.GetStringInput("Enter your email: ");

        try
        {
            var bookings = await _bookingService.ManageBookingsAsync(passengerEmail);

            if (!bookings.Any())
            {
                ConsoleHelper.PrintWarning("No bookings found.");
                return;
            }

            ConsoleHelper.PrintSuccess($"Found {bookings.Count()} bookings:");
            foreach (var booking in bookings)
            {
                Console.WriteLine($"{booking.BookingId}: Flight ID {booking.FlightId}, Seats: {booking.NumberOfSeats}");
            }
        }
        catch (ArgumentException ex)
        {
            ConsoleHelper.PrintError($"Input Error: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            ConsoleHelper.PrintError($"Booking Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            ConsoleHelper.Pause();
        }
    }
    private static async Task FilterBookings()
    {
        ConsoleHelper.PrintHeader("Filter Bookings");

        var flightId = ConsoleHelper.GetStringInput("Flight ID (leave blank to skip): ", false);
        var passengerId = ConsoleHelper.GetStringInput("Passenger ID (leave blank to skip): ", false);
        var departureCountry = ConsoleHelper.GetStringInput("Departure Country (leave blank to skip): ", false);
        var destinationCountry = ConsoleHelper.GetStringInput("Destination Country (leave blank to skip): ", false);
        var departureDate = ConsoleHelper.GetDateTimeInput("Departure Date (yyyy-MM-dd HH:mm): ");
        var maxPrice = ConsoleHelper.GetDecimalInput("Max Price (leave blank to skip): ", 0);

        var bookings = await _bookingService.SearchBookingsAsync(
            flightId: string.IsNullOrWhiteSpace(flightId) ? null : flightId,
            passengerId: string.IsNullOrWhiteSpace(passengerId) ? null : passengerId,
            departureCountry: string.IsNullOrWhiteSpace(departureCountry) ? null : departureCountry,
            destinationCountry: string.IsNullOrWhiteSpace(destinationCountry) ? null : destinationCountry,
            departureDate: departureDate,
            maxPrice: maxPrice == 0 ? null : maxPrice
        );

        if (!bookings.Any())
        {
            ConsoleHelper.PrintWarning("No bookings found matching the criteria.");
            ConsoleHelper.Pause();
            return;
        }

        ConsoleHelper.PrintSuccess($"Found {bookings.Count()} bookings:");
        foreach (var booking in bookings)
        {
            Console.WriteLine($"{booking.BookingId}: {booking}");
        }

        ConsoleHelper.Pause();
    }

    private static async Task BatchFlightUpload()
    {
        ConsoleHelper.PrintHeader("Batch Flight Upload (CSV)");

        ConsoleHelper.PrintInfo("Please enter the full path to the CSV file:");
        var filePath = ConsoleHelper.GetStringInput("CSV file path: ");

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            ConsoleHelper.PrintError("File not found.");
            ConsoleHelper.Pause();
            return;
        }

        var csvContent = await File.ReadAllTextAsync(filePath);
        var result = await _csvImportService.ImportFlightsFromCsvAsync(csvContent);

        if (result.IsSuccess)
        {
            ConsoleHelper.PrintSuccess($"Successfully imported {result.SuccessCount} flights.");
        }
        else
        {
            ConsoleHelper.PrintError("Errors occurred during import:");
            foreach (var error in result.Errors)
            {
                ConsoleHelper.PrintError(error);
            }
        }

        ConsoleHelper.Pause();
    }

    private static void ViewValidationDetails()
    {
        ConsoleHelper.PrintHeader("Flight Data Validation Details");
        Console.WriteLine("Flight Data Model Validation Details:");
        Console.WriteLine();
        Console.WriteLine("FlightNumber: Required, max 50 characters");
        Console.WriteLine("DepartureCountry: Required, max 50 characters");
        Console.WriteLine("DestinationCountry: Required, max 50 characters");
        Console.WriteLine("DepartureDate: Required, format yyyy-MM-dd HH:mm, must be today or future");
        Console.WriteLine("DepartureAirport: Required, max 50 characters");
        Console.WriteLine("ArrivalAirport: Required, max 50 characters");
        Console.WriteLine("Price: Required, positive decimal");
        Console.WriteLine("AvailableSeats: Required, non-negative integer");
        Console.WriteLine();
        ConsoleHelper.Pause();
    }
}
