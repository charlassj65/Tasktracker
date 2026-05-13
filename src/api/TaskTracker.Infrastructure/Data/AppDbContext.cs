using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");

            entity.HasKey(task => task.Id);

            entity.Property(task => task.Id)
                .ValueGeneratedOnAdd();

            entity.Property(task => task.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(task => task.Description)
                .IsRequired(false);

            entity.Property(task => task.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(task => task.DueDate)
                .IsRequired(false);
        });
    }
}
