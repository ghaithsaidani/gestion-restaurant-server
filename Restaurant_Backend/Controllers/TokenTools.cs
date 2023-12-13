using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Restaurant_Backend.Models.DbModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Restaurant_Backend.Controllers
{
    public class TokenTools
    {
        public static string GenerateToken(string secretKey, string idForClaim)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
            {
                    new Claim("Id", idForClaim),
                }),
                Expires = DateTime.UtcNow.AddDays(1), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        public static bool ValidateToken(string token, string secretKey,ApiDbContext _context)
        {
            if (!_context.tokenExists.Any(e => e.Token == token))
            {
                return false;
            }
            else
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };

                SecurityToken validatedToken;
                ClaimsPrincipal principal;

                try
                {
                    principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                }
                catch
                {
                    // Token validation failed
                    return false;
                }

                // Token is valid; you can now access the claims from the principal
                return true;
            }

        }

    }
}
