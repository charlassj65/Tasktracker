using System.ComponentModel.DataAnnotations;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs;

public class CreateTaskRequest
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

    public DateTime? DueDate { get; set; }
}
