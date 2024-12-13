using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Tessera.Models.Chapter.Data;

namespace Aegis.Data
{
    public class TesseraDbContext : IdentityDbContext<AppUser>
    {
        public TesseraDbContext(DbContextOptions<TesseraDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppUser> Users {  get; set; }
        public DbSet<BookEntity> Library { get; set; }
        public DbSet<ReportEntity> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
