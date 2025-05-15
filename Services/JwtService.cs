using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventBookingSystemV1.Configuration;
using EventBookingSystemV1.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventBookingSystemV1.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _opts;

        public JwtService(IOptions<JwtSettings> opts)
        {
            _opts = opts?.Value ?? throw new ArgumentNullException(nameof(opts));
        }

        /// <summary>
        /// Lifetime of the token in minutes, per configuration.
        /// </summary>
        public int TokenLifetimeMinutes => _opts.DurationInMinutes;

        public string GenerateToken(User user)
        {
            // 1) Prepare key and credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 2) Build claims
            var now = DateTime.UtcNow;
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(ClaimTypes.Name,               user.FullName),
                new Claim(ClaimTypes.DateOfBirth,        user.BirthDate.ToString("yyyy-MM-dd")),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role,               user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            };

            // 3) Create token
            var token = new JwtSecurityToken(
                issuer: _opts.ValidIss,
                audience: _opts.ValidAud,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_opts.DurationInMinutes),
                signingCredentials: creds
            );

            // 4) Return serialized JWT
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
