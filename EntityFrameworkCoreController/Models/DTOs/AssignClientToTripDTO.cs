using System;
using System.ComponentModel.DataAnnotations;

namespace EntityFrameworkCoreController.Models.DTOs
{
    public class AssignClientToTripDto
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Telephone is required")]
        public string Telephone { get; set; }
        
        [Required(ErrorMessage = "PESEL is required")]
        public string Pesel { get; set; }
        
        [Required(ErrorMessage = "Trip ID is required")]
        public int IdTrip { get; set; }
        
        [Required(ErrorMessage = "Trip name is required")]
        public string TripName { get; set; }
        
        public DateTime PaymentDate { get; set; }
    }
}