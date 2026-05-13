using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Application.Services;

public class TaskSummaryService : ITaskSummaryService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAiSummaryProvider _aiSummaryProvider;

    public TaskSummaryService(
        ITaskRepository taskRepository,
        IAiSummaryProvider aiSummaryProvider)
    {
        _taskRepository = taskRepository;
        _aiSummaryProvider = aiSummaryProvider;
    }

    public async Task<TaskSummaryResponse> SummarizeTodayTasksAsync()
    {
        var today = DateTime.UtcNow.Date;

        var tasks = await _taskRepository.GetTasksDueTodayAsync(today);

        if (!tasks.Any())
        {
            return new TaskSummaryResponse
            {
                Date = today,
                TotalTasks = 0,
                Summary = "You do not have any tasks due today."
            };
        }

        var summary = await _aiSummaryProvider.SummarizeTasksAsync(tasks);

        return new TaskSummaryResponse
        {
            Date = today,
            TotalTasks = tasks.Count,
            Summary = summary
        };
    }
}
