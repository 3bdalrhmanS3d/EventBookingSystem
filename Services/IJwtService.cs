using EventBookingSystemV1.Models;

namespace EventBookingSystemV1.Services
{
    public interface IJwtService
    {
        /// <summary>
        /// Generates a signed JWT for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>A JWT as a string.</returns>
        string GenerateToken(EventBookingSystemV1.Models.User user);
        
    }

}
