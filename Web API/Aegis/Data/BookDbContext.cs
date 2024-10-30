using Microsoft.EntityFrameworkCore;
using Tessera.Models.Chapter;

namespace Aegis.Data
{
    public class BookDbContext : DbContext
    {
        public DbSet<ChapterEntity> Chapters { get; set; }

        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
