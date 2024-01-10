using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Backend.Models.DbModels;
using Restaurant_Backend.Models.RequestTemplates;
using Microsoft.AspNetCore.SignalR;
using Restaurant_Backend.Models.RealTimeCommunication;
using Restaurant_Backend.Controllers.Tools;

namespace Server_Side.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IHubContext<UpdateHub> _hubContext;
        private readonly string secretKey = "sesame_bytes_want_to_crypt_this_so_do__it_right___now!";
        private readonly string hashkey = "NPLb3uBmXV4Tvr5u-Vg09iwGX_DLkHMozv1Q3NWUDR0=";
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AdminController(ApiDbContext context, IHubContext<UpdateHub> hubContext, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hubContext = hubContext;
            _hostingEnvironment = hostingEnvironment;

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
                string tokenString = TokenTools.GenerateToken(secretKey, user.ID.ToString(),"admin");
                var token = new TokenExist
                {
                    Token = tokenString
                };
                _context.tokenExists.Add(token);
                _context.SaveChanges();
                TokenTools.SetCookie("jwt", tokenString, Response);
                return Ok(user);
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

            //// SignalR TEST BEGIN            

            await _hubContext.Clients.All.SendAsync("sendUpdate", "Record updated");

            //// SignalR TEST ENDS  

            return CreatedAtAction("GetAdmin", new { id = newAdmin.ID }, newAdmin);

        }
        // Logout

        [HttpGet("logout")]
        public async Task<ActionResult> LogoutAdmin()
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if(token != "")
            {
                if (TokenTools.ValidateToken(token, secretKey, _context))
                {
                    var givenToken = await _context.tokenExists.FirstOrDefaultAsync(t => t.Token == token);
                    if (givenToken == null)
                    {
                        return Unauthorized("INVALID TOKEN !");
                    }
                    _context.tokenExists.Remove(givenToken);
                    TokenTools.DeleteCookie("jwt", Response);
                    await _context.SaveChangesAsync();
                    return Ok("Logout Successfuly");


                }
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
            string body = $"Hello, to reset your password follow this link : http://localhost:4200/auth/change-password?id={Crypt.Encrypt(user.ID.ToString(), hashkey)}";

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
        public async Task<ActionResult<IEnumerable<Admin>>> Getadmins()
        {
            string token = TokenTools.GetCookie("jwt", Request);
            
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                return await _context.admins.ToListAsync();
            }
            return Unauthorized("INVALID TOKEN !");
            
            //return await _context.admins.ToListAsync();


        }

        // GET: api/Admin/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Admin>> GetAdmin([FromHeader(Name = "AUTHORIZATION")] string token, int id)
        {
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var admin = await _context.admins.FindAsync(id);

                if (admin == null)
                {
                    return NotFound();
                }

                return admin;
            }
            return Unauthorized("INVALID TOKEN !");

        }

        // PUT: api/Admin/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdmin([FromHeader(Name = "AUTHORIZATION")] string token, int id, [FromBody] UpdateModel updateModel)
        {
            /*
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var admin = await _context.admins.FindAsync(id);
                if (admin == null)
                {
                    return NotFound("User Not Found !");
                }

                admin.Nom = updateModel.Nom != "" ? updateModel.Nom : admin.Nom;
                admin.Prenom = updateModel.Prenom != "" ? updateModel.Prenom : admin.Prenom;
                admin.Telephone = updateModel.Telephone != "" ? updateModel.Telephone : admin.Telephone;
                admin.Adresse = updateModel.Adresse != "" ? updateModel.Adresse : admin.Adresse;
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
            */

            var admin = await _context.admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound("User Not Found !");
            }

            admin.Nom = updateModel.Nom != "" ? updateModel.Nom : admin.Nom;
            admin.Prenom = updateModel.Prenom != "" ? updateModel.Prenom : admin.Prenom;
            admin.Telephone = updateModel.Telephone != "" ? updateModel.Telephone : admin.Telephone;
            admin.Adresse = updateModel.Adresse != "" ? updateModel.Adresse : admin.Adresse;
            try
            {
                await _context.SaveChangesAsync();


                //// SignalR TEST BEGIN            

                await _hubContext.Clients.All.SendAsync("sendUpdate", "Record updated");

                //// SignalR TEST ENDS  
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Error");
            }

            return Ok("User Updated !");



        }

        // DELETE: api/Admin/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin([FromHeader(Name = "AUTHORIZATION")] string token, int id)
        {
            var admin = await _context.admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            _context.admins.Remove(admin);
            await _context.SaveChangesAsync();


            //// SignalR TEST BEGIN            

            await _hubContext.Clients.All.SendAsync("sendUpdate", "Record updated");

            //// SignalR TEST ENDS  

            return Ok("Admin removed");
        }



        

    }

}





