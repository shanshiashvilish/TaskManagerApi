using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Models;

namespace TaskManagerApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskTransferHistory> TaskTransferHistories => Set<TaskTransferHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Id)
            .IsUnique();

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.Title)
            .IsUnique();

        modelBuilder.Entity<TaskTransferHistory>()
            .HasIndex(h => new { h.TaskId, h.UserId })
            .IsUnique(false);
    }
}