using Microsoft.EntityFrameworkCore;
using user.Models;

namespace user.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Assign> Assignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Configure Composite Key for the Assign table
        modelBuilder.Entity<Assign>()
            .HasKey(a => new { a.UserId, a.JobId });

        // 2. Configure User side of the relationship
        modelBuilder.Entity<Assign>()
            .HasOne(a => a.User)
            .WithMany(u => u.Assignments)
            .HasForeignKey(a => a.UserId);

        // 3. Configure Job side of the relationship
        modelBuilder.Entity<Assign>()
            .HasOne(a => a.Job)
            .WithMany(j => j.Assignments)
            .HasForeignKey(a => a.JobId);
    }
}
