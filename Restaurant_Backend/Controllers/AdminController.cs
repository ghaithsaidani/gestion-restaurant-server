using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server_Side.Models;
using System.Security.Cryptography;

namespace Server_Side.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly string secretKey = "sesame_bytes_want_to_crypt_this_so_do__it_right___now!";
        private readonly string hashkey = "NPLb3uBmXV4Tvr5u-Vg09iwGX_DLkHMozv1Q3NWUDR0=";

        public AdminController(ApiDbContext context)
        {
            _context = context;
        }
        //LoginAdmin

        [HttpPost("login")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginModel loginModel)
        {
            var user = await _context.admins.FirstOrDefaultAsync(u => u.Email == loginModel.Email);
            if (user == null)
            {
                return NotFound("User Not Found");
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password);
            if (isPasswordValid)
            {
                string tokenString = GenerateToken(secretKey, user.ID.ToString());
                var token = new TokenExist
                {
                    Token = tokenString
                };
                _context.tokenExists.Add(token);
                _context.SaveChanges();
                return Ok(new { Jwt = tokenString });
            }
            return BadRequest("Invalid Credentials");

        }


        //RegisterAdmin
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel registerModel)
        {
            if (await _context.admins.AnyAsync(u => u.Email == registerModel.Email))
            {
                return BadRequest("Email us already exist");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerModel.Password);
            var newAdmin = new Admin
            {
                Email = registerModel.Email,
                Password = hashedPassword,
                Nom = registerModel.Nom,
                Prenom = registerModel.Prenom,
                Telephone = registerModel.Telephone,
                Adresse = registerModel.Adresse

            };
            _context.admins.Add(newAdmin);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetAdmin", new { id = newAdmin.ID }, newAdmin);

        }
        // Logout

        [HttpGet("logout")]
        public async Task<ActionResult> LogoutAdmin([FromHeader(Name = "AUTHORIZATION")] string token)
        {
            if (ValidateToken(token, secretKey))
            {
                var givenToken = await _context.tokenExists.FirstOrDefaultAsync(t => t.Token == token);
                if (givenToken == null)
                {
                    return Unauthorized("INVALID TOKEN !");
                }
                _context.tokenExists.Remove(givenToken);
                await _context.SaveChangesAsync();
                return Ok("Logout Successfuly");


            }
            return Unauthorized("INVALID TOKEN !");

        }
        [HttpPost("sendPassRecoveryLink")]
        public async Task<ActionResult> SendPassRecoveryAdmin([FromBody] SendMailModel mailModel)
        {
            var user = await _context.admins.FirstOrDefaultAsync(u => u.Email == mailModel.Email);
            if (user == null)
            {
                return NotFound("User Not Found");
            }
            // Sender's email and password (use an application-specific password for security)
            string senderEmail = "mohamedamine.khemiri@sesame.com.tn";
            string senderPassword = "SygfkhMi"; // Use an application-specific password
            string displayName = "Sesame Restaurant";

            // Recipient's email
            string recipientEmail = mailModel.Email;

            // Compose the email
            string subject = "Password Recovery";
            string body = $"Hello, this is recovery pasword mail and hashed ID is {Encrypt(user.ID.ToString(), hashkey)}";

            // Configure SmtpClient
            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                // Compose the email message
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, displayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(recipientEmail);

                // Send the email
                try
                {
                    client.Send(mailMessage);
                    return Ok("Email sent successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return BadRequest("Error while sending mail !");
                }
            }
        }
        [HttpPut("changePassword")]
        public async Task<ActionResult> ChangePassword([FromQuery(Name = "id")] string id,[FromBody]RecoveryPasswordModel recoveryPassword)
        {
            try
            {
                string userId = Decrypt(id, hashkey);
                var user = await _context.admins.FindAsync(int.Parse(userId));
                if (user == null)
                {
                    return NotFound("User not Found");
                }
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(recoveryPassword.Password);
                user.Password = hashedPassword;
                await _context.SaveChangesAsync();
                return Ok("Password Updated Successfuly !");
            }
            catch
            {
                return BadRequest("Invalid User !");
            }
            
        }


        // GET: api/Admin
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Admin>>> Getadmins([FromHeader(Name = "AUTHORIZATION")] string token)
        {
            if (ValidateToken(token, secretKey))
            {
                return await _context.admins.ToListAsync();
            }
            return Unauthorized("INVALID TOKEN !");


        }

        // GET: api/Admin/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Admin>> GetAdmin(int id)
        {
            var admin = await _context.admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound();
            }

            return admin;
        }

        // PUT: api/Admin/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdmin(int id, Admin admin)
        {
            if (id != admin.ID)
            {
                return BadRequest();
            }

            _context.Entry(admin).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Admin/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            _context.admins.Remove(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AdminExists(int id)
        {
            return _context.admins.Any(e => e.ID == id);
        }
        private bool TokenExist(string token)
        {
            return _context.tokenExists.Any(e => e.Token == token);
        }
        private bool ValidateToken(string token, string secretKey)
        {
            if (!TokenExist(token))
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
        private string GenerateToken(string secretKey, string idForClaim)
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


        public static string Encrypt(string clearText, string EncryptionKey)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText, string EncryptionKey)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }





    }
}





