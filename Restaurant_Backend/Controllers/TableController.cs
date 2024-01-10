using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Restaurant_Backend.Controllers.Tools;
using Restaurant_Backend.Models.DbModels;
using Restaurant_Backend.Models.RealTimeCommunication;
using Restaurant_Backend.Models.RequestTemplates;

namespace Restaurant_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IHubContext<UpdateHub> _hubContext;
        private readonly string secretKey = "sesame_bytes_want_to_crypt_this_so_do__it_right___now!";

        public TableController(ApiDbContext context, IHubContext<UpdateHub> hubContext, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/Table
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Table>>> Gettables()
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                return await _context.tables.ToListAsync();
            }
            return Unauthorized("INVALID TOKEN !");
        }

        // GET: api/Table/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Table>> GetTable(string id)
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var table = await _context.tables.FindAsync(int.Parse(id));

                if (table == null)
                {
                    return NotFound();
                }
                return table;
            }
            return Unauthorized("INVALID TOKEN !");
        }

        // PUT: api/Table/5
        [HttpPut]
        public async Task<IActionResult> PutTable([FromQuery(Name = "id")] string id, TableModel tableModel)
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var table = await _context.tables.FindAsync(int.Parse(id));
                if (table == null)
                {
                    return NotFound("Dish Not Found !");
                }

                table.Places = (int)(tableModel.Places != null ? tableModel.Places : table.Places);
                table.IsReserved = (bool)(tableModel.IsReserved != null ? tableModel.IsReserved : table.IsReserved);
                
                try
                {
                    await _context.SaveChangesAsync();

                    await _hubContext.Clients.All.SendAsync("sendUpdate", "Record updated");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest("Error");
                }
                return Ok(table);
            }
            return Unauthorized("INVALID TOKEN !");
        }

        // POST: api/Table
        [HttpPost]
        public async Task<ActionResult<Table>> PostTable(TableModel tableModel)
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var newTable = new Table
                {
                    Places = (int) (tableModel.Places != null ? tableModel.Places : 0) ,
                    IsReserved = (bool) (tableModel.IsReserved != null ? tableModel.IsReserved : false)
                };
                _context.tables.Add(newTable);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("sendUpdate", "Record updated");
                return CreatedAtAction("GetTable", new { id = newTable.ID }, newTable);
            }
            return Unauthorized("INVALID TOKEN !");
        }

        // DELETE: api/Table/5
        [HttpDelete]
        public async Task<IActionResult> DeleteTable([FromQuery(Name = "id")] string id)
        {
            string token = TokenTools.GetCookie("jwt", Request);
            if (TokenTools.ValidateToken(token, secretKey, _context))
            {
                var table = await _context.tables.FindAsync(int.Parse(id));
                if (table == null)
                {
                    return NotFound("Dish not exist");
                }

                _context.tables.Remove(table);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("sendUpdate", "Record updated");

                return Ok(table);
            }
            return Unauthorized("INVALID TOKEN !");
        }

    }
}
