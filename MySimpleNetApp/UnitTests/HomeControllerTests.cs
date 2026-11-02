using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using MySimpleNetApp.WebUI.Controllers;
using MySimpleNetApp.WebUI.Models;

namespace MySimpleNetApp.UnitTests;

public class HomeControllerTests
{
    private static HomeController CreateController(
        string traceId = "test-trace-id",
        FakeTodoRepository? todos = null,
        FakeDatabaseConnectionChecker? database = null)
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = traceId };
        var controller = new HomeController(
            NullLogger<HomeController>.Instance,
            todos ?? new FakeTodoRepository(),
            database ?? new FakeDatabaseConnectionChecker(true))
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, new NullTempDataProvider())
        };
        return controller;
    }

    [Fact]
    public async Task Index_WhenDatabaseIsConnected_SetsConnectedFlag()
    {
        var controller = CreateController(database: new FakeDatabaseConnectionChecker(true));

        var result = await controller.Index(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(true, view.ViewData["DatabaseConnected"]);
    }

    [Fact]
    public async Task Index_WhenDatabaseIsDisconnected_SetsDisconnectedFlag()
    {
        var controller = CreateController(database: new FakeDatabaseConnectionChecker(false));

        var result = await controller.Index(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(false, view.ViewData["DatabaseConnected"]);
    }

    [Fact]
    public void Privacy_WhenCalled_ReturnsViewResult()
    {
        var result = CreateController().Privacy();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_WhenCalled_ReturnsViewModelWithRequestId()
    {
        var result = CreateController("trace-123").Error();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(view.Model);
        Assert.False(string.IsNullOrWhiteSpace(model.RequestId));
        Assert.True(model.ShowRequestId);
    }

    [Fact]
    public async Task TodoList_WhenCalled_ReturnsViewWithAllItems()
    {
        var todos = new FakeTodoRepository();
        await todos.AddAsync("pierwsze zadanie");
        await todos.AddAsync("drugie zadanie");

        var result = await CreateController(todos: todos).TodoList(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<TodoItem>>(view.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task AddTodo_WhenTitleIsWhitespace_RedirectsWithoutAdding()
    {
        var todos = new FakeTodoRepository();
        var controller = CreateController(todos: todos);

        var result = await controller.AddTodo("   ", CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(HomeController.TodoList), redirect.ActionName);
        Assert.Empty(todos.Items);
        Assert.Equal("Task text is required.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task AddTodo_WhenTitleIsTooLong_RedirectsWithoutAdding()
    {
        var todos = new FakeTodoRepository();
        var controller = CreateController(todos: todos);

        var result = await controller.AddTodo(new string('x', 201), CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(todos.Items);
        Assert.NotNull(controller.TempData["Error"]);
    }

    [Fact]
    public async Task AddTodo_WhenTitleIsValid_TrimsAndAddsItem()
    {
        var todos = new FakeTodoRepository();
        var controller = CreateController(todos: todos);

        var result = await controller.AddTodo("  Kupić mleko  ", CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        var item = Assert.Single(todos.Items);
        Assert.Equal("Kupić mleko", item.Title);
        Assert.NotEqual(default, item.CreatedAt);
    }
}

internal sealed class NullTempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

    public void SaveTempData(HttpContext context, IDictionary<string, object> values)
    {
    }
}
