namespace EventBookingSystemV1.ViewModels
{
    public enum UserRoleViewModel
    {
        User,
        Admin
    }
    
    public class UserViewModel
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public UserRoleViewModel Role { get; set; }

        public DateTimeOffset BirthDate { get; set; }

        public int Age
        {
            get
            {
                var age = DateTime.UtcNow.Year - BirthDate.Year;
                if (DateTime.UtcNow < BirthDate.AddYears(age)) age--;
                return age;
            }
            set { }
        }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
