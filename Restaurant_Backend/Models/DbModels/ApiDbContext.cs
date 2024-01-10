using Microsoft.EntityFrameworkCore;
using Restaurant_Backend.Models.RequestTemplates;

namespace Restaurant_Backend.Models.DbModels
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions option) : base(option) { }
        public DbSet<Admin> admins { get; set; }
        public DbSet<Client> clients { get; set; }
        public DbSet<TokenExist> tokenExists { get; set; }
        public DbSet<Dish> dishes { get; set; }
        public DbSet<Table> tables { get; set; }
    }
}
