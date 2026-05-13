namespace TaskTracker.Application.DTOs;

public class TaskSummaryResponse
{
    public DateTime Date { get; set; }

    public int TotalTasks { get; set; }

    public string Summary { get; set; } = string.Empty;
}
