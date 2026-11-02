using MySimpleNetApp.WebUI.Models;

namespace MySimpleNetApp.WebUI.Data;

public interface ITodoRepository
{
    Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TodoItem> AddAsync(string title, CancellationToken cancellationToken = default);

    Task<bool> UpdateTitleAsync(int id, string title, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
