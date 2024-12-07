using Microsoft.EntityFrameworkCore;
using Tessera.Models.Chapter;

namespace Aegis.Data
{
    public class BookDbContext : DbContext
    {
        public DbSet<ChapterEntity> Chapters { get; set; }
        public DbSet<DocumentEntity> Documents { get; set; }
        public DbSet<RowEntity> Rows { get; set; }

        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChapterEntity>()
                .HasOne(c => c.Document)              
                .WithOne(d => d.Chapter)        
                .HasForeignKey<DocumentEntity>(d => d.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChapterEntity>()
                .HasMany<RowEntity>(c => c.Rows)
                .WithOne()
                .HasForeignKey(r => r.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
