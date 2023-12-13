using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Backend.Controllers;
using Restaurant_Backend.Models.DbModels;
using Restaurant_Backend.Models.RequestTemplates;
using Utilities;
using NuGet.Common;

namespace Server_Side.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly string secretKey = "sesame_bytes_want_to_crypt_this_so_do__it_right___now!";
        private readonly string hashkey = "NPLb3uBmXV4Tvr5u-Vg09iwGX_DLkHMozv1Q3NWUDR0=";

        public ClientController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginClient([FromBody] LoginModel loginModel)
        {
            var user = await _context.clients.FirstOrDefaultAsync(u => u.Email == loginModel.Email);
            if (user == null)
            {
                return NotFound("User Not Found");
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password);
            if (isPasswordValid)
            {
                string tokenString = TokenTools.GenerateToken(secretKey, user.ID.ToString());
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


        [HttpPost("register")]
        public async Task<IActionResult> RegisterClient([FromBody] RegisterModel registerModel)
        {
            if (await _context.clients.AnyAsync(u => u.Email == registerModel.Email))
            {
                return BadRequest("Email us already exist");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerModel.Password);
            var newClient = new Client
            {
                Email = registerModel.Email,
                Password = hashedPassword,
                Nom = registerModel.Nom,
                Prenom = registerModel.Prenom,
                Telephone = registerModel.Telephone,
                Adresse = registerModel.Adresse

            };
            _context.clients.Add(newClient);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetClient", new { id = newClient.ID }, newClient);

        }

        [HttpGet("logout")]
        public async Task<ActionResult> LogoutClient([FromHeader(Name = "AUTHORIZATION")] string token)
        {
            if (TokenTools.ValidateToken(token, secretKey, _context))
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
        public async Task<ActionResult> SendPassRecoveryClient([FromBody] SendMailModel mailModel)
        {
            var user = await _context.clients.FirstOrDefaultAsync(u => u.Email == mailModel.Email);
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
            string body = $"Hello, this is recovery pasword mail and hashed ID is {Crypt.Encrypt(user.ID.ToString(), hashkey)}";

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
        public async Task<ActionResult> ChangePassword([FromQuery(Name = "id")] string id, [FromBody] RecoveryPasswordModel recoveryPassword)
        {
            try
            {
                string userId = Crypt.Decrypt(id, hashkey);
                var user = await _context.clients.FindAsync(int.Parse(userId));
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

        // GET: api/Client
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> Getclients([FromHeader(Name = "AUTHORIZATION")] string token)
        {
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                return await _context.clients.ToListAsync();
            }
            return Unauthorized("INVALID TOKEN !");
        }

        // GET: api/Client/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient([FromHeader(Name = "AUTHORIZATION")] string token,int id)
        {
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var client = await _context.clients.FindAsync(id);

                if (client == null)
                {
                    return NotFound();
                }

                return client;
            }
            return Unauthorized("INVALID TOKEN !");
        }

        // PUT: api/Client/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient([FromHeader(Name = "AUTHORIZATION")] string token, int id, UpdateModel updateModel)
        {
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var client = await _context.clients.FindAsync(id);
                if (client == null)
                {
                    return NotFound("User Not Found !");
                }

                client.Nom = updateModel.Nom != "" ? updateModel.Nom : client.Nom;
                client.Prenom = updateModel.Prenom != "" ? updateModel.Prenom : client.Prenom;
                client.Telephone = updateModel.Telephone != "" ? updateModel.Telephone : client.Telephone;
                client.Adresse = updateModel.Adresse != "" ? updateModel.Adresse : client.Adresse;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest("Error");
                }

                return Ok("User Updated !");
            }
            return Unauthorized("INVALID TOKEN !");
        }

        // DELETE: api/Client/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient([FromHeader(Name = "AUTHORIZATION")] string token,int id)
        {
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var client = await _context.clients.FindAsync(id);
                if (client == null)
                {
                    return NotFound();
                }

                _context.clients.Remove(client);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            return Unauthorized("INVALID TOKEN !");
        }
        
    }
}
