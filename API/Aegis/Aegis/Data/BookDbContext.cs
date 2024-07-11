using Microsoft.EntityFrameworkCore;
using Tessera.Models.Book;
using Tessera.Models.ChapterComponents;

namespace Aegis.Data
{
    public class BookDbContext : DbContext
    {
        /*********************************
         * DECLARING DBSET<> PROPERTIES
         * CAUSES THE INITIALIZATION OF
         * THE DB TO FAIL!
         * 
         * To Avoid this use Direct Database
         * Access to initialize the database
         *********************************/

        public DbSet<BookEntity> Book {  get; set; }
        public DbSet<ChapterEntity> Chapters { get; set; }
        public DbSet<ScholarEntity> Scholars { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
        public DbSet<RolePermissionsEntity> RolePermissions { get; set; }
        public DbSet<RoleCollectionEntity> RoleCollections { get; set; }

        public BookDbContext(DbContextOptions<BookDbContext> options)
            : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
