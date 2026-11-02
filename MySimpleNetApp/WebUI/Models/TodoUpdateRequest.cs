using System.ComponentModel.DataAnnotations;

namespace MySimpleNetApp.WebUI.Models;

public class TodoUpdateRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}
