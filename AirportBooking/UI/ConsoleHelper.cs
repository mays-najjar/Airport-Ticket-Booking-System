using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AirportBooking.Models;

namespace AirportBooking.UI
{
    public class ConsoleHelper
    {
        public static void PrintHeader(string title)
        {
            Console.Clear();
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();
        }

        public static void PrintSeparator()
        {
            Console.WriteLine(new string('-', 60));
        }

        public static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }

        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }

        public static void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {message}");
            Console.ResetColor();
        }

        public static void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ℹ {message}");
            Console.ResetColor();
        }

        public static string GetStringInput(string prompt, bool required = true)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine()?.Trim() ?? string.Empty;

                if (required && string.IsNullOrWhiteSpace(input))
                {
                    PrintError("This field is required. Please try again.");
                    continue;
                }

                return input;
            }
        }

        public static int GetIntInput(string prompt, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out var value) && value >= min && value <= max)
                {
                    return value;
                }

                PrintError($"Please enter a valid number between {min} and {max}.");
            }
        }

        public static decimal GetDecimalInput(string prompt, decimal min = decimal.MinValue)
        {
            while (true)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) && value >= min)
                {
                    return value;
                }

                PrintError($"Please enter a valid number greater than or equal to {min}.");
            }
        }

        public static DateTime GetDateTimeInput(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (yyyy-MM-dd HH:mm): ");
                if (DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date;
                }

                PrintError("Invalid date format. Please use yyyy-MM-dd HH:mm");
            }
        }

        public static bool GetYesNoInput(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (y/n): ");
                var input = Console.ReadLine()?.Trim().ToLower();
                switch (input)
                {
                    case "y":
                    case "yes":
                        return true;
                    case "n":
                    case "no":
                        return false;
                    default:
                        PrintError("Please enter 'y' or 'n'.");
                        break;
                }
            }
        }

        public static void Pause()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static void DisplayMenu(string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {options[i]}");
            }
        }
        public static void PrintBookingTable(IEnumerable<Booking> bookings)
        {
            PrintSeparator();
            Console.WriteLine($"{"ID",-8} {"Flight",-10} {"Passenger",-15} {"Class",-10} {"Seats",-6} {"Total",-10} {"Date",-20}");
            PrintSeparator();
            foreach (var b in bookings)
            {
                Console.WriteLine($"{b.BookingId,-8} {b.FlightId,-10} {b.PassengerId,-15} {b.SelectedClass,-10} {b.NumberOfSeats,-6} {b.TotalPrice,-10:C} {b.BookingDate,-20:yyyy-MM-dd HH:mm}");
            }
            PrintSeparator();
        }
        public static int GetMenuChoice(int maxChoice)
        {
            while (true)
            {
                Console.Write("\nEnter your choice: ");
                if (int.TryParse(Console.ReadLine(), out var choice) && choice >= 1 && choice <= maxChoice)
                {
                    return choice;
                }

                PrintError($"Please enter a number between 1 and {maxChoice}.");
            }
        }
    }
}