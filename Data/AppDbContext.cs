
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ProjectManagement_Mirela.Models;

namespace ProjectManagement_Mirela.Data
{
    public class AppDbContext : IdentityDbContext<Employee>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Employee>()
                .HasOne(e => e.Team)
                .WithMany(t => t.Employees)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Project>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Projects)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Team>()
                .HasIndex(t => t.Name)
                .IsUnique();

            builder.Entity<Project>()
                .HasIndex(p => p.Name)
                .IsUnique();

            builder.Entity<Employee>()
                .Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Entity<Project>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<Team>()
                .Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
