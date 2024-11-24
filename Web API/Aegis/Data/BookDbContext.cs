using Microsoft.EntityFrameworkCore;
using Tessera.Models.Chapter;

namespace Aegis.Data
{
    public class BookDbContext : DbContext
    {
        public DbSet<ChapterEntity> Chapters { get; set; }
        public DbSet<DocumentEntity> Documents { get; set; }

        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            

            //modelBuilder.Entity<ChapterEntity>()
            //    .HasOne(c => c.List)
            //    .WithOne()
            //    .HasForeignKey<ListEntity>(l => l.ChapterId);

            //modelBuilder.Entity<ChapterEntity>()
            //    .HasOne(c => c.Calendar)
            //    .WithOne()
            //    .HasForeignKey<CalendarEntity>(cal => cal.ChapterId);
        }
    }
}
