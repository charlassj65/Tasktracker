using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces;

public interface IAiSummaryProvider
{
    Task<string> SummarizeTasksAsync(IEnumerable<TaskItem> tasks);
}
