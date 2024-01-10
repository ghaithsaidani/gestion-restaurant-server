using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Restaurant_Backend.Controllers.Tools;
using Restaurant_Backend.Models.DbModels;
using Restaurant_Backend.Models.RealTimeCommunication;
using Restaurant_Backend.Models.RequestTemplates;
using System;
using System.IO;

namespace Restaurant_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisheController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IHubContext<UpdateHub> _hubContext;
        private readonly string secretKey = "sesame_bytes_want_to_crypt_this_so_do__it_right___now!";
        private readonly string hashkey = "NPLb3uBmXV4Tvr5u-Vg09iwGX_DLkHMozv1Q3NWUDR0=";
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DisheController(ApiDbContext context, IHubContext<UpdateHub> hubContext, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hubContext = hubContext;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dish>>> Getdishes()
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                return await _context.dishes.ToListAsync();
            }
            return Unauthorized("INVALID TOKEN !");

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Dish>> GetDish(string id)
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var dish = await _context.dishes.FindAsync(id);

                if (dish == null)
                {
                    return NotFound();
                }
                return dish;
            }
            return Unauthorized("INVALID TOKEN !");
        }

        [HttpPut]
        public async Task<IActionResult> PutDish([FromQuery(Name ="id")] string id, DishModel dishModel)
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var dish = await _context.dishes.FindAsync(int.Parse(id));
                if(dish == null)
                {
                    return NotFound("Dish Not Found !");
                }
                
                dish.Title = dishModel.Title != "" ? dishModel.Title : dish.Title;
                dish.Type = dishModel.Type != "" ? dishModel.Type : dish.Type;
                dish.Description = dishModel.Description != "" ? dishModel.Description : dish.Description;
                if (dishModel.Image != null)
                {
                    UploadFile.DeleteImage(_hostingEnvironment, dish.Image);
                    Random random = new Random();
                    string fileName = $"{DateTime.Today.ToString("yyyy-MM-dd")}_{random.Next()}";
                    dish.Image = await UploadFile.UploadImage(dishModel.Image, _hostingEnvironment, fileName);
                }
                try
                {
                    await _context.SaveChangesAsync();
                    
                    await _hubContext.Clients.All.SendAsync("sendUpdate", "Record updated");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest("Error");
                }
                return Ok(dish);
            }
            return Unauthorized("INVALID TOKEN !");
        }

        [HttpPost]
        public async Task<ActionResult> PostDish(DishModel dishModel)
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                Random random = new Random();
                string fileName = $"{DateTime.Today.ToString("yyyy-MM-dd")}_{random.Next()}";
                string pathImage = await UploadFile.UploadImage(dishModel.Image, _hostingEnvironment, fileName);
                var newDish = new Dish
                {
                    Title = dishModel.Title,
                    Type = dishModel.Type,
                    Description = dishModel.Description,
                    Image = pathImage
                };
                _context.dishes.Add(newDish);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("sendUpdate", "Record updated");
                return CreatedAtAction("GetDish", new { id = newDish.ID }, newDish);
            }
            return Unauthorized("INVALID TOKEN !");

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDish([FromQuery(Name ="id")] string id)
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var dish = await _context.dishes.FindAsync(int.Parse(id));
                if (dish == null)
                {
                    return NotFound("Dish not exist");
                }

                _context.dishes.Remove(dish);
                UploadFile.DeleteImage(_hostingEnvironment, dish.Image);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("sendUpdate", "Record updated");

                return Ok(dish);
            }
            return Unauthorized("INVALID TOKEN !");
        }
    }
}
