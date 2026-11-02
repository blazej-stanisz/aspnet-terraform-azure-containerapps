using MySimpleNetApp.WebUI.Data;
using MySimpleNetApp.WebUI.Models;

namespace MySimpleNetApp.UnitTests;

public sealed class FakeTodoRepository : ITodoRepository
{
    private readonly List<TodoItem> _items = [];
    private int _nextId = 1;

    public IReadOnlyList<TodoItem> Items => _items;

    public Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.ToList());

    public Task<TodoItem> AddAsync(string title, CancellationToken cancellationToken = default)
    {
        var item = new TodoItem
        {
            Id = _nextId++,
            Title = title.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(i => i.Id == id));

    public Task<bool> UpdateTitleAsync(int id, string title, CancellationToken cancellationToken = default)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is null)
        {
            return Task.FromResult(false);
        }

        item.Title = title.Trim();
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(item is not null && _items.Remove(item));
    }
}
