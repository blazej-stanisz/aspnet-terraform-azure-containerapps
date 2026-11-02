namespace MySimpleNetApp.WebUI.Models;

public sealed record TodoResponse(int Id, string Title, DateTime CreatedAt)
{
    public static TodoResponse From(TodoItem item) => new(item.Id, item.Title, item.CreatedAt);
}
