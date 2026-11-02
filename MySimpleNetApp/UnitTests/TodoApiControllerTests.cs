using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySimpleNetApp.WebUI.Controllers;
using MySimpleNetApp.WebUI.Models;

namespace MySimpleNetApp.UnitTests;

public class TodoApiControllerTests
{
    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsOkWithEmptyList()
    {
        var controller = new TodoApiController(new FakeTodoRepository());

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<TodoResponse>>(ok.Value);
        Assert.Empty(items);
    }

    [Fact]
    public async Task Create_WhenTitleIsValid_ReturnsCreatedWithItem()
    {
        var controller = new TodoApiController(new FakeTodoRepository());

        var result = await controller.Create(
            new TodoCreateRequest { Title = "  nowe zadanie  " },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<TodoResponse>(created.Value);
        Assert.Equal("nowe zadanie", dto.Title);
        Assert.True(dto.Id > 0);
        Assert.Equal(nameof(TodoApiController.GetById), created.ActionName);
    }

    [Fact]
    public async Task Create_WhenTitleIsWhitespace_ReturnsValidationProblemForTitle()
    {
        var controller = new TodoApiController(new FakeTodoRepository());

        var result = await controller.Create(
            new TodoCreateRequest { Title = "   " },
            CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        var details = Assert.IsType<ValidationProblemDetails>(problem.Value);
        Assert.True(details.Errors.ContainsKey(nameof(TodoCreateRequest.Title)));
    }

    [Fact]
    public async Task Update_WhenItemExists_ReturnsNoContent()
    {
        var repository = new FakeTodoRepository();
        var item = await repository.AddAsync("stara nazwa");
        var controller = new TodoApiController(repository);

        var result = await controller.Update(
            item.Id,
            new TodoUpdateRequest { Title = "nowa nazwa" },
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("nowa nazwa", repository.Items.Single().Title);
    }

    [Fact]
    public async Task Update_WhenItemDoesNotExist_ReturnsNotFound()
    {
        var controller = new TodoApiController(new FakeTodoRepository());

        var result = await controller.Update(
            999,
            new TodoUpdateRequest { Title = "cokolwiek" },
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_WhenItemExists_ReturnsNoContent()
    {
        var repository = new FakeTodoRepository();
        var item = await repository.AddAsync("do usuniecia");
        var controller = new TodoApiController(repository);

        var result = await controller.Delete(item.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(repository.Items);
    }

    [Fact]
    public async Task Delete_WhenItemDoesNotExist_ReturnsNotFound()
    {
        var controller = new TodoApiController(new FakeTodoRepository());

        var result = await controller.Delete(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
