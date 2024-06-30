//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;

//namespace Aegis.Data
//{
//    public class DbContextFactory
//    {
//        private readonly IConfiguration _configuration;

//        public DbContextFactory(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public TesseraDbContext CreateDbContext(string organizationConnectionStringName)
//        {
//            string connectionString = _configuration.GetConnectionString(organizationConnectionStringName);

//            var optionsBuilder = new DbContextOptionsBuilder<TesseraDbContext>();
//            optionsBuilder.UseSqlServer(connectionString);

//            return new TesseraDbContext(optionsBuilder.Options);
//        }
//    }
//}
