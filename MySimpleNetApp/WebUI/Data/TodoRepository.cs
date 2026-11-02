using Microsoft.EntityFrameworkCore;
using MySimpleNetApp.WebUI.Models;

namespace MySimpleNetApp.WebUI.Data;

public class TodoRepository(AppDbContext db) : ITodoRepository
{
    public Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        db.Todos
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);

    public async Task<TodoItem> AddAsync(string title, CancellationToken cancellationToken = default)
    {
        var item = new TodoItem
        {
            Title = title.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.Todos.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return item;
    }

    public Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<bool> UpdateTitleAsync(int id, string title, CancellationToken cancellationToken = default)
    {
        var item = await db.Todos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (item is null)
        {
            return false;
        }

        item.Title = title.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await db.Todos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (item is null)
        {
            return false;
        }

        db.Todos.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
