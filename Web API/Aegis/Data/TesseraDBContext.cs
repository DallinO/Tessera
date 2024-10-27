using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Aegis.Data
{
    public class TesseraDbContext : IdentityDbContext<Scribe>
    {
        public TesseraDbContext(DbContextOptions<TesseraDbContext> options)
            : base(options)
        {
        }

        public DbSet<Scribe> Scribes {  get; set; }
        public DbSet<BookEntity> Library { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
