using Microsoft.EntityFrameworkCore;

namespace Server_Side.Models
{
    public class ApiDbContext:DbContext
    {
        public ApiDbContext(DbContextOptions option) : base(option) { }
        public DbSet<Admin> admins { get; set; }
        public DbSet<Client> clients { get; set; }
        public DbSet<TokenExist> tokenExists { get; set; }
    }
}
