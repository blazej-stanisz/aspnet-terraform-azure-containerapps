using Microsoft.EntityFrameworkCore;
using MySimpleNetApp.WebUI.Models;

namespace MySimpleNetApp.WebUI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var todo = modelBuilder.Entity<TodoItem>();

        todo.ToTable("todos");
        todo.HasKey(t => t.Id);
        todo.Property(t => t.Id).HasColumnName("id");
        todo.Property(t => t.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
        todo.Property(t => t.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
    }
}
