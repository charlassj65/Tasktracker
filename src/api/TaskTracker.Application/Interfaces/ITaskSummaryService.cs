using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Interfaces;

public interface ITaskSummaryService
{
    Task<TaskSummaryResponse> SummarizeTodayTasksAsync();
}
