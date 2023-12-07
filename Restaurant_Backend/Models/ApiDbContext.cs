using Microsoft.EntityFrameworkCore;

namespace Server_Side.Models
{
    public class ApiDbContext:DbContext
    {
        public ApiDbContext(DbContextOptions option) : base(option) { }
    }
}
