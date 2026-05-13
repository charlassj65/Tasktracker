using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs;

public class TaskResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }

    public DateTime? DueDate { get; set; }
}
