using Microsoft.EntityFrameworkCore;
using Tessera.Models.Chapter;
using Tessera.Models.Chapter.Data;

namespace Aegis.Data
{
    public class BookDbContext : DbContext
    {
        public DbSet<ChapterEntity> Chapters { get; set; }
        public DbSet<DocumentEntity> Documents { get; set; }
        public DbSet<RowEntity> Rows { get; set; }
        public DbSet<NotificationEntity> Notifications { get; set; }
        public DbSet<EventEntity> Events { get; set; }

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
                .HasMany(c => c.Rows)
                .WithOne(r => r.Chapter) // Ensure this matches the navigation property in RowEntity
                .HasForeignKey(r => r.ChapterId) // Explicitly set the foreign key
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
