using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Infrastructure.AI;

public class SimpleTaskSummaryProvider : IAiSummaryProvider
{
    public Task<string> SummarizeTasksAsync(IEnumerable<TaskItem> tasks)
    {
        var taskList = tasks.ToList();

        var total = taskList.Count;
        var completed = taskList.Count(task => task.Status == TaskItemStatus.Done);
        var inProgress = taskList.Count(task => task.Status == TaskItemStatus.InProgress);
        var pending = taskList.Count(task => task.Status == TaskItemStatus.Todo);

        var overdue = taskList.Count(task =>
            task.DueDate.HasValue &&
            task.DueDate.Value.Date < DateTime.UtcNow.Date &&
            task.Status != TaskItemStatus.Done);

        var summary =
            $"You have {total} tasks today. " +
            $"{completed} completed, " +
            $"{inProgress} in progress, " +
            $"{pending} pending.";

        if (overdue > 0)
        {
            summary += $" {overdue} overdue tasks need attention.";
        }

        return Task.FromResult(summary);
    }
}
