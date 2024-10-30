using Microsoft.EntityFrameworkCore;

namespace Aegis.Data
{
    public class DbContextFactory
    {
        private readonly IConfiguration _configuration;

        public DbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public BookDbContext CreateDbContext(string connectionStringName)
        {
            string connectionString = _configuration.GetConnectionString("TesseraPM");

            var optionsBuilder = new DbContextOptionsBuilder<BookDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new BookDbContext(optionsBuilder.Options);
        }
    }
}
