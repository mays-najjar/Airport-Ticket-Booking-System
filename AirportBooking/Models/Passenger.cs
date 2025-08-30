using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AirportBooking.Models
{
    public class Passenger
    {
        public string PassengerId { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passport number is required")]
        [StringLength(20)]
        public string PassportNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passenger phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{FirstName} {LastName} - Passport: {PassportNumber}";
        }
    }
}