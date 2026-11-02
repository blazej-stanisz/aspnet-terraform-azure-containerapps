using System.ComponentModel.DataAnnotations;

namespace MySimpleNetApp.WebUI.Models;

public class TodoCreateRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}
