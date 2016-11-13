using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using ModelBuilder = Microsoft.EntityFrameworkCore.ModelBuilder;

namespace RandomPasswordGenerator.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.HasPostgresExtension("uuid-ossp");

            // Cascade delete when deleting group
            builder.Entity<ApplicationUser>().HasMany(u => u.Passwords).WithOne(p=>p.User).OnDelete(DeleteBehavior.Cascade);
            
            // Unique
            builder.Entity<ApplicationUser>().HasIndex(u => u.Email).IsUnique();
        }

        public DbSet<Password> Password { get; set; }
    }
}