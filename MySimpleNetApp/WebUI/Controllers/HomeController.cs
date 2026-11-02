using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySimpleNetApp.WebUI.Data;
using MySimpleNetApp.WebUI.Models;

namespace MySimpleNetApp.WebUI.Controllers;

public class HomeController : Controller
{
    private const int MaxTitleLength = 200;

    private readonly ILogger<HomeController> _logger;
    private readonly ITodoRepository _todos;
    private readonly IDatabaseConnectionChecker _databaseStatus;

    public HomeController(
        ILogger<HomeController> logger,
        ITodoRepository todos,
        IDatabaseConnectionChecker databaseStatus)
    {
        _logger = logger;
        _todos = todos;
        _databaseStatus = databaseStatus;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["DatabaseConnected"] = await _databaseStatus.IsConnectedAsync(cancellationToken);
        return View();
    }

    public async Task<IActionResult> TodoList(CancellationToken cancellationToken)
    {
        var items = await _todos.GetAllAsync(cancellationToken);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTodo(string? title, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["Error"] = "Task text is required.";
            return RedirectToAction(nameof(TodoList));
        }

        if (title.Trim().Length > MaxTitleLength)
        {
            TempData["Error"] = $"Task text can be at most {MaxTitleLength} characters.";
            return RedirectToAction(nameof(TodoList));
        }

        await _todos.AddAsync(title, cancellationToken);
        return RedirectToAction(nameof(TodoList));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
