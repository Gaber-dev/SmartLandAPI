using SmartLandAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace SmartLandAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }

        public DbSet<Plant> Plants { get; set; }
        public DbSet<Crop> Crops { get; set; }
        public DbSet<Fertilizer> Fertilizers { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        

        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

           
            builder.Entity<PasswordReset>()
                .HasIndex(pr => new { pr.UserId, pr.Code });

           
            
            




           
            builder.Entity<Plant>()
                .HasIndex(p => p.Name);

            builder.Entity<Crop>()
                .HasIndex(c => c.Name);

            builder.Entity<Fertilizer>()
                .HasIndex(f => f.Name);






        }

    }
    
}
