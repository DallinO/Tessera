using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tessera.Constants;

namespace Aegis.Data
{
    public class DbContextFactory
    {
        private readonly IConfiguration _configuration;

        public DbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public BookDbContext CreateDbContext(string dbName)
        {
            string connectionString = Keys.SQL_SERVER_ROOT + $"Database = {dbName}; Trusted_Connection = True; Encrypt = False;";

            var optionsBuilder = new DbContextOptionsBuilder<BookDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new BookDbContext(optionsBuilder.Options);
        }
    }
}
