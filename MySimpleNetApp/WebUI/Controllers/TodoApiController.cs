using Microsoft.AspNetCore.Mvc;
using MySimpleNetApp.WebUI.Data;
using MySimpleNetApp.WebUI.Models;

namespace MySimpleNetApp.WebUI.Controllers;

[ApiController]
[Route("api/todos")]
[Produces("application/json")]
public class TodoApiController(ITodoRepository todos) : ControllerBase
{
    // GET /api/todos
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TodoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TodoResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await todos.GetAllAsync(cancellationToken);
        return Ok(items.Select(TodoResponse.From));
    }

    // GET /api/todos/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await todos.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(TodoResponse.From(item));
    }

    // POST /api/todos
    [HttpPost]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoResponse>> Create(
        TodoCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            ModelState.AddModelError(nameof(request.Title), "Task text is required.");
            return ValidationProblem(ModelState);
        }

        var created = await todos.AddAsync(request.Title, cancellationToken);
        var response = TodoResponse.From(created);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // PUT /api/todos/{id}
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        TodoUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            ModelState.AddModelError(nameof(request.Title), "Task text is required.");
            return ValidationProblem(ModelState);
        }

        var updated = await todos.UpdateTitleAsync(id, request.Title, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    // DELETE /api/todos/{id}
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await todos.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
