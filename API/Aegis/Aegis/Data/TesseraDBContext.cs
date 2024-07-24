using Tessera.Models;
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

        public DbSet<Preface> Library { get; set; }
        public DbSet<Catalog> Catalog { get; set; }


        // DbSet properties for your application's entities if needed
        // Example:
        // public DbSet<SomeEntity> SomeEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override defaults if needed
            // For example, you can rename the default ASP.NET Identity tables

            // Configure the many-to-many relationship
            builder.Entity<Catalog>()
                .HasKey(uo => new { uo.ScribeId, uo.BookId });

            builder.Entity<Catalog>()
                .HasOne(uo => uo.Scribe)
                .WithMany()
                .HasForeignKey(uo => uo.ScribeId);

            builder.Entity<Catalog>()
                .HasOne(uo => uo.Preface)
                .WithMany()
                .HasForeignKey(uo => uo.BookId);
        }
    }
}
