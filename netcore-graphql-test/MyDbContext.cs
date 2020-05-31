using Microsoft.EntityFrameworkCore;

namespace netcore_graphql_test
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Location> Locations { get; set; }
    }
}