using Microsoft.EntityFrameworkCore;
using Tellus.Models.Business;
using Tessera.Models.Book;
using Tessera.Models.Chapter;
using Tessera_Models.Chapter.Data;

namespace Aegis.Data
{
    public class BookDbContext : DbContext
    {
        public DbSet<ChapterEntity> Chapters { get; set; }
        public DbSet<ScholarEntity> Scholars { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
        public DbSet<ListEntity> Lists { get; set; }
        public DbSet<ColumnEntity> Columns { get; set; }
        public DbSet<DataEntity> Data { get; set; }


        public BookDbContext(DbContextOptions<BookDbContext> options)
            : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-Many Relationship: RoleEntity and PermissionsEntity
            modelBuilder.Entity<RoleEntity>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity<RolePermissionsEntity>(
                    j => j
                        .HasOne(rp => rp.Permission)
                        .WithMany(p => p.RolePermissions)
                        .HasForeignKey(rp => rp.PermissionID),
                    j => j
                        .HasOne(rp => rp.Role)
                        .WithMany(r => r.RolePermissions)
                        .HasForeignKey(rp => rp.RoleId),
                    j =>
                    {
                        j.HasKey(t => new { t.RoleId, t.PermissionID });
                    });

            // Many-to-Many Relationship: RoleEntity and RoleCollectionEntity
            modelBuilder.Entity<RoleEntity>()
                .HasMany(r => r.RoleCollections)
                .WithMany(rc => rc.Roles)
                .UsingEntity<RoleCollectionRoles>(
                    j => j
                        .HasOne(rcr => rcr.RoleCollection)
                        .WithMany(rc => rc.RoleCollectionRoles)
                        .HasForeignKey(rcr => rcr.RoleCollectionId),
                    j => j
                        .HasOne(rcr => rcr.Role)
                        .WithMany(r => r.RoleCollectionRoles)
                        .HasForeignKey(rcr => rcr.RoleId),
                    j =>
                    {
                        j.HasKey(t => new { t.RoleCollectionId, t.RoleId });
                    });

            // Many-to-Many Relationship: StatusEntity and StatusCollectionEntity
            modelBuilder.Entity<StatusEntity>()
                .HasMany(s => s.StatusCollections)
                .WithMany(sc => sc.Statuses)
                .UsingEntity<StatusCollectionStatuses>(
                    j => j
                        .HasOne(scs => scs.StatusCollection)
                        .WithMany(sc => sc.StatusCollectionStatuses)
                        .HasForeignKey(scs => scs.StatusCollectionId),
                    j => j
                        .HasOne(scs => scs.Status)
                        .WithMany(s => s.StatusCollectionStatuses)
                        .HasForeignKey(scs => scs.StatusId),
                    j =>
                    {
                        j.HasKey(t => new { t.StatusCollectionId, t.StatusId });
                    });
        }
    }
}
