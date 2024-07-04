using Tessera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Aegis.Data
{
    public class TesseraDbContext : IdentityDbContext<ApplicationUser>
    {
        public TesseraDbContext(DbContextOptions<TesseraDbContext> options)
            : base(options)
        {
        }

        public DbSet<OrganizationBase> Organizations { get; set; }
        public DbSet<UserOrganization> UserOrganizations { get; set; }


        // DbSet properties for your application's entities if needed
        // Example:
        // public DbSet<SomeEntity> SomeEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override defaults if needed
            // For example, you can rename the default ASP.NET Identity tables

            // Configure the many-to-many relationship
            builder.Entity<UserOrganization>()
                .HasKey(uo => new { uo.ApplicationUserId, uo.OrganizationBaseId });

            builder.Entity<UserOrganization>()
                .HasOne(uo => uo.ApplicationUser)
                .WithMany()
                .HasForeignKey(uo => uo.ApplicationUserId);

            builder.Entity<UserOrganization>()
                .HasOne(uo => uo.OrganizationBase)
                .WithMany()
                .HasForeignKey(uo => uo.OrganizationBaseId);
        }
    }
}
