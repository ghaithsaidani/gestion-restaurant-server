using Azure;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Restaurant_Backend.Models.DbModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Restaurant_Backend.Controllers.Tools
{
    public class TokenTools
    {
        public static string GenerateToken(string secretKey, string idForClaim,string userTypeForClaim)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
            {
                    new Claim("Id", idForClaim),
                    new Claim("Type",userTypeForClaim)
                }),
                Expires = DateTime.UtcNow.AddDays(1), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        public static bool ValidateToken(string token, string secretKey, ApiDbContext _context)
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
        public static string GetCookie(string cookieFieldName, HttpRequest request)
        {
            string? myString = request.Cookies[cookieFieldName];

            if (myString != null)
            {
                return myString;
            }
            else
            {
                return "";
            }
        }

        public static void DeleteCookie(string cookieFieldName, HttpResponse response)
        {
            response.Cookies.Delete(cookieFieldName);
        }

        public static void SetCookie(string cookieFieldName, string token, HttpResponse response)
        {

            // Set the HTTP-only cookie on the client
            response.Cookies.Append(cookieFieldName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to true if using HTTPS
                SameSite = SameSiteMode.Strict, // Adjust based on your requirements
                Expires = DateTime.UtcNow.AddDays(1), // Adjust expiration time
            });

        }

    }
}
