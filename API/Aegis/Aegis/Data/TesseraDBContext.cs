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

        public DbSet<BookEntity> Library { get; set; }
        public DbSet<Catalog> Catalog { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the many-to-many relationship
            builder.Entity<Catalog>()
                .HasKey(c => new { c.ScribeId, c.BookId });

            builder.Entity<Catalog>()
                .HasOne(c => c.Scribe)
                .WithMany(s => s.Catalogs) // Configure Scribe to have a collection of Catalogs
                .HasForeignKey(c => c.ScribeId);

            builder.Entity<Catalog>()
                .HasOne(c => c.BookEntity)
                .WithMany(b => b.Catalogs) // Configure BookEntity to have a collection of Catalogs
                .HasForeignKey(c => c.BookId);
        }
    }

}
