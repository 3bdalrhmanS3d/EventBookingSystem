using EventBookingSystemV1.Models;
using System.Collections.Generic;

namespace EventBookingSystemV1.ViewModels
{
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public UserRoleViewModel Role { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<BookingViewModel> Bookings { get; set; } = new();
    }

    
}
