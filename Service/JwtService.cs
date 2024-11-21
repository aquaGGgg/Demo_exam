using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Test_Demo_Ex.Service
{
        public class JwtService
        {
        private readonly string _secret;
        private readonly int _accessTokenExpirationMinutes;

        public JwtService(string secret, int accessTokenExpirationMinutes)
        {
            // Ensure the secret is at least 32 characters (256 bits)
            if (Encoding.UTF8.GetByteCount(secret) < 32)
            {
                throw new ArgumentException("The JWT secret key must be at least 32 bytes long.");
            }

            _secret = secret;
            _accessTokenExpirationMinutes = accessTokenExpirationMinutes;
        }

        public string GenerateToken(Guid userId, string role)
        {
            var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // Unique user identifier
            new Claim(ClaimTypes.Role, role), // User role
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Token identifier
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
